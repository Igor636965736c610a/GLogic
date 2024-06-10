using System.Runtime.CompilerServices;
using GLogic.Jobs.Renderer;
using GLogicECS.Components.Common;
using GLogicECS.Components.Interfaces;
using GLogicGlobal.Common;

namespace GLogic.Jobs;

public static class EntityQuery
{
    public static IEnumerable<T> AABB_Entities<T>(IEnumerable<T> entities, Area area) where T : struct, IAABBCompare
    {
        foreach (var entity in entities)
        {
            if (entity.Compare(Unsafe.As<Area, EcsArea>(ref area)))
            {
                yield return entity;
            }
        }
        
        // return entities.Where(entity => entity.Compare(Unsafe.As<Area, EcsArea>(ref area)));
    }

    public static IEnumerable<T> AABB_Entities<T>(IEnumerable<T> entities, Vector2Int point)
        where T : struct, IAABBCompare
    {
        foreach (var entity in entities)
        {
            if (entity.Compare(point))
            {
                yield return entity;
            }
        }
        
        // return entities.Where(entity => entity.Compare(point));
    }
}