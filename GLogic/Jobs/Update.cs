using GLogic.Components;
using GLogic.Components.Common;

namespace GLogic.Jobs;

public static class Update
{
    public static void SetIoStates(uint frame)
    {
        foreach (var ioComponent in EntityManager.IterIoComponents())
        {
            var value = PenetrateIoComponent(ioComponent, frame);
            var toUpdate = ioComponent with { Value = value, LastFrame = ioComponent.LastFrame + 1 };
            EntityManager.UpdateIoComponent(toUpdate);
        }
    }

    private static bool PenetrateIoComponent(IoComponent ioComponent, uint frame)
    {
        if (!EntityManager.IsAlive(ioComponent.Entity))
        {
            return false;
        }
        if (ioComponent.LastFrame > frame)
        {
            return ioComponent.Value;
        }
        var input1 = EntityManager.GetIoComponent(ioComponent.Input1);
        var input2 = EntityManager.GetIoComponent(ioComponent.Input2);
        bool value = false;

        switch (ioComponent.IoType)
        {
            case IoType.AND:
            {
                value = PenetrateIoComponent(input1, frame) && PenetrateIoComponent(input2, frame);
                break;
            }
            case IoType.OR:
            {
                value = PenetrateIoComponent(input1, frame) || PenetrateIoComponent(input2, frame);
                break;
            }
            case IoType.NOT:
            {
                value = PenetrateIoComponent(input1, frame) != PenetrateIoComponent(input2, frame);
                break;
            }
            case IoType.XOR:
            {
                value = PenetrateIoComponent(input1, frame) ^ PenetrateIoComponent(input2, frame);
                break;
            }
            case IoType.NAND:
            {
                value = !(PenetrateIoComponent(input1, frame) && PenetrateIoComponent(input2, frame));
                break;
            }
            case IoType.NOR:
            {
                value = !(PenetrateIoComponent(input1, frame) || PenetrateIoComponent(input2, frame));
                break;
            }
            case IoType.XNOR:
            {
                value = !(PenetrateIoComponent(input1, frame) ^ PenetrateIoComponent(input2, frame));
                break;
            }
            case IoType.Input:
                return ioComponent.Value;
            case IoType.Output:
            {
                value = PenetrateIoComponent(input1, frame) && PenetrateIoComponent(input2, frame);
                break;
            }
            case IoType.Wire:
            {
                if (input1.Entity.Id != ioComponent.Entity.Id)
                {
                    value = PenetrateIoComponent(input1, frame);
                }
                else
                {
                    value = PenetrateIoComponent(input2, frame);
                }
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
        var toUpdate = ioComponent with { Value = value, LastFrame = ioComponent.LastFrame + 1 };
        EntityManager.UpdateIoComponent(toUpdate);
        
        return value;
    }
}