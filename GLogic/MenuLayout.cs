using System.Collections.Immutable;
using System.Numerics;
using GLogic.Components.Common;
using NetEscapades.EnumGenerators;

namespace GLogic;

public static class MenuLayout
{
    public static ImmutableArray<MenuCheckRect> MenuOptions = ImmutableArray.Create(new[]
    {
        new MenuCheckRect(new Vector2Int(10, 30), new Vector2Int(130, 60)),
        new MenuCheckRect(new Vector2Int(10, 85), new Vector2Int(130, 60)),
        new MenuCheckRect(new Vector2Int(10, 140), new Vector2Int(130, 60)),
        new MenuCheckRect(new Vector2Int(10, 195), new Vector2Int(130, 60)),
        new MenuCheckRect(new Vector2Int(10, 250), new Vector2Int(130, 60)),
        new MenuCheckRect(new Vector2Int(10, 305), new Vector2Int(130, 60)),
        new MenuCheckRect(new Vector2Int(10, 360), new Vector2Int(130, 60)),
        new MenuCheckRect(new Vector2Int(10, 415), new Vector2Int(130, 60)),
        new MenuCheckRect(new Vector2Int(10, 470), new Vector2Int(130, 60)),
        new MenuCheckRect(new Vector2Int(10, 525), new Vector2Int(130, 60)),
        new MenuCheckRect(new Vector2Int(10, 580), new Vector2Int(130, 60)),
        new MenuCheckRect(new Vector2Int(10, 635), new Vector2Int(130, 60)),
    });
}

public record MenuCheckRect(Vector2Int Position, Vector2Int Size);


[EnumExtensions]
public enum CurrentMenuOption
{
    AND,
    OR,
    NOT,
    XOR,
    NAND,
    NOR,
    XNOR,
    Input1,
    Input0,
    Output,
    Wire,
    Delete,
    None,
}