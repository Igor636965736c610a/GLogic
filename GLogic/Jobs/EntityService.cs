using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using GLogic.Components.Common;
using GLogic.Jobs.Renderer;
using GLogicECS.Api;
using GLogicECS.Components;
using GLogicECS.Components.Common;
using GLogicECS.Components.Init;

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
        if (ioType == IoType.Wire)
        {
            throw new InvalidProgramException("Invalid IoType - Wire");
        }
        if (IdStructure.IsValid(
                CheckArea(position, ComponentManager
                    .IterLGateComponents()
                    .Select(x => x.Entity)).Id
                )
            )
        {
            return;
        }

        var initTransformComponent = new InitTransformComponent(position, RectLGateSize);
        var initLGate = new InitLGate(initTransformComponent, ioType, value);
        var entity = EntityManager.CreateEntity(initLGate);

        Console.WriteLine(entity.Id);
    }

    public static void RemoveEntity(Vector2Int position)
    {
        var lGates = ComponentManager.IterLGateComponents();
        var entityToDelete = EntityQuery.AABB_Entities(lGates.Select(x => x.Entity), position)
            .FirstOrDefault(new Entity(IdStructure.MakeInvalidId()));

        if (!IdStructure.IsValid(entityToDelete.Id))
        {
            var wires = ComponentManager.IterWireComponents();
            foreach (var wireComponent in EntityQuery.AABB_Entities(wires.Select(x => x.Entity), position))
            {
                // TODO
            }
        }

        if (IdStructure.IsValid(entityToDelete.Id))
        {
            EntityManager.RemoveEntity(entityToDelete);
        }
    }

    public static bool GetEntityWithBiggestOverlap([NotNullWhen(true)] out TransformComponent? transformComponent,
        Area overlapArea, IEnumerable<Entity> entities)
    {
        transformComponent = null;
        var overlap = 0;
        foreach (var entity in EntityQuery.AABB_Entities(entities, overlapArea))
        {
            var transformComp = ComponentManager.GetTransformComponent(entity);
            var newOverlap = CalculateOverlap(overlapArea, transformComp.Position);
            if (newOverlap > overlap)
            {
                overlap = newOverlap;
                transformComponent = transformComp;
            }
        }

        return overlap != 0;
    }

    public static Vector2Int AdjustEntityPosition(Vector2Int adjustedEntityPosition,
        TransformComponent entityInOverlapArea)
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
                Y = yAxis.predicate(yAxis.yBound,
                    AdjustEntityAxis(entityInOverlapArea.Position.Y, adjustedEntityPosition.Y, yDiff))
            };
        }
        else //(xDiff < adjustedYDiffToX)
        {
            (Func<int, int, int> predicate, int xBound) xAxis =
                entityInOverlapArea.Position.X < adjustedEntityPosition.X
                    ? (Math.Min, entityInOverlapArea.Position.X + RectLGateSize.X)
                    : (Math.Max, entityInOverlapArea.Position.X - RectLGateSize.X);

            return new Vector2Int
            {
                X = xAxis.predicate(xAxis.xBound,
                    AdjustEntityAxis(entityInOverlapArea.Position.X, adjustedEntityPosition.X, xDiff)),
                Y = AdjustEntityAxis(entityInOverlapArea.Position.Y, adjustedEntityPosition.Y, RectLGateSize.Y)
            };
        }
    }

    public static Entity CheckArea(Vector2Int position, IEnumerable<Entity> entities)
    {
        return EntityQuery
            .AABB_Entities(entities, new Area(position, RectLGateSize))
            .FirstOrDefault(new Entity { Id = IdStructure.MakeInvalidId() });
    }

    public static Area GetLGateOverlapArea(Vector2Int position)
    {
        return new Area
        {
            Position = new Vector2Int(position.X - 20, position.Y - 10),
            Size = new Vector2Int(
                RectLGateSize.X + 40,
                RectLGateSize.Y + 20
            )
        };
    }

    public static Vector2Int CenterRectPositionToCursor(Vector2Int position)
    {
        return new Vector2Int(position.X - RectLGateSize.X / 2, position.Y - RectLGateSize.Y / 2);
    }

    public static void UpdateEntityPosition(Entity entity, Vector2Int newPosition)
    {
        if (!IdStructure.IsValid(entity.Id))
        {
            throw new InvalidProgramException("Shifting removed entity");
        }

        var transformComponent = ComponentManager.GetTransformComponent(entity);
        Debug.Assert(IdStructure.IsValid(transformComponent.Entity.Id));

        transformComponent.Position = newPosition;

        Debug.Assert(!IdStructure.IsValid(CheckArea(
            transformComponent.Position,
            ComponentManager.IterLGateComponents().Select(x => x.Entity).Where(x => x.Id != entity.Id)).Id)
        );

        ComponentManager.UpdateTransformComponent(transformComponent);
    }

    private static int AdjustEntityAxis(int targetRectAxis, int observerRectAxis, int length)
    {
        return targetRectAxis > observerRectAxis
            ? targetRectAxis - length
            : targetRectAxis + length;
    }

    private static int CalculateOverlap(Area targetArea, Vector2Int observer)
    {
        float overlapX = Math.Max(0,
            Math.Min(targetArea.Position.X + targetArea.Size.X, observer.X + RectLGateSize.X) -
            Math.Max(targetArea.Position.X, observer.X));
        float overlapY = Math.Max(0,
            Math.Min(targetArea.Position.Y + targetArea.Size.Y, observer.Y + RectLGateSize.Y) -
            Math.Max(targetArea.Position.Y, observer.Y));

        return Math.Abs((int)(overlapX * overlapY));
    }
}