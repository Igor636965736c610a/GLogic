using System.Diagnostics;
using GLogicECS.Api;
using GLogicECS.Components.Common;
using GLogicECS.Components.Interfaces;
using GLogicGlobal.Common;

namespace GLogicECS.Components;

public struct LGateComponent : IAABBCompare
{
    public Entity Entity;

    public LGateComponent()
    {
        Entity = new Entity(IdStructure.MakeInvalidId());
    }

    public LGateComponent(Entity entity)
    {
        Entity = entity;
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
        //AABB
    }

    public Entity GetEntity()
        => Entity;
}

/*
 
Gate:
InputHookNumber0   ---|----+
                      |------+
                      |---------+
                      |-----------+-  OutputHookNumber0
                      |---------+
                      |------+
InputHookNumber1   ---|----+ 

*/