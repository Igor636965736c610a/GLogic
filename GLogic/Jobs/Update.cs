using GLogic.Components;
using GLogic.Components.System;

namespace GLogic.Jobs;

public static class Update
{
    public static void SetIoStates(uint frame)
    {
        foreach (var ioComponent in EntityManager.IterIoComponents())
        {
            PenetrateIoComponent(ioComponent, frame);
        }
    }

    private static bool PenetrateIoComponent(IoComponent ioComponent, uint frame)
    {
        if (!EntityManager.IsAlive(ioComponent.Entity))
        {
            return false;
        }
        if (ioComponent.LastFrame == frame + 1)
        {
            return ioComponent.Value;
        }
        var input1 = EntityManager.GetIoComponent(ioComponent.ConnectionOne);
        var input2 = EntityManager.GetIoComponent(ioComponent.ConnectionTwo);
        bool value;
        var earlyUpdate = ioComponent with { LastFrame = frame + 1 };
        EntityManager.UpdateIoComponent(earlyUpdate);
        var entityType = EntityManager.GetEntityTypeComponent(ioComponent.Entity);

        switch (entityType.Type)
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
        var lateUpdate = ioComponent with { Value = value };
        EntityManager.UpdateIoComponent(lateUpdate);
        
        return value;
    }
}