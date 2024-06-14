using GLogic.Jobs.Internal.EcsStateModifiers;
using GLogic.Jobs.Renderer;
using GLogicECS.Api;
using GLogicECS.Components;
using GLogicECS.Components.Common;
using GLogicGlobal.Common;

namespace GLogic.Jobs.Internal;

internal static class CommonUserActionExecutor
{
    public static Entity? AddLGate(Vector2Int adjustedCursorPosition, bool lGateValue, MenuOption chosenMenuOption)
    {
        var info = EntityService.GetDynamicLGateParamsToRender(
            adjustedCursorPosition,
            ComponentManager.IterLGateComponents()
        );

        if (info.placement == Placement.Valid)
        {
            return EntityService.AddLGate(
                info.position, GetLGateTypeFromMenuOption(chosenMenuOption), lGateValue
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

    private static IoType GetLGateTypeFromMenuOption(MenuOption menuOption)
    {
        return menuOption switch
        {
            MenuOption.AND => IoType.AND,
            MenuOption.OR => IoType.OR,
            MenuOption.NOT => IoType.NOT,
            MenuOption.XOR => IoType.XOR,
            MenuOption.NAND => IoType.NAND,
            MenuOption.NOR => IoType.NOR,
            MenuOption.XNOR => IoType.XNOR,
            MenuOption.LowConstant => IoType.Constant,
            MenuOption.HighConstant => IoType.Constant,
            MenuOption.LedOutput => IoType.LedOutput,
            _ => throw new ArgumentOutOfRangeException(nameof(menuOption), menuOption, null)
        };
    }
}