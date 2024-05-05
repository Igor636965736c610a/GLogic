using GLogicECS.Collections;
using GLogicECS.Components.Common;

namespace GLogicECS.Components;

public struct InputComponent
{
    public Entity Entity;
    public SmallList<ConnectionInfo> Inputs;

    public static int GetInputsCount(IoType ioType)
    {
        return ioType switch
        {
            IoType.AND => 2,
            IoType.OR => 2,
            IoType.NOT => 2,
            IoType.XOR => 2,
            IoType.NAND => 2,
            IoType.NOR => 2,
            IoType.XNOR => 2,
            IoType.Input => 0,
            IoType.Output => 2,
            IoType.Wire => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(ioType), ioType, null)
        };
    }
}