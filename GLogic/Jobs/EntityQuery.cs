using System.Diagnostics;
using GLogic.Components.Common;
using GLogic.Jobs.Renderer;
using GLogicECS.Api;
using GLogicECS.Components;
using GLogicECS.Components.System;

namespace GLogic.Jobs;

public static class EntityQuery
{
    public static IEnumerable<T> AABB_Entities<T>(IEnumerable<T> entities, Area area) where T : IAABBCompare
    {
        return entities.Where(entity => entity.Compare(area.ToEcsArea()));
    }

    public static IEnumerable<T> AABB_Entities<T>(IEnumerable<T> entities, Vector2Int point) where T : IAABBCompare
    {
        return entities.Where(entity => entity.Compare(point));
    }
    
    // public static IEnumerable<Entity> AABB_Entities(IEnumerable<Entity> entities, Area area)
    // {
    //     foreach (var entity in entities)
    //     {
    //         var transformComponent = ComponentManager.GetTransformComponent(entity);
    //     
    //         Debug.Assert(EntityManager.IsAlive(transformComponent.Entity));
    //         // if (!EntityManager.IsAlive(transformComponent.Entity))
    //         // {
    //         //     continue;
    //         // }
    //     
    //         if (transformComponent.Position.X < area.Position.X + area.Size.X
    //             && transformComponent.Position.X + transformComponent.Size.X > area.Position.X
    //             && transformComponent.Position.Y < area.Position.Y + area.Size.Y
    //             && transformComponent.Position.Y + transformComponent.Size.Y > area.Position.Y) //AABB
    //         {
    //             yield return transformComponent.Entity;
    //         }
    //     }
    // }
    //
    // public static IEnumerable<Entity> AABB_Entities(IEnumerable<Entity> entities, Vector2Int point)
    // {
    //     foreach (var entity in entities)
    //     {
    //         var transformComponent = ComponentManager.GetTransformComponent(entity);
    //
    //         Debug.Assert(EntityManager.IsAlive(transformComponent.Entity));
    //         // if (!EntityManager.IsAlive(transformComponent.Entity))
    //         // {
    //         //     continue;
    //         // }
    //
    //         if (transformComponent.Position.X < point.X
    //             && transformComponent.Position.X + transformComponent.Size.X > point.X
    //             && transformComponent.Position.Y < point.Y
    //             && transformComponent.Position.Y + transformComponent.Size.Y > point.Y) //AABB
    //         {
    //             yield return transformComponent.Entity;
    //         }
    //     }
    // }
}