using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using GLogic.Jobs.Renderer;
using GLogicECS.Api;
using GLogicECS.Components;
using GLogicECS.Components.Common;
using GLogicECS.Components.Init;
using GLogicECS.Components.Interfaces;
using GLogicGlobal.Common;

namespace GLogic.Jobs.Internal.EcsStateModifiers;

internal static class EntityService
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

        // Debug.Assert(!IdStructure.IsValid(
        //         AnyEntityInArea(position, ComponentManager
        //             .IterLGateComponents()).Entity.Id
        //     )
        // );

        var initTransformComponent = new InitTransformComponent(position, RectLGateSize);
        var initLGate = new InitLGate(initTransformComponent, ioType, value);
        
        return EntityManager.CreateEntity(initLGate);
    }

    public static Entity? AddWire(Vector2Int point)
    {
        var lGates = ComponentManager.IterLGateComponents();
        var lGateComp = EntityQuery
            .AABB_Entities(lGates, point)
            .FirstOrDefault(new LGateComponent(new Entity(IdStructure.MakeInvalidId())));

        var entityToConnect = lGateComp.Entity;

        if (!EntityManager.IsAlive(entityToConnect))
        {
            WireService.Reset();
            return null;
        }

        var ioType = ComponentManager.GetEntityTypeComponent(entityToConnect).Type;
        var entityPosition = ComponentManager.GetTransformComponent(entityToConnect).Position;

        var hookInfo = WireService.GetHookInfo(ioType, entityPosition, point);
        if (hookInfo is null)
        {
            WireService.Reset();
            return null;
        }

        var wire = WireService.Create(new Connection
        {
            ConnectionType = hookInfo.Value.ConnectionType,
            Entity = entityToConnect,
            HookNumber = hookInfo.Value.HookNumber
        });

        return wire;
    }

    public static void RemoveEntity(Entity entity)
    {
        var type = ComponentManager.GetEntityTypeComponent(entity).Type;
        if (type != IoType.Wire)
        {
            var inputComp = ComponentManager.GetInputComponent(entity);
            var outputComp = ComponentManager.GetOutputComponent(entity);

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

        //TODO
        EntityManager.RemoveEntity(entity);
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

    public static (Vector2Int position, Placement placement) GetDynamicLGateParamsToRender<T>(Vector2Int position,
        IEnumerable<T> otherEntities) where T : struct, IAABBCompare
    {
        var chosenLGatePosition = CenterRectPositionToCursor(position);

        if (UserActionsHandler.ShiftKeyState)
        {
            return HandleShiftKeyPressed(chosenLGatePosition, otherEntities);
        }

        var placement = GetPlacement(chosenLGatePosition, otherEntities);
        return (chosenLGatePosition, placement);
    }

    public static Entity GetEntityToDelete(Vector2Int position)
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

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    private static (Vector2Int position, Placement placement) HandleShiftKeyPressed<T>(Vector2Int chosenLGatePosition,
        IEnumerable<T> otherEntities) where T : struct, IAABBCompare
    {
        var overlapArea = GetLGateOverlapArea(chosenLGatePosition);
        var overlap = GetEntityWithBiggestOverlap(out var entityInOverlapArea, overlapArea, otherEntities);
        
        if (!overlap)
        {
            return HandleNoOverlap(chosenLGatePosition, otherEntities);
        }

        Debug.Assert(entityInOverlapArea.HasValue);
        chosenLGatePosition = AdjustEntityPosition(chosenLGatePosition, entityInOverlapArea.Value);
        var placement = GetPlacement(chosenLGatePosition, otherEntities);
        return (chosenLGatePosition, placement);
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    private static (Vector2Int position, Placement placement) HandleNoOverlap<T>(Vector2Int chosenLGatePosition,
        IEnumerable<T> otherEntities) where T : struct, IAABBCompare
    {
        chosenLGatePosition = new Vector2Int(
            AdjustToGrid(chosenLGatePosition.X),
            AdjustToGrid(chosenLGatePosition.Y)
        );
        
        var overlapArea = GetLGateOverlapArea(chosenLGatePosition);
        var overlap = GetEntityWithBiggestOverlap(out var entityInOverlapArea, overlapArea, otherEntities);

        if (!overlap)
        {
            return (chosenLGatePosition, Placement.Valid);
        }

        Debug.Assert(entityInOverlapArea.HasValue);
        chosenLGatePosition = AdjustEntityPosition(chosenLGatePosition, entityInOverlapArea.Value);
        var placement = GetPlacement(chosenLGatePosition, otherEntities);
        return (chosenLGatePosition, placement);
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
        var distanceToEntityX = Math.Abs(entityInOverlapArea.Position.X - adjustedEntityPosition.X);
        var distanceToEntityY = Math.Abs(entityInOverlapArea.Position.Y - adjustedEntityPosition.Y);
        var adjustedYDistanceToEntityX = distanceToEntityY * 2;

        if (distanceToEntityX > adjustedYDistanceToEntityX)
        {
            if (distanceToEntityY <= 10)
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

            var newY = AdjustEntityAxis(entityInOverlapArea.Position.Y, adjustedEntityPosition.Y, distanceToEntityY);
            newY = AdjustToGrid(newY);
            newY = entityInOverlapArea.Position.Y < adjustedEntityPosition.Y
                ? Math.Min(yBound, newY)
                : Math.Max(yBound, newY);

            return new Vector2Int
            {
                X = AdjustEntityAxis(entityInOverlapArea.Position.X, adjustedEntityPosition.X, RectLGateSize.X),
                Y = newY
            };
        }

        if (distanceToEntityX <= 10)
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

        var newX = AdjustEntityAxis(entityInOverlapArea.Position.X, adjustedEntityPosition.X, distanceToEntityX);
        newX = AdjustToGrid(newX);
        newX = entityInOverlapArea.Position.X < adjustedEntityPosition.X
            ? Math.Min(xBound, newX)
            : Math.Max(xBound, newX);

        return new Vector2Int
        {
            X = newX,
            Y = AdjustEntityAxis(entityInOverlapArea.Position.Y, adjustedEntityPosition.Y, RectLGateSize.Y)
        };
    }

    private static int AdjustToGrid(int value)
        => (value + 5) / 10 * 10;

    private static T AnyEntityInArea<T>(Vector2Int position, IEnumerable<T> entities) where T : struct, IAABBCompare
        => EntityQuery
            .AABB_Entities(entities, new Area(position, RectLGateSize))
            .FirstOrDefault(new T());

    private static Area GetLGateOverlapArea(Vector2Int position)
    {
        const int magnificationCorrectX = 20;
        const int magnificationCorrectY = 20;
        return new Area
        {
            Position = new Vector2Int(position.X - magnificationCorrectX, position.Y - magnificationCorrectY),
            Size = new Vector2Int(
                RectLGateSize.X + magnificationCorrectX * 2,
                RectLGateSize.Y + magnificationCorrectY * 2
            )
        };
    }

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