using NetEscapades.EnumGenerators;

namespace GLogic.Components;

public struct EntityTypeComponent
{
    public Entity Entity;
    public IoType Type;
}

[EnumExtensions]
public enum IoType
{
    //textured
    AND,
    OR,
    NOT,
    XOR,
    NAND,
    NOR,
    XNOR,
    Input,
    Output,
    //textured
    
    Wire
}