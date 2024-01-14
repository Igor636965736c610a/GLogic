namespace GLogic.Components;

public struct IoComponent
{
    public Entity Entity;
    public Entity Input1;
    public Entity Input2;
    public bool Value;
    public IoType IoType;
    public uint LastFrame;
}
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