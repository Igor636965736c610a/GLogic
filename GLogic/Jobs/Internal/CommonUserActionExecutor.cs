using GLogic.Data;
using GLogic.Jobs.Internal.EcsStateModifiers;
using GLogic.Jobs.Renderer;
using GLogicECS.Api;
using GLogicECS.Components;
using GLogicECS.Components.Common;
using GLogicGlobal.Common;

namespace GLogic.Jobs.Internal;

internal static class CommonUserActionExecutor
{
    public static Entity? AddLGate(Vector2Int adjustedCursorPosition, bool lGateValue, LeftPanelOptions chosenLeftPanelOptions)
    {
        var info = EntityService.GetDynamicLGateParamsToRender(
            adjustedCursorPosition,
            ComponentManager.IterLGateComponents()
        );

        if (info.placement == Placement.Valid)
        {
            return EntityService.AddLGate(
                info.position, GetLGateTypeFromMenuOption(chosenLeftPanelOptions), lGateValue
            );
        }

        return null;
    }

    public static Entity MarkEntity(Vector2Int adjustedCursorPosition)
    {
        var markedEntity = EntityQuery.AABB_Entities(
            ComponentManager.IterLGateComponents(),
            adjustedCursorPosition
        ).FirstOrDefault(new LGateComponent(new Entity(IdStructure.MakeInvalidId()))).Entity;

        Console.WriteLine(markedEntity.Id);
        return markedEntity;
    }

    private static IoType GetLGateTypeFromMenuOption(LeftPanelOptions leftPanelOptions)
    {
        return leftPanelOptions switch
        {
            LeftPanelOptions.AND => IoType.AND,
            LeftPanelOptions.OR => IoType.OR,
            LeftPanelOptions.NOT => IoType.NOT,
            LeftPanelOptions.XOR => IoType.XOR,
            LeftPanelOptions.NAND => IoType.NAND,
            LeftPanelOptions.NOR => IoType.NOR,
            LeftPanelOptions.XNOR => IoType.XNOR,
            LeftPanelOptions.LowConstant => IoType.Constant,
            LeftPanelOptions.HighConstant => IoType.Constant,
            LeftPanelOptions.LedOutput => IoType.LedOutput,
            _ => throw new ArgumentOutOfRangeException(nameof(leftPanelOptions), leftPanelOptions, null)
        };
    }
}