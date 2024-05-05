using GLogicECS.Collections;
using GLogicECS.Components;
using GLogicECS.Components.Common;
using GLogicECS.Components.Init;
using GLogicECS.Components.System;
using GLogicECS.Components.System.DoubleIndexing;

namespace GLogicECS.Api;

public static class EntityManager
{
    public static Entity CreateEntity(InitLGate init)
    {
        if (init.IoType == IoType.Wire)
        {
            throw new ArgumentException("IoType cant be Wire");
        }

        var entity = EntitySystem.CreateEntity(
            new InitEntity(
                init.TransformComponent,
                init.IoType,
                init.State,
                default
            ));

        LGateComponentSystem.Add(entity);

        return entity;
    }

    public static Entity CreateEntity(InitWire init)
    {
        // Implement ICollection to Small List to avoid that
        var inputs = new SmallList<ConnectionInfo>();
        inputs.Add(new ConnectionInfo(init.Input.Entity, init.Input.HookNumber));
        var outputs = new SmallList<ConnectionInfo>();
        outputs.Add(new ConnectionInfo(init.Output.Entity, init.Output.HookNumber));

        var initInputOutputComp = new InitInputsOutputs(
            inputs,
            outputs
        );

        var entity = EntitySystem.CreateEntity(
            new InitEntity(
                init.TransformComponent,
                IoType.Wire,
                false,
                initInputOutputComp
            ));

        WireComponentSystem.Add(entity,
            init.WireComponent
        );

        return entity;
    }

    public static void RemoveEntity(Entity entity)
    {
        LGateComponentSystem.Remove(entity);
        WireComponentSystem.Remove(entity);
        EntitySystem.RemoveEntity(entity);
    }

    public static bool IsAlive(Entity entity)
        => EntitySystem.IsAlive(entity);
}