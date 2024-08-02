using GLogic.Jobs.Renderer;
using GLogicGlobal.Common;
using NetEscapades.EnumGenerators;

namespace GLogic.Data.Panels;

public sealed class LeftPanel
{
    public LeftPanel()
    {
        Rect = new Area(new Vector2Int(0, 0), new Vector2Int(150, AppSettings.WindowSize.Y));
        Options = InitLeftPanelOptions().ToList();
    }
    
    public Area Rect { get; }
    public IEnumerable<PanelOption<LeftPanelOption>> Options { get; }

    public LeftPanelOption? GetClickedOption(Vector2Int cursor)
    {
        foreach (var (optionRect, option) in Options)
        {
            if (cursor.X >= optionRect.Position.X
                && cursor.X <= optionRect.Position.X + optionRect.Size.X
                && cursor.Y >= optionRect.Position.Y
                && cursor.Y < optionRect.Position.Y + optionRect.Size.Y)
            {
                return option;
            }
        }

        return null;
    }

    private static IEnumerable<PanelOption<LeftPanelOption>> InitLeftPanelOptions()
    {
        for (var i = 0; i < Enum.GetNames(typeof(LeftPanelOption)).Length - 1; i++)
        {
            var y = 30 + 55 * i;
            yield return new PanelOption<LeftPanelOption>(new Area(new Vector2Int(10, y), new Vector2Int(130, 55)), (LeftPanelOption)i);
        }
    }
}


[EnumExtensions]
public enum LeftPanelOption
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