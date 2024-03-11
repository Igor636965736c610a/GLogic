using System.Diagnostics;
using GLogic.Components.Common;
using GLogic.Components.Init;

namespace GLogic.Components.System;

public static class EntityManager
{
    private static readonly List<byte> Generations = new();
    private static readonly Queue<uint> FreeIds = new(MinFreeIds * 2);
    private const int MinFreeIds = 512;

    private static readonly List<TransformComponent> TransformComponents = new();
    private static readonly List<IoComponent> IoComponents = new();
    private static readonly List<EntityTypeComponent> EntityTypeComponents = new();

    public static Entity CreateEntity(InitEntityInfo info)
    {
        uint id;
        int index;
        Entity entity;
        if (FreeIds.Count > MinFreeIds)
        {
            id = FreeIds.Dequeue();
            entity = new Entity { Id = id };
            Debug.Assert(!IsAlive(entity));
            id = IdStructure.IdWithNewGeneration(id);
            index = (int)IdStructure.Index(id);
            Generations[index] = IdStructure.Generation(id);
            TransformComponents[index] = new TransformComponent
            {
                Entity = entity,
                Position = info.TransformComponentInfo.Position,
                Size = info.TransformComponentInfo.Size,
            };
            IoComponents[index] = new IoComponent
            {
                Entity = entity,
                ConnectionOne = info.IoComponentInfo.ConnectionOne,
                ConnectionTwo = info.IoComponentInfo.ConnectionTwo,
                LastFrame = info.IoComponentInfo.Frame,
                Value = info.IoComponentInfo.Value,
            };
            EntityTypeComponents[index] = new EntityTypeComponent
            {
                Entity = entity,
                Type = info.IoType,
            };
        }
        else
        {
            index = Generations.Count;
            id = IdStructure.MakeEntityId((uint)index);
            entity = new Entity { Id = id };
            Generations.Add(0);
            TransformComponents.Add(new TransformComponent
            {
                Entity = entity,
                Position = info.TransformComponentInfo.Position,
                Size = info.TransformComponentInfo.Size,
            });
            IoComponents.Add(new IoComponent
            {
                Entity = entity,
                ConnectionOne = info.IoComponentInfo.ConnectionOne,
                ConnectionTwo = info.IoComponentInfo.ConnectionTwo,
                LastFrame = info.IoComponentInfo.Frame,
                Value = info.IoComponentInfo.Value,
            });
            EntityTypeComponents.Add(new EntityTypeComponent
            {
                Entity = entity,
                Type = info.IoType,
            });
        }

        return entity;
    }

    public static Archetype CreateArchetypeForEntity(InitArchetype initArchetype)
    {
        Debug.Assert(IsAlive(initArchetype.Entity));
        var index = IdStructure.Index(initArchetype.Entity.Id);

        if (ArchetypeManager.Archetypes.Count <= index)
        {
            ArchetypeManager.Archetypes.Add(new Archetype
            {
                Entity = initArchetype.Entity,
                ArchetypeOption = initArchetype.ArchetypeOption,
                IndexOfArchetypeOption = initArchetype.ArchetypeIndex,
            });
            
            Debug.Assert(ArchetypeManager.Archetypes.Count - 1 == index);
        }
        else
        {
            ArchetypeManager.Archetypes[(int)index] = new Archetype
            {
                Entity = initArchetype.Entity,
                ArchetypeOption = initArchetype.ArchetypeOption,
                IndexOfArchetypeOption = initArchetype.ArchetypeIndex,
            };
        }

        return ArchetypeManager.Archetypes[(int)index];
    }

    public static void RemoveEntity(Entity entity)
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

    public static bool IsAlive(Entity entity)
    {
        if (!IdStructure.IsValid(entity.Id))
        {
            return false;
        }

        var index = (int)IdStructure.Index(entity.Id);
        Debug.Assert(index < Generations.Count);
        return Generations[index] == IdStructure.Generation(entity.Id);
    }

    public static TransformComponent GetTransformComponent(Entity entity)
        => TransformComponents[(int)IdStructure.Index(entity.Id)];

    public static IoComponent GetIoComponent(Entity entity)
        => IoComponents[(int)IdStructure.Index(entity.Id)];

    public static EntityTypeComponent GetEntityTypeComponent(Entity entity)
        => EntityTypeComponents[(int)IdStructure.Index(entity.Id)];

    public static IEnumerable<TransformComponent> IterTransformComponents()
        => TransformComponents;

    public static IEnumerable<IoComponent> IterIoComponents()
        => IoComponents;

    public static IEnumerable<EntityTypeComponent> IterEntityTypeComponents()
        => EntityTypeComponents;

    public static void UpdateIoComponent(IoComponent ioComponent)
    {
        var index = (int)IdStructure.Index(ioComponent.Entity.Id);
        IoComponents[index] = ioComponent;
    }
}