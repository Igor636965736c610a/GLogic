using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using GLogicECS.Components.Common;
using GLogicECS.Components.Init;

namespace GLogicECS.Components.System.DoubleIndexing;

internal static class WireComponentSystem
{
    private static int _freeBackIndexes;

    private static readonly List<int> IdMaps = new();
    private static readonly List<WireComponent> WireComponents = new();

    [Description("internal using: only when creating a new entity in newly added index in the generations list")]
    internal static void IncreaseIdMaps()
    {
        IdMaps.Add(-1);
    }

    internal static WireComponent Add(Entity entity, InitWireComponent init)
    {
        var index = (int)IdStructure.Index(entity.Id);
        Debug.Assert(index < IdMaps.Count);
        if (_freeBackIndexes > 0)
        {
            var mappedIndex = WireComponents.Count - _freeBackIndexes;
            WireComponents[mappedIndex] = new WireComponent
            {
                Entity = entity,
                P1 = init.P1,
                P2 = init.P2
            };
            IdMaps[index] = mappedIndex;

            _freeBackIndexes--;

            return WireComponents[mappedIndex];
        }
        else
        {
            var mappedIndex = WireComponents.Count;
            WireComponents.Add(new WireComponent
            {
                Entity = entity,
                P1 = init.P1,
                P2 = init.P2
            });
            IdMaps[index] = mappedIndex;

            return WireComponents[mappedIndex];
        }
    }

    internal static void Remove(Entity entity)
    {
        var index = (int)IdStructure.Index(entity.Id);
        if (!IsIdMapValid(IdMaps[index]) || !IdStructure.IsValid(entity.Id))
        {
            return;
        }

        Debug.Assert(IdMaps[index] < IdMaps.Count - _freeBackIndexes);
        Debug.Assert(IsIdMapValid(IdMaps[index]));

        var absorbedIndex = WireComponents.Count - _freeBackIndexes - 1;
        var componentToSwap = WireComponents[absorbedIndex];
        var indexMapToSwap = IdMaps[index];
        var deletingComponent = WireComponents[indexMapToSwap];
        Debug.Assert(EntitySystem.IsAlive(componentToSwap.Entity));

        WireComponents[indexMapToSwap] = componentToSwap;
        WireComponents[absorbedIndex] = deletingComponent;
        IdMaps[(int)IdStructure.Index(componentToSwap.Entity.Id)] = indexMapToSwap;
        IdMaps[index] = absorbedIndex;

        _freeBackIndexes++;
    }

    internal static WireComponent Get(Entity entity)
    {
        var mapIndex = IdMaps[(int)IdStructure.Index(entity.Id)];
        Debug.Assert(IsIdMapValid(mapIndex));

        return WireComponents[mapIndex];
    }

    internal static void Update(WireComponent wireComponent)
    {
        var mapIndex = IdMaps[(int)IdStructure.Index(wireComponent.Entity.Id)];
        Debug.Assert(IsIdMapValid(mapIndex));

        WireComponents[mapIndex] = wireComponent;
    }

    internal static IEnumerable<WireComponent> IterWireComponents()
    {
        //return WireComponents.Slice(0, WireComponents.Count - _freeBackIndexes);
        for (var i = 0; i < WireComponents.Count - _freeBackIndexes; i++)
        {
            yield return WireComponents[i];
        }
    }
    
    internal static Span<WireComponent> GetSpan()
    {
        return CollectionsMarshal.AsSpan(WireComponents).Slice(0, WireComponents.Count - _freeBackIndexes);
    }

    private static bool IsIdMapValid(int id)
        => id != -1;
}