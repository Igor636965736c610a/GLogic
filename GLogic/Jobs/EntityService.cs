using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
    
    public static void AddLGate(Vector2Int position, IoType ioType, bool value)
    {
        Debug.Assert(ioType != IoType.Wire);
        if (IdStructure.IsValid(CheckArea(position).Id))
        {
            return;
        }
        var initIoComponent = new InitIoComponentInfo(
            new Entity { Id = IdStructure.MakeInvalidId() },
            new Entity { Id = IdStructure.MakeInvalidId() }, 0, value);
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

    public static void RemoveEntity(Vector2Int position)
    {
        var lGates = ArchetypeManager.IterLGateComponents();
        var entityToDelete = EntityQuery.AABB_Entity(lGates.Select(x => x.Entity), position);
        if (!IdStructure.IsValid(entityToDelete.Id))
        {
            var wires = ArchetypeManager.IterWireComponents();
            foreach(var wireComponent in EntityQuery.AABB_Entities(wires.Select(x => x.Entity), position))
            {
                // TODO
            }
        }
        
        EntityManager.RemoveEntity(entityToDelete);
    }
    
    public static bool GetEntityWithBiggestOverlap([NotNullWhen(true)]out TransformComponent? transformComponent, Area overlapArea)
    {
        transformComponent = null;
        var overlap = 0;
        foreach (var entity in EntityQuery.AABB_Entities(ArchetypeManager.IterLGateComponents().Select(x => x.Entity), overlapArea))
        {
            var transformComp = EntityManager.GetTransformComponent(entity);
            var newOverlap = CalculateOverlap(overlapArea, transformComp.Position);
            if (newOverlap > overlap)
            {
                overlap = newOverlap;
                transformComponent = transformComp;
            }
        }

        return overlap != 0;
    }

    public static Vector2Int AdjustEntityPosition(Vector2Int adjustedEntityPosition, TransformComponent entityInOverlapArea)
    {
        var xDiff = Math.Abs(entityInOverlapArea.Position.X - adjustedEntityPosition.X);
        var yDiff = Math.Abs(entityInOverlapArea.Position.Y - adjustedEntityPosition.Y);
        var adjustedYDiffToX = yDiff * 2;
        if (xDiff > adjustedYDiffToX)
        {
            (Func<int, int, int> predicate, int yBound) yAxis =
                entityInOverlapArea.Position.Y < adjustedEntityPosition.Y
                    ? (Math.Min, entityInOverlapArea.Position.Y + RectLGateSize.Y)
                    : (Math.Max, entityInOverlapArea.Position.Y - RectLGateSize.Y);
            
            return new Vector2Int 
            { 
                X = AdjustEntityAxis(entityInOverlapArea.Position.X, adjustedEntityPosition.X, RectLGateSize.X), 
                Y = yAxis.predicate(yAxis.yBound, AdjustEntityAxis(entityInOverlapArea.Position.Y, adjustedEntityPosition.Y, yDiff)),
            };
        }
        else //(xDiff < adjustedYDiffToX)
        {
            (Func<int, int, int> predicate, int xBound) xAxis =
                entityInOverlapArea.Position.X < adjustedEntityPosition.X ? 
                    (Math.Min, entityInOverlapArea.Position.X + RectLGateSize.X) :
                    (Math.Max, entityInOverlapArea.Position.X - RectLGateSize.X);

            return new Vector2Int
            {
                X = xAxis.predicate(xAxis.xBound, AdjustEntityAxis(entityInOverlapArea.Position.X, adjustedEntityPosition.X, xDiff)), 
                Y = AdjustEntityAxis(entityInOverlapArea.Position.Y, adjustedEntityPosition.Y, RectLGateSize.Y), 
            };
        }
    }
    
    public static Entity CheckArea(Vector2Int position)
    {
        var entities = ArchetypeManager.IterLGateComponents();

        return EntityQuery.AABB_Entities(entities.Select(x => x.Entity), new Area(position, RectLGateSize))
            .FirstOrDefault(new Entity { Id = IdStructure.MakeInvalidId() });
    }

    public static Area GetLGateOverlapArea(Vector2Int position)
    {
        return new Area
        {
            Position = new Vector2Int(position.X - 20, position.Y - 10),
            Size = new Vector2Int(
                RectLGateSize.X + 40,
                RectLGateSize.Y + 20)
        };
    }
    
    public static Vector2Int CenterRectPositionToCursor(Vector2Int position)
    {
        return new Vector2Int(position.X - RectLGateSize.X / 2, position.Y - RectLGateSize.Y / 2);
    }
    
    private static int AdjustEntityAxis(int targetRectAxis, int observerRectAxis, int length)
    {
        return targetRectAxis > observerRectAxis
            ? targetRectAxis - length
            : targetRectAxis + length;
    }
    
    private static int CalculateOverlap(Area targetArea, Vector2Int observer)
    {
        float overlapX = Math.Max(0, Math.Min(targetArea.Position.X + targetArea.Size.X, observer.X + RectLGateSize.X) - Math.Max(targetArea.Position.X, observer.X));
        float overlapY = Math.Max(0, Math.Min(targetArea.Position.Y + targetArea.Size.Y, observer.Y + RectLGateSize.Y) - Math.Max(targetArea.Position.Y, observer.Y));

        return Math.Abs((int)(overlapX * overlapY));
    }
}