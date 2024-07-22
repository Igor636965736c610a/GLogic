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

public sealed class UserActionExecutorInInstantSimMode : IUserActionExecutor
{
    private readonly IInstantSimulationModifier _instantSimulationModifier;

    public UserActionExecutorInInstantSimMode(IInstantSimulationModifier instantSimulationModifier)
    {
        _instantSimulationModifier = instantSimulationModifier;
        HeldEntity = new Entity(IdStructure.MakeInvalidId());
    }

    public Entity HeldEntity { get; set; }

    public void ClickExecute(Vector2Int adjustedCursorPosition, LeftPanelOption chosenLeftPanelOption)
    {
        switch (chosenLeftPanelOption)
        {
            case LeftPanelOption.AND:
            case LeftPanelOption.OR:
            case LeftPanelOption.NOT:
            case LeftPanelOption.XOR:
            case LeftPanelOption.NAND:
            case LeftPanelOption.NOR:
            case LeftPanelOption.XNOR:
            case LeftPanelOption.LedOutput:
            case LeftPanelOption.LowConstant:
            case LeftPanelOption.HighConstant:
                var lGate = CommonUserActionExecutor.AddLGate(adjustedCursorPosition,
                    chosenLeftPanelOption == LeftPanelOption.HighConstant, chosenLeftPanelOption);
                if (lGate is not null)
                {
                    _instantSimulationModifier.IncreaseEntityStatesStorage();
                }

                break;
            case LeftPanelOption.Wire:
                var wire = EntityService.AddWire(adjustedCursorPosition);
                if (wire is not null)
                {
                    Debug.Assert(ComponentManager.GetOutputComponent(wire.Value).Outputs.Count == 1);

                    _instantSimulationModifier.IncreaseEntityStatesStorage();
                }

                break;
            case LeftPanelOption.Delete:
                var entityToDelete = EntityService.GetEntityToDelete(adjustedCursorPosition);
                if (!IdStructure.IsValid(entityToDelete.Id))
                {
                    return;
                }

                EntityService.RemoveEntity(entityToDelete);

                break;
            case LeftPanelOption.None:
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