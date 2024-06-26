using System.Diagnostics;
using GLogicECS.Components.Common;
using GLogicECS.Components.Init;
using GLogicECS.Components.System.DoubleIndexing;

namespace GLogicECS.Components.System;

internal static class EntitySystem
{
    private const int MinFreeIds = 512;
    private static readonly List<byte> Generations = new();
    private static readonly Queue<uint> FreeIds = new(MinFreeIds * 2);

    internal static readonly List<TransformComponent> TransformComponents = new();
    internal static readonly List<OutputComponent> OutputComponents = new();
    internal static readonly List<InputComponent> InputComponents = new();
    internal static readonly List<EntityTypeComponent> EntityTypeComponents = new();
    internal static readonly List<StateComponent> StateComponents = new();

    internal static Entity CreateEntity(InitEntity info)
    {
        uint id;
        int index;
        Entity entity;

        if (FreeIds.Count > MinFreeIds)
        {
            id = FreeIds.Dequeue();
            id = IdStructure.IdWithNewGeneration(id);
            entity = new Entity(id);
            index = (int)IdStructure.Index(id);
            Generations[index] = IdStructure.Generation(id);
            Debug.Assert(IsAlive(entity));
            TransformComponents[index] = new TransformComponent
            {
                Entity = entity,
                Position = info.TransformComponent.Position,
                Size = info.TransformComponent.Size
            };
            OutputComponents[index] = new OutputComponent
            {
                Entity = entity,
                Outputs = info.InputsOutputsSmallList.Outputs
            };
            InputComponents[index] = new InputComponent
            {
                Entity = entity,
                Inputs = info.InputsOutputsSmallList.Inputs
            };
            EntityTypeComponents[index] = new EntityTypeComponent
            {
                Entity = entity,
                Type = info.IoType
            };
            StateComponents[index] = new StateComponent
            {
                Entity = entity,
                State = info.State
            };
        }
        else
        {
            index = Generations.Count;
            id = IdStructure.MakeEntityId((uint)index);
            entity = new Entity(id);
            Generations.Add(0);
            TransformComponents.Add(new TransformComponent
            {
                Entity = entity,
                Position = info.TransformComponent.Position,
                Size = info.TransformComponent.Size
            });
            OutputComponents.Add(new OutputComponent
            {
                Entity = entity,
                Outputs = info.InputsOutputsSmallList.Outputs
            });
            InputComponents.Add(new InputComponent
            {
                Entity = entity,
                Inputs = info.InputsOutputsSmallList.Inputs
            });
            EntityTypeComponents.Add(new EntityTypeComponent
            {
                Entity = entity,
                Type = info.IoType
            });
            StateComponents.Add(new StateComponent
            {
                Entity = entity,
                State = info.State
            });
            LGateComponentSystem.IncreaseIdMaps();
            WireComponentSystem.IncreaseIdMaps();
        }

        return entity;
    }

    internal static void RemoveEntity(Entity entity)
    {
        var index = (int)IdStructure.Index(entity.Id);
        Debug.Assert(IsAlive(entity));
        if (IsAlive(entity))
        {
            FreeIds.Enqueue(entity.Id);
            var newGeneration = IdStructure.IdWithNewGeneration(entity.Id);
            Generations[index] = IdStructure.Generation(newGeneration);
        }
    }

    internal static bool IsAlive(Entity entity)
    {
        if (!IdStructure.IsValid(entity.Id))
        {
            return false;
        }

        var index = (int)IdStructure.Index(entity.Id);
        Debug.Assert(index < Generations.Count);
        return Generations[index] == IdStructure.Generation(entity.Id);
    }

    internal static int EntitiesCount()
        => Generations.Count;
}