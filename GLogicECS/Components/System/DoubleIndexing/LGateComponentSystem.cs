using System.ComponentModel;
using System.Diagnostics;
using GLogicECS.Components.Common;

namespace GLogicECS.Components.System.DoubleIndexing;

internal static class LGateComponentSystem
{
    private static int _freeBackIndexes;

    private static readonly List<int> IdMaps = new();
    private static readonly List<LGateComponent> LGateComponents = new();

    [Description("internal using: only when creating a new entity in newly added index in the generations list")]
    internal static void IncreaseIdMaps()
    {
        IdMaps.Add(-1);
    }

    internal static LGateComponent Add(Entity entity)
    {
        //Console.WriteLine(EntitySystem.IsAlive(entity));
        var index = (int)IdStructure.Index(entity.Id);
        Debug.Assert(index < IdMaps.Count);
        if (_freeBackIndexes > 0)
        {
            var mappedIndex = LGateComponents.Count - _freeBackIndexes;
            Debug.Assert(mappedIndex >= 0);
            LGateComponents[mappedIndex] = new LGateComponent
            {
                Entity = entity
            };
            IdMaps[index] = mappedIndex;

            _freeBackIndexes--;

            return LGateComponents[mappedIndex];
        }
        else
        {
            var mappedIndex = LGateComponents.Count;
            LGateComponents.Add(new LGateComponent
            {
                Entity = entity
            });
            IdMaps[index] = mappedIndex;

            return LGateComponents[mappedIndex];
        }
    }

    internal static void Remove(Entity entity)
    {
        var index = (int)IdStructure.Index(entity.Id);
        if (!IsIdMapValid(IdMaps[index]) || !IdStructure.IsValid(entity.Id))
        {
            return;
        }

        Debug.Assert(IdMaps[index] < LGateComponents.Count - _freeBackIndexes);
        Debug.Assert(IsIdMapValid(IdMaps[index]));

        var absorbedIndex = LGateComponents.Count - _freeBackIndexes - 1;
        var componentToSwap = LGateComponents[absorbedIndex];
        var indexMapToSwap = IdMaps[index];
        var deletingComponent = LGateComponents[indexMapToSwap];
        Debug.Assert(EntitySystem.IsAlive(componentToSwap.Entity));

        LGateComponents[indexMapToSwap] = componentToSwap;
        LGateComponents[absorbedIndex] = deletingComponent;
        IdMaps[(int)IdStructure.Index(componentToSwap.Entity.Id)] = indexMapToSwap;
        IdMaps[index] = absorbedIndex;

        _freeBackIndexes++;

        //Console.WriteLine($"Count: {LGateComponents.Count} Free: {_freeBackIndexes}");
    }

    internal static LGateComponent Get(Entity entity)
    {
        var mapIndex = IdMaps[(int)IdStructure.Index(entity.Id)];
        Debug.Assert(IsIdMapValid(mapIndex));

        return LGateComponents[mapIndex];
    }

    internal static void Update(LGateComponent lGateComponent)
    {
        var mapIndex = IdMaps[(int)IdStructure.Index(lGateComponent.Entity.Id)];
        Debug.Assert(IsIdMapValid(mapIndex));

        LGateComponents[mapIndex] = lGateComponent;
    }

    internal static IEnumerable<LGateComponent> IterLGateComponents()
    {
        for (var i = 0; i < LGateComponents.Count - _freeBackIndexes; i++)
        {
            yield return LGateComponents[i];
        }
    }

    private static bool IsIdMapValid(int id)
        => id != -1;
}