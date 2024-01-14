using System.Diagnostics;
using GLogic.Components.Common;

namespace GLogic.Components;

public static class EntityManager
{ 
    private static readonly List<byte> Generations = new();
    private static readonly Queue<uint> FreeIds = new();
    private const int MinFreeIds = 512;

    private static readonly List<TransformComponent> _transformComponents = new();
    private static readonly List<IoComponent> _ioComponents = new();

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
            _transformComponents[index] = new TransformComponent
            {
                Entity = entity
            };
            _ioComponents[index] = new IoComponent
            {
                Entity = entity,
                Input1 = info.Input1,
                Input2 = info.Input2,
                IoType = info.IoType,
                LastFrame = info.Frame,
                Value = false
            };
        }
        else
        {
            index = Generations.Count;
            id = (uint)index;
            entity = new Entity { Id = id };
            Generations.Add(0);
            _transformComponents.Add(new TransformComponent
            {
                Entity = entity
            });
            _ioComponents.Add(new IoComponent
            {
                Entity = entity,
                Input1 = info.Input1,
                Input2 = info.Input2,
                IoType = info.IoType,
                LastFrame = info.Frame,
                Value = false
            });
        }
        
        return entity;
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
        var index = (int)IdStructure.Index(entity.Id);
        Debug.Assert(index < Generations.Count);
        Debug.Assert(Generations[index] == IdStructure.Generation(entity.Id));
        return Generations[index] == IdStructure.Generation(entity.Id);
    }

    public static TransformComponent GetTransformComponent(Entity entity)
        => _transformComponents[(int)IdStructure.Index(entity.Id)];

    public static IoComponent GetIoComponent(Entity entity)
        => _ioComponents[(int)IdStructure.Index(entity.Id)];

    public static IEnumerable<TransformComponent> IterTransformComponents()
        => _transformComponents;

    public static IEnumerable<IoComponent> IterIoComponents()
        => _ioComponents;

    public static void UpdateIoComponent(IoComponent ioComponent)
    {
        var index = (int)IdStructure.Index(ioComponent.Entity.Id);
        _ioComponents[index] = ioComponent;
    }
}