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

public class UserActionExecutorInStepWiseSimMode : IUserActionExecutor
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
                CommonUserActionExecutor.AddLGate(adjustedCursorPosition, false, chosenMenuOption);

                break;
            case MenuOption.LowConstant:
            case MenuOption.HighConstant:
                CommonUserActionExecutor.AddLGate(adjustedCursorPosition, chosenMenuOption == MenuOption.HighConstant, chosenMenuOption);

                break;
            case MenuOption.Wire:
                var wire = EntityService.AddWire(adjustedCursorPosition);
                
                break;
            case MenuOption.Delete:
                EntityService.RemoveEntity(adjustedCursorPosition);

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