using GLogicECS.Components;
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
                init.State
            ));

        LGateComponentSystem.Add(entity);

        return entity;
    }

    public static Entity CreateEntity(InitWire init)
    {
        var wireComponent = init.WireComponent;
        
        var entity = EntitySystem.CreateEntity(
            new InitEntity(
                init.TransformComponent,
                IoType.Wire,
                false
            ));

        WireComponentSystem.Add(entity,
            new InitWireComponent(
                wireComponent.Increasing, 
                wireComponent.InputHookNumber,
                wireComponent.OutputHookNumber)
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
