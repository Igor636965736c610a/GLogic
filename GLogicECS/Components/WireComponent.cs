using System.ComponentModel;
using System.Diagnostics;
using GLogicECS.Api;
using GLogicECS.Components.Common;
using GLogicECS.Components.Interfaces;
using GLogicGlobal.Common;

namespace GLogicECS.Components;

public struct WireComponent : IAABBCompare
{
    public Entity Entity;
    [Description("Input")] public Vector2Int P1;
    [Description("Output")] public Vector2Int P2;

    public WireComponent()
    {
        Entity = new Entity(IdStructure.MakeInvalidId());
        P1 = default;
        P2 = default;
    }

    public bool Compare(EcsArea area)
    {
        var transformComponent = ComponentManager.GetTransformComponent(Entity);

        Debug.Assert(EntityManager.IsAlive(transformComponent.Entity));

        return transformComponent.Position.X < area.Position.X + area.Size.X
               && transformComponent.Position.X + transformComponent.Size.X > area.Position.X
               && transformComponent.Position.Y < area.Position.Y + area.Size.Y
               && transformComponent.Position.Y + transformComponent.Size.Y > area.Position.Y;
    }

    public bool Compare(Vector2Int point)
    {
        var transformComponent = ComponentManager.GetTransformComponent(Entity);

        Debug.Assert(EntityManager.IsAlive(transformComponent.Entity));

        return transformComponent.Position.X < point.X
               && transformComponent.Position.X + transformComponent.Size.X > point.X
               && transformComponent.Position.Y < point.Y
               && transformComponent.Position.Y + transformComponent.Size.Y > point.Y;
    }

    public Entity GetEntity()
        => Entity;
}

/*
 
Wire:
InputHookNumber0   -----------------  OutputHookNumber0

*/