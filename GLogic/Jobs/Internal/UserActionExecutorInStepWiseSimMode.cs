using System.Diagnostics;
using GLogic.Jobs.Internal.EcsStateModifiers;
using GLogic.Jobs.Internal.EcsStateModifiers.LogicCircuitUpdates;
using GLogic.Jobs.Internal.EcsStateModifiers.LogicCircuitUpdates.Simulations;
using GLogic.Jobs.Renderer;
using GLogicECS.Api;
using GLogicECS.Components;
using GLogicECS.Components.Common;
using GLogicGlobal.Common;

namespace GLogic.Jobs.Internal;

internal sealed class UserActionExecutorInStepWiseSimMode : IUserActionExecutor
{
    private readonly IStepWiseSimulationModifier _stepWiseSimulationModifier;

    public UserActionExecutorInStepWiseSimMode(IStepWiseSimulationModifier stepWiseSimulationModifier)
    {
        _stepWiseSimulationModifier = stepWiseSimulationModifier;
        LGateToMove = new Entity(IdStructure.MakeInvalidId());
    }
    
    public Entity LGateToMove { get; set; }

    public void ClickExecute(Vector2Int adjustedCursorPosition, MenuOption chosenMenuOption)
    {
        switch (chosenMenuOption)
        {
            case MenuOption.AND:
            case MenuOption.OR:
            case MenuOption.NOT:
            case MenuOption.XOR:
            case MenuOption.NAND:
            case MenuOption.NOR:
            case MenuOption.XNOR:
            case MenuOption.LedOutput:
                var lGate = CommonUserActionExecutor.AddLGate(adjustedCursorPosition, false, chosenMenuOption);
                if (lGate is not null)
                {
                    _stepWiseSimulationModifier.AddToSimulationQueue(lGate.Value);
                }
                
                break;
            case MenuOption.LowConstant:
            case MenuOption.HighConstant:
                CommonUserActionExecutor.AddLGate(adjustedCursorPosition, chosenMenuOption == MenuOption.HighConstant, chosenMenuOption);

                break;
            case MenuOption.Wire:
                var wire = EntityService.AddWire(adjustedCursorPosition);
                if (wire is not null)
                {
                    _stepWiseSimulationModifier.AddToSimulationQueue(wire.Value);
                }
                
                break;
            case MenuOption.Delete:
                var entityToDelete = EntityService.GetEntityToDelete(adjustedCursorPosition);
                if (!IdStructure.IsValid(entityToDelete.Id))
                {
                    return;
                }

                var outputWires = ComponentManager.GetOutputComponent(entityToDelete).Outputs;
                for (int i = 0; i < outputWires.Count; i++)
                {
                    Debug.Assert(EntityManager.IsAlive(outputWires[i].Entity));
                    var wireOutput = ComponentManager.GetOutputComponent(outputWires[i].Entity);
                    _stepWiseSimulationModifier.AddToSimulationQueue(wireOutput.Outputs[0].Entity);
                }
                
                EntityService.RemoveEntity(entityToDelete);

                break;
            case MenuOption.None:
                LGateToMove = CommonUserActionExecutor.MarkEntity(adjustedCursorPosition);
                
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public void HeldExecute(Vector2Int adjustedCursorPosition)
    {
        if (!IdStructure.IsValid(LGateToMove.Id))
        {
            return;
        }
        
        var info = EntityService.GetDynamicLGateParamsToRender(
            adjustedCursorPosition,
            ComponentManager.IterLGateComponents().Where(z => z.Entity.Id != LGateToMove.Id)
        );

        if (info.placement != Placement.Valid)
        {
            return;
        }

        EntityService.UpdateEntityPosition(LGateToMove, info.position);
        WireService.UpdateConnectedWiresPosition(LGateToMove);
    }
}