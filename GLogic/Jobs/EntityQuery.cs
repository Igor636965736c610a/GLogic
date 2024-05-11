using GLogic.Jobs.Renderer;
using GLogicECS.Components.Interfaces;
using GLogicGlobal.Common;

namespace GLogic.Jobs;

public static class EntityQuery
{
    public static IEnumerable<T> AABB_Entities<T>(IEnumerable<T> entities, Area area) where T : struct, IAABBCompare
    {
        return entities.Where(entity => entity.Compare(area.ToEcsArea()));
    }

    public static IEnumerable<T> AABB_Entities<T>(IEnumerable<T> entities, Vector2Int point)
        where T : struct, IAABBCompare
    {
        return entities.Where(entity => entity.Compare(point));
    }
}