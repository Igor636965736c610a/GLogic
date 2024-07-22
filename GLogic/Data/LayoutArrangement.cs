using GLogic.Data.Panels;
using NetEscapades.EnumGenerators;

namespace GLogic.Data;

public sealed class LayoutArrangement
{
    public LayoutArrangement()
    {
        LeftPanel = new LeftPanel();
        TopPanel = new TopPanel();
    }

    public LeftPanel LeftPanel { get; }
    public TopPanel TopPanel { get; }
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