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
                continue;
            var center = new Vector2Int
            {
                X = (int)(transformComponent.Position.X * Renderer.Zoom + Renderer.CameraShift.X + transformComponent.Size.X / 2f * Renderer.Zoom),
                Y = (int)(transformComponent.Position.Y * Renderer.Zoom + Renderer.CameraShift.Y + transformComponent.Size.Y / 2f * Renderer.Zoom),
            };
            if (transformComponent.Position.X < area.Position.X + area.Size.X 
                && transformComponent.Position.X + transformComponent.Size.X > area.Position.X 
                && transformComponent.Position.Y < area.Position.Y + area.Size.Y
                && transformComponent.Position.Y + transformComponent.Size.Y > area.Position.Y) //AABB
            {
                yield return transformComponent.Entity;
            }
        }
    }
}