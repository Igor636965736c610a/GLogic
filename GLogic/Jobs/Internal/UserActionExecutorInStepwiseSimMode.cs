using System.Diagnostics;
using GLogic.Jobs.Internal.EcsStateModifiers;
using GLogic.Jobs.Internal.EcsStateModifiers.LogicCircuitUpdates.Simulations;
using GLogic.Jobs.Renderer;
using GLogicECS.Api;
using GLogicECS.Components;
using GLogicECS.Components.Common;
using GLogicGlobal.Common;

namespace GLogic.Jobs.Internal;

internal sealed class UserActionExecutorInStepwiseSimMode : IUserActionExecutor
{
    private readonly IStepwiseSimulationModifier _stepwiseSimulationModifier;

    public UserActionExecutorInStepwiseSimMode(IStepwiseSimulationModifier stepwiseSimulationModifier)
    {
        _stepwiseSimulationModifier = stepwiseSimulationModifier;
        HeldEntity = new Entity(IdStructure.MakeInvalidId());
    }

    public Entity HeldEntity { get; set; }

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
                    _stepwiseSimulationModifier.AddToSimulationQueue(lGate.Value);
                }

                break;
            case MenuOption.LowConstant:
            case MenuOption.HighConstant:
                CommonUserActionExecutor.AddLGate(adjustedCursorPosition, chosenMenuOption == MenuOption.HighConstant,
                    chosenMenuOption);

                break;
            case MenuOption.Wire:
                var wire = EntityService.AddWire(adjustedCursorPosition);
                if (wire is not null)
                {
                    Debug.Assert(ComponentManager.GetOutputComponent(wire.Value).Outputs.Count == 1);

                    var outputEntity = ComponentManager.GetOutputComponent(wire.Value).Outputs[0].Entity;
                    _stepwiseSimulationModifier.AddToSimulationQueue(outputEntity);
                }

                break;
            case MenuOption.Delete:
                var entityToDelete = EntityService.GetEntityToDelete(adjustedCursorPosition);
                if (!IdStructure.IsValid(entityToDelete.Id))
                {
                    return;
                }

                var outputWires = ComponentManager.GetOutputComponent(entityToDelete).Outputs;
                for (var i = 0; i < outputWires.Count; i++)
                {
                    Debug.Assert(EntityManager.IsAlive(outputWires[i].Entity));
                    var wireOutput = ComponentManager.GetOutputComponent(outputWires[i].Entity);
                    _stepwiseSimulationModifier.AddToSimulationQueue(wireOutput.Outputs[0].Entity);
                }

                EntityService.RemoveEntity(entityToDelete);

                break;
            case MenuOption.None:
                HeldEntity = CommonUserActionExecutor.MarkEntity(adjustedCursorPosition);

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void HeldExecute(Vector2Int adjustedCursorPosition)
    {
        if (!IdStructure.IsValid(HeldEntity.Id))
        {
            return;
        }

        var info = EntityService.GetDynamicLGateParamsToRender(
            adjustedCursorPosition,
            ComponentManager.IterLGateComponents().Where(z => z.Entity.Id != HeldEntity.Id)
        );

        if (info.placement != Placement.Valid)
        {
            return;
        }

        EntityService.UpdateEntityPosition(HeldEntity, info.position);
        WireService.UpdateConnectedWiresPosition(HeldEntity);
    }
}