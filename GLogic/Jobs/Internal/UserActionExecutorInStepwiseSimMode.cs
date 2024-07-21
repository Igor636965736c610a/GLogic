using System.Diagnostics;
using GLogic.Data;
using GLogic.Jobs.Internal.EcsStateModifiers;
using GLogic.Jobs.Internal.EcsStateModifiers.Simulations;
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

    public void ClickExecute(Vector2Int adjustedCursorPosition, LeftPanelOptions chosenLeftPanelOptions)
    {
        switch (chosenLeftPanelOptions)
        {
            case LeftPanelOptions.AND:
            case LeftPanelOptions.OR:
            case LeftPanelOptions.NOT:
            case LeftPanelOptions.XOR:
            case LeftPanelOptions.NAND:
            case LeftPanelOptions.NOR:
            case LeftPanelOptions.XNOR:
            case LeftPanelOptions.LedOutput:
                var lGate = CommonUserActionExecutor.AddLGate(adjustedCursorPosition, false, chosenLeftPanelOptions);
                if (lGate is not null)
                {
                    _stepwiseSimulationModifier.AddToSimulationQueue(lGate.Value);
                }

                break;
            case LeftPanelOptions.LowConstant:
            case LeftPanelOptions.HighConstant:
                CommonUserActionExecutor.AddLGate(adjustedCursorPosition, chosenLeftPanelOptions == LeftPanelOptions.HighConstant,
                    chosenLeftPanelOptions);

                break;
            case LeftPanelOptions.Wire:
                var wire = EntityService.AddWire(adjustedCursorPosition);
                if (wire is not null)
                {
                    Debug.Assert(ComponentManager.GetOutputComponent(wire.Value).Outputs.Count == 1);

                    var outputEntity = ComponentManager.GetOutputComponent(wire.Value).Outputs[0].Entity;
                    _stepwiseSimulationModifier.AddToSimulationQueue(outputEntity);
                }

                break;
            case LeftPanelOptions.Delete:
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
            case LeftPanelOptions.None:
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