using GLogic.Jobs.Renderer;
using GLogicGlobal.Common;
using NetEscapades.EnumGenerators;

namespace GLogic.Data;

public sealed class LayoutArrangement
{
    public LayoutArrangement()
    {
        LeftPanelOptions = GetLeftPanelOptions().ToList();
        LeftPanelRect = new Area(new Vector2Int(0, 0), new Vector2Int(150, AppSettings.WindowSize.Y));
        TopPanelRect = new Area(new Vector2Int(150, 0), new Vector2Int(AppSettings.WindowSize.Y - 150, 100));
    }

    public IEnumerable<Area> LeftPanelOptions { get; }
    public Area LeftPanelRect { get; }
    public Area TopPanelRect { get; }
    
    private static IEnumerable<Area> GetLeftPanelOptions()
    {
        for (var i = 0; i <= Enum.GetNames(typeof(LeftPanelOptions)).Length - 2; i++)
        {
            var y = 30 + 55 * i;
            yield return new Area(new Vector2Int(10, y), new Vector2Int(130, 55));
        }
    }
}

[EnumExtensions]
public enum LeftPanelOptions
{
    AND,
    OR,
    NOT,
    XOR,
    NAND,
    NOR,
    XNOR,
    LowConstant,
    HighConstant,
    LedOutput,
    Wire,
    Delete,
    None
}