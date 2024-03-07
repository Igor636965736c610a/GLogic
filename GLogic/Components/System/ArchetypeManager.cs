using System.Diagnostics;
using GLogic.Components.Common;
using GLogic.Components.Init;

namespace GLogic.Components.System;

public static class ArchetypeManager
{
    private static readonly List<byte> WireGenerations = new();
    private static readonly Queue<uint> WireFreeIds = new(1024);
    private static readonly List<byte> LGateGenerations = new();
    private static readonly Queue<uint> LGateFreeIds = new(1024);
    
    public static readonly List<Archetype> Archetypes = new(); // Entity indexes == Archetype indexes
    
    private static readonly List<WireComponent> WireComponents = new();
    private static readonly List<LGateComponent> LGateComponents = new();
    
    private const int MinFreeIds = 512;
    
    public static WireComponent CreateWireArchetype(InitWireComponent initWireComponent)
    {
        uint id;
        int index;
        WireComponent wireComponent;
        Debug.Assert(EntityManager.IsAlive(initWireComponent.Entity));
        if (WireFreeIds.Count > MinFreeIds)
        {
            id = WireFreeIds.Dequeue();
            id = IdStructure.IdWithNewGeneration(id);
            index = (int)IdStructure.Index(id);
            WireGenerations[index] = IdStructure.Generation(id);
            wireComponent = new WireComponent
            {
                Id = id,
                Entity = initWireComponent.Entity,
            };

            WireComponents[index] = wireComponent;
        }
        else
        {
            index = WireGenerations.Count;
            id = IdStructure.MakeEntityId((uint)index);
            WireGenerations.Add(0);
            wireComponent = new WireComponent
            {
                Id = id,
                Entity = initWireComponent.Entity,
            };
            
            WireComponents.Add(wireComponent);
        }

        return wireComponent;
    }
    
    public static LGateComponent CreateLGateArchetype(InitLGateComponent initLGateComponent)
    {
        Debug.Assert(EntityManager.IsAlive(initLGateComponent.Entity));
        uint id;
        int index;
        LGateComponent lGateComponent;
        if (LGateFreeIds.Count > MinFreeIds)
        {
            id = WireFreeIds.Dequeue();
            id = IdStructure.IdWithNewGeneration(id);
            index = (int)IdStructure.Index(id);
            LGateGenerations[index] = IdStructure.Generation(id);
            lGateComponent = new LGateComponent
            {
                Id = id,
                Entity = initLGateComponent.Entity,
            };

            LGateComponents[index] = lGateComponent;
        }
        else
        {
            index = LGateGenerations.Count;
            id = IdStructure.MakeEntityId((uint)index);
            LGateGenerations.Add(0);
            lGateComponent = new LGateComponent
            {
                Id = id,
                Entity = initLGateComponent.Entity,
            };
            
            LGateComponents.Add(lGateComponent);
        }

        return lGateComponent;
    }

    // Only while removing an entity
    public static void RemoveWireArchetype(WireComponent wireComponent)
    {
        var index = (int)IdStructure.Index(wireComponent.Id);
        WireFreeIds.Enqueue(wireComponent.Id);
        var newGeneration = IdStructure.IdWithNewGeneration(wireComponent.Id);
        WireGenerations[index] = IdStructure.Generation(newGeneration);
    }
    
    // Only while removing an entity
    public static void RemoveLGateArchetype(LGateComponent lGateComponent)
    {
        var index = (int)IdStructure.Index(lGateComponent.Id);
        LGateFreeIds.Enqueue(lGateComponent.Id);
        var newGeneration = IdStructure.IdWithNewGeneration(lGateComponent.Id);
        LGateGenerations[index] = IdStructure.Generation(newGeneration);
    }

    public static IEnumerable<WireComponent> IterWireComponents()
        => WireComponents;
    
    public static IEnumerable<LGateComponent> IterLGateComponents()
        => LGateComponents;
}