using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using GLogic.Components.Common;
using GLogic.Jobs.Renderer;
using GLogicECS.Api;
using GLogicECS.Components;
using GLogicECS.Components.Common;
using GLogicECS.Components.Init;
using GLogicECS.Components.Interfaces;

namespace GLogic.Jobs;

public static class EntityService
{
    static EntityService()
    {
        RectLGateSize = new Vector2Int(100, 50);
    }

    public static Vector2Int RectLGateSize { get; }

    public static Entity AddLGate(Vector2Int position, IoType ioType, bool value)
    {
        if (ioType == IoType.Wire)
        {
            throw new InvalidProgramException("Invalid IoType - Wire");
        }

        Debug.Assert(!IdStructure.IsValid(
                AnyEntityInArea(position, ComponentManager
                    .IterLGateComponents()).Entity.Id
            )
        );

        var initTransformComponent = new InitTransformComponent(position, RectLGateSize);
        var initLGate = new InitLGate(initTransformComponent, ioType, value);
        return EntityManager.CreateEntity(initLGate);

        //Console.WriteLine(entity.Id);
    }

    public static void AddWire(Vector2Int point)
    {
        var lGates = ComponentManager.IterLGateComponents();
        var lGateComp = EntityQuery
            .AABB_Entities(lGates, point)
            .FirstOrDefault(new LGateComponent(new Entity(IdStructure.MakeInvalidId())));

        var entityToConnect = lGateComp.Entity;

        if (!EntityManager.IsAlive(entityToConnect))
        {
            WireService.Reset();
            return;
        }

        var ioType = ComponentManager.GetEntityTypeComponent(entityToConnect).Type;
        var entityPosition = ComponentManager.GetTransformComponent(entityToConnect).Position;

        var hookInfo = WireService.GetHookInfo(ioType, entityPosition, point);
        if (hookInfo is null)
        {
            WireService.Reset();
            return;
        }

        WireService.Create(new Connection
        {
            ConnectionType = hookInfo.Value.ConnectionType,
            Entity = entityToConnect,
            HookNumber = hookInfo.Value.HookNumber
        });
    }

    public static void RemoveEntity(Vector2Int position)
    {
        var entityToDelete = GetEntityToDelete(position);

        if (!IdStructure.IsValid(entityToDelete.Id))
        {
            return;
        }

        var type = ComponentManager.GetEntityTypeComponent(entityToDelete).Type;
        if (type != IoType.Wire)
        {
            var inputComp = ComponentManager.GetInputComponent(entityToDelete);
            var outputComp = ComponentManager.GetOutputComponent(entityToDelete);

            for (var i = 0; i < inputComp.Inputs.Count; i++)
            {
                Debug.Assert(EntityManager.IsAlive(inputComp.Inputs[i].Entity));
                Debug.Assert(
                    ComponentManager.GetEntityTypeComponent(inputComp.Inputs[i].Entity).Type == IoType.Wire);

                WireService.RemoveFromOtherOutputs(inputComp.Inputs[i].Entity);
                EntityManager.RemoveEntity(inputComp.Inputs[i].Entity);
            }

            for (var i = 0; i < outputComp.Outputs.Count; i++)
            {
                Debug.Assert(EntityManager.IsAlive(outputComp.Outputs[i].Entity));
                Debug.Assert(ComponentManager.GetEntityTypeComponent(outputComp.Outputs[i].Entity).Type ==
                             IoType.Wire);

                WireService.RemoveFromOtherInputs(outputComp.Outputs[i].Entity);
                EntityManager.RemoveEntity(outputComp.Outputs[i].Entity);
            }
        }

        EntityManager.RemoveEntity(entityToDelete);
    }

    public static void UpdateEntityPosition(Entity entity, Vector2Int newPosition)
    {
        if (!IdStructure.IsValid(entity.Id))
        {
            throw new InvalidProgramException("Shifting removed entity");
        }

        var transformComponent = ComponentManager.GetTransformComponent(entity);
        Debug.Assert(IdStructure.IsValid(transformComponent.Entity.Id));
        Debug.Assert(ComponentManager.GetEntityTypeComponent(entity).Type != IoType.Wire);

        transformComponent.Position = newPosition;

        Debug.Assert(!IdStructure.IsValid(
                EntityQuery.AABB_Entities(
                        ComponentManager.IterLGateComponents(),
                        new Area(newPosition, RectLGateSize)
                    )
                    .FirstOrDefault(
                        x => x.Entity.Id != entity.Id,
                        new LGateComponent(new Entity(IdStructure.MakeInvalidId()))
                    ).Entity.Id
            )
        );

        ComponentManager.UpdateTransformComponent(transformComponent);
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public static (Vector2Int position, Placement placement) GetDynamicLGateParamsToRender<T>(Vector2Int position,
        IEnumerable<T> otherEntities) where T : struct, IAABBCompare
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

    private static Entity GetEntityToDelete(Vector2Int position)
    {
        var lGates = ComponentManager.IterLGateComponents();
        var lGateComp = EntityQuery
            .AABB_Entities(lGates, position)
            .FirstOrDefault(new LGateComponent(new Entity(IdStructure.MakeInvalidId())));

        var entityToDelete = lGateComp.Entity;

        if (!IdStructure.IsValid(entityToDelete.Id))
        {
            var wires = ComponentManager.IterWireComponents();
            // foreach (var wireComponent in EntityQuery.AABB_Entities(wires.Select(x => x.Entity), position))
            // {
            //     // TODO
            // }
        }

        return entityToDelete;
    }

    private static Placement GetPlacement<T>(Vector2Int position, IEnumerable<T> otherEntities)
        where T : struct, IAABBCompare
    {
        var placement = Placement.Valid;
        if (IdStructure.IsValid(AnyEntityInArea(
                position, otherEntities).GetEntity().Id)
           )
        {
            placement = Placement.Invalid;
        }

        return placement;
    }

    private static bool GetEntityWithBiggestOverlap<T>([NotNullWhen(true)] out TransformComponent? transformComponent,
        Area overlapArea, IEnumerable<T> entities) where T : struct, IAABBCompare
    {
        transformComponent = null;
        var overlap = 0;
        foreach (var entity in EntityQuery.AABB_Entities(entities, overlapArea))
        {
            var transformComp = ComponentManager.GetTransformComponent(entity.GetEntity());
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

        // (xDiff < adjustedYDiffToX)
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

    private static T AnyEntityInArea<T>(Vector2Int position, IEnumerable<T> entities) where T : struct, IAABBCompare
        => EntityQuery
            .AABB_Entities(entities, new Area(position, RectLGateSize))
            .FirstOrDefault(new T());

    private static Area GetLGateOverlapArea(Vector2Int position)
        => new()
        {
            Position = new Vector2Int(position.X - 20, position.Y - 10),
            Size = new Vector2Int(
                RectLGateSize.X + 40,
                RectLGateSize.Y + 20
            )
        };

    private static Vector2Int CenterRectPositionToCursor(Vector2Int position)
        => new(position.X - RectLGateSize.X / 2, position.Y - RectLGateSize.Y / 2);

    private static int AdjustEntityAxis(int targetRectAxis, int observerRectAxis, int length)
        => targetRectAxis > observerRectAxis
            ? targetRectAxis - length
            : targetRectAxis + length;

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