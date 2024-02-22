using System.Diagnostics;
using GLogic.Components;
using GLogic.Components.Common;
using GLogic.Components.Init;
using GLogic.Components.System;

namespace GLogic.Jobs;

public static class EntityService
{
    public static Vector2Int RectLGateSize { get; }

    static EntityService()
    {
        RectLGateSize = new Vector2Int(100, 50);
    }
    
    public static void AddLGate(Vector2Int cursor, IoType ioType, bool value)
    {
        Debug.Assert(ioType != IoType.Wire);
        var initIoComponent = new InitIoComponentInfo(
            new Entity { Id = IdStructure.MakeInvalidId() },
            new Entity { Id = IdStructure.MakeInvalidId() }, 0, value);
        var position = new Vector2Int
        {
            X = (int)((cursor.X - Renderer.CameraShift.X) / Renderer.Zoom - (RectLGateSize.X / 2f)),
            Y = (int)((cursor.Y - Renderer.CameraShift.Y) / Renderer.Zoom - (RectLGateSize.Y / 2f)),
        };
        var initTransformComponent = new InitTransformComponentInfo(position, RectLGateSize);
        var initEntity = new InitEntityInfo(initTransformComponent, initIoComponent, ioType);
        var entity = EntityManager.CreateEntity(initEntity);
        var lGateArchetype = ArchetypeManager.CreateLGateArchetype(new InitLGateComponent { Entity = entity });
        EntityManager.CreateArchetypeForEntity(new InitArchetype
            {
                ArchetypeOption = ArchetypeOption.LGate, 
                Entity = entity,
                ArchetypeIndex = (int)IdStructure.Index(lGateArchetype.Id),
            });
    }

    private static Entity CheckArea(Vector2Int position) // Working
    {
        var entities = ArchetypeManager.IterLGateComponents();

        return EntityQuery.AABB_Entities(entities.Select(x => x.Entity), new Area(position, RectLGateSize))
            .FirstOrDefault(new Entity { Id = IdStructure.MakeInvalidId() });
    }
}