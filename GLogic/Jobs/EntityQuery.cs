using GLogic.Components;
using GLogic.Components.Common;
using GLogic.Components.System;

namespace GLogic.Jobs;

public static class EntityQuery
{
    public static IEnumerable<Entity> AABB_Entities(IEnumerable<Entity> entities, Area area)
    {
        foreach (var entity in entities)
        {
            var transformComponent = EntityManager.GetTransformComponent(entity);

            if (!EntityManager.IsAlive(transformComponent.Entity))
            {
                continue;
            }

            if (transformComponent.Position.X < area.Position.X + area.Size.X
                && transformComponent.Position.X + transformComponent.Size.X > area.Position.X
                && transformComponent.Position.Y < area.Position.Y + area.Size.Y
                && transformComponent.Position.Y + transformComponent.Size.Y > area.Position.Y) //AABB
            {
                yield return transformComponent.Entity;
            }
        }
    }

    public static IEnumerable<Entity> AABB_Entities(IEnumerable<Entity> entities, Vector2Int point)
    {
        foreach (var entity in entities)
        {
            var transformComponent = EntityManager.GetTransformComponent(entity);

            if (!EntityManager.IsAlive(transformComponent.Entity))
            {
                continue;
            }

            if (transformComponent.Position.X < point.X
                && transformComponent.Position.X + transformComponent.Size.X > point.X
                && transformComponent.Position.Y < point.Y
                && transformComponent.Position.Y + transformComponent.Size.Y > point.Y) //AABB
            {
                yield return transformComponent.Entity;
            }
        }
    }

    public static Entity AABB_Entity(IEnumerable<Entity> entities, Area area)
    {
        foreach (var entity in entities)
        {
            var transformComponent = EntityManager.GetTransformComponent(entity);

            if (!EntityManager.IsAlive(transformComponent.Entity))
            {
                continue;
            }

            if (transformComponent.Position.X < area.Position.X + area.Size.X
                && transformComponent.Position.X + transformComponent.Size.X > area.Position.X
                && transformComponent.Position.Y < area.Position.Y + area.Size.Y
                && transformComponent.Position.Y + transformComponent.Size.Y > area.Position.Y) //AABB
            {
                return transformComponent.Entity;
            }
        }

        return new Entity { Id = IdStructure.MakeInvalidId() };
    }

    public static Entity AABB_Entity(IEnumerable<Entity> entities, Vector2Int point)
    {
        foreach (var entity in entities)
        {
            var transformComponent = EntityManager.GetTransformComponent(entity);

            if (!EntityManager.IsAlive(transformComponent.Entity))
            {
                continue;
            }

            if (transformComponent.Position.X < point.X
                && transformComponent.Position.X + transformComponent.Size.X > point.X
                && transformComponent.Position.Y < point.Y
                && transformComponent.Position.Y + transformComponent.Size.Y > point.Y) //AABB
            {
                return transformComponent.Entity;
            }
        }

        return new Entity { Id = IdStructure.MakeInvalidId() };
    }
}