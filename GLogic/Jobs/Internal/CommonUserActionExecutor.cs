using GLogic.Data;
using GLogic.Data.Panels;
using GLogic.Data.TextureStorage;
using GLogic.Jobs.Internal.EcsStateModifiers;
using GLogic.Jobs.Renderer;
using GLogicECS.Api;
using GLogicECS.Components;
using GLogicECS.Components.Common;
using GLogicGlobal.Common;

namespace GLogic.Jobs.Internal;

internal static class CommonUserActionExecutor
{
    public static Entity? AddLGate(Vector2Int adjustedCursorPosition, bool lGateValue, LeftPanelOption chosenLeftPanelOption)
    {
        var info = EntityService.GetDynamicLGateParamsToRender(
            adjustedCursorPosition,
            ComponentManager.IterLGateComponents()
        );

        if (info.placement == LGatePlacementF.Valid)
        {
            return EntityService.AddLGate(
                info.position, GetLGateTypeFromMenuOption(chosenLeftPanelOption), lGateValue
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

    private static IoType GetLGateTypeFromMenuOption(LeftPanelOption leftPanelOption)
    {
        return leftPanelOption switch
        {
            LeftPanelOption.AND => IoType.AND,
            LeftPanelOption.OR => IoType.OR,
            LeftPanelOption.NOT => IoType.NOT,
            LeftPanelOption.XOR => IoType.XOR,
            LeftPanelOption.NAND => IoType.NAND,
            LeftPanelOption.NOR => IoType.NOR,
            LeftPanelOption.XNOR => IoType.XNOR,
            LeftPanelOption.LowConstant => IoType.Constant,
            LeftPanelOption.HighConstant => IoType.Constant,
            LeftPanelOption.LedOutput => IoType.LedOutput,
            _ => throw new ArgumentOutOfRangeException(nameof(leftPanelOption), leftPanelOption, null)
        };
    }
}