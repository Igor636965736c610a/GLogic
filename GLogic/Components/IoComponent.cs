using NetEscapades.EnumGenerators;

namespace GLogic.Components;

public struct IoComponent
{
    public Entity Entity;
    public Entity TopInput;
    public Entity BottomInput;
    public bool Value;
    public IoType IoType;
    public uint LastFrame;
}

[EnumExtensions]
public enum IoType
{
    AND,
    OR,
    NOT,
    XOR,
    NAND,
    NOR,
    XNOR,
    Input,
    Output,
    Wire
}