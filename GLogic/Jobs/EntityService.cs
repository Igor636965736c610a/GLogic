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
                AnyEntityInArea(position, ComponentManager
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

    public static void UpdateEntityPosition(Entity entity, Vector2Int newPosition)
    {
        if (!IdStructure.IsValid(entity.Id))
        {
            throw new InvalidProgramException("Shifting removed entity");
        }

        var transformComponent = ComponentManager.GetTransformComponent(entity);
        Debug.Assert(IdStructure.IsValid(transformComponent.Entity.Id));

        transformComponent.Position = newPosition;

        Debug.Assert(!IdStructure.IsValid(AnyEntityInArea(
            transformComponent.Position,
            ComponentManager.IterLGateComponents().Select(x => x.Entity).Where(x => x.Id != entity.Id)).Id)
        );

        ComponentManager.UpdateTransformComponent(transformComponent);
    }
    
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public static (Vector2Int position, Placement placement) GetDynamicLGateParamsToRender(Vector2Int position, IEnumerable<Entity> otherEntities)
    {
        var chosenLGatePosition = CenterRectPositionToCursor(position);

        var placement = Placement.Valid;
        if (UserActionsHandler.ShiftKeyState)
        {
            var overlapArea = GetLGateOverlapArea(chosenLGatePosition);
            var overlap = GetEntityWithBiggestOverlap(
                out var entityInOverlapArea,
                overlapArea,
                otherEntities
            );
            
            if (!overlap)
            {
                return (chosenLGatePosition, placement);
            }

            Debug.Assert(entityInOverlapArea.HasValue);
            chosenLGatePosition = AdjustEntityPosition(chosenLGatePosition, entityInOverlapArea.Value);
            placement = GetPlacement(chosenLGatePosition, otherEntities);
        }
        else
        {
            placement = GetPlacement(chosenLGatePosition, otherEntities);
        }
        
        return (chosenLGatePosition, placement);
    }

    private static Placement GetPlacement(Vector2Int position, IEnumerable<Entity> otherEntities)
    {
        var placement = Placement.Valid;
        if (IdStructure.IsValid(AnyEntityInArea(
                position, otherEntities).Id)
           )
        {
            placement = Placement.Invalid;
        }

        return placement;
    }

    private static bool GetEntityWithBiggestOverlap([NotNullWhen(true)] out TransformComponent? transformComponent,
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
    
    private static Vector2Int AdjustEntityPosition(Vector2Int adjustedEntityPosition,
        TransformComponent entityInOverlapArea)
    {
        var xDiff = Math.Abs(entityInOverlapArea.Position.X - adjustedEntityPosition.X);
        var yDiff = Math.Abs(entityInOverlapArea.Position.Y - adjustedEntityPosition.Y);
        var adjustedYDiffToX = yDiff * 2;

        if (xDiff > adjustedYDiffToX)
        {
            if (yDiff <= 10)
            {
                return entityInOverlapArea.Position with
                {
                    X = AdjustEntityAxis(
                        entityInOverlapArea.Position.X, 
                        adjustedEntityPosition.X, RectLGateSize.X
                        )
                };
            }
            var yBound = entityInOverlapArea.Position.Y < adjustedEntityPosition.Y
                ? entityInOverlapArea.Position.Y + RectLGateSize.Y
                : entityInOverlapArea.Position.Y - RectLGateSize.Y;

            var newY = AdjustEntityAxis(entityInOverlapArea.Position.Y, adjustedEntityPosition.Y, yDiff);
            newY = entityInOverlapArea.Position.Y < adjustedEntityPosition.Y 
                ? Math.Min(yBound, newY) 
                : Math.Max(yBound, newY);

            return new Vector2Int
            {
                X = AdjustEntityAxis(entityInOverlapArea.Position.X, adjustedEntityPosition.X, RectLGateSize.X),
                Y = newY
            };
        }
        else // (xDiff < adjustedYDiffToX)
        {
            if (xDiff <= 10)
            {
                return entityInOverlapArea.Position with
                {
                    Y = AdjustEntityAxis(
                        entityInOverlapArea.Position.Y, 
                        adjustedEntityPosition.Y, RectLGateSize.Y
                    )
                };
            }
            
            var xBound = entityInOverlapArea.Position.X < adjustedEntityPosition.X
                ? entityInOverlapArea.Position.X + RectLGateSize.X
                : entityInOverlapArea.Position.X - RectLGateSize.X;

            var newX = AdjustEntityAxis(entityInOverlapArea.Position.X, adjustedEntityPosition.X, xDiff);
            newX = entityInOverlapArea.Position.X < adjustedEntityPosition.X 
                ? Math.Min(xBound, newX) 
                : Math.Max(xBound, newX);

            return new Vector2Int
            {
                X = newX,
                Y = AdjustEntityAxis(entityInOverlapArea.Position.Y, adjustedEntityPosition.Y, RectLGateSize.Y)
            };
        }
    }

    private static Entity AnyEntityInArea(Vector2Int position, IEnumerable<Entity> entities)
    {
        return EntityQuery
            .AABB_Entities(entities, new Area(position, RectLGateSize))
            .FirstOrDefault(new Entity { Id = IdStructure.MakeInvalidId() });
    }

    private static Area GetLGateOverlapArea(Vector2Int position)
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

    private static Vector2Int CenterRectPositionToCursor(Vector2Int position)
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
        float overlapX = Math.Max(0,
            Math.Min(targetArea.Position.X + targetArea.Size.X, observer.X + RectLGateSize.X) -
            Math.Max(targetArea.Position.X, observer.X));
        float overlapY = Math.Max(0,
            Math.Min(targetArea.Position.Y + targetArea.Size.Y, observer.Y + RectLGateSize.Y) -
            Math.Max(targetArea.Position.Y, observer.Y));

        return Math.Abs((int)(overlapX * overlapY));
    }
}