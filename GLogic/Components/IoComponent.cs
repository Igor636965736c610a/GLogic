using NetEscapades.EnumGenerators;

namespace GLogic.Components;

public struct IoComponent
{
    public Entity Entity;
    public Entity ConnectionOne;
    public Entity ConnectionTwo;
    public bool Value;
    public uint LastFrame;
}