using GLogicECS.Collections;
using GLogicECS.Components.Common;

namespace GLogicECS.Components;

public struct OutputComponent
{
    public Entity Entity;
    public SmallList<ConnectionInfo> Outputs;

    public static int GetOutputsCount(IoType ioType)
    {
        return ioType switch
        {
            IoType.AND => 1,
            IoType.OR => 1,
            IoType.NOT => 1,
            IoType.XOR => 1,
            IoType.NAND => 1,
            IoType.NOR => 1,
            IoType.XNOR => 1,
            IoType.Input => 1,
            IoType.Output => 0,
            IoType.Wire => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(ioType), ioType, null)
        };
    }
}