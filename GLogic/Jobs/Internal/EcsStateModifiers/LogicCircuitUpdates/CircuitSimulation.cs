using System.Diagnostics;
using GLogicECS.Api;
using GLogicECS.Components;

namespace GLogic.Jobs.Internal.EcsStateModifiers.LogicCircuitUpdates;

public class CircuitSimulation : ICircuitUpdate, ISimulationModifier
{
    private readonly Queue<Entity> _entitiesToUpdate;
    
    public CircuitSimulation()
    {
        _entitiesToUpdate = new Queue<Entity>();
    }

    public CircuitSimulation InitExecutionTree()
    {
        //TODO
        
        return this;
    }

    public void AddToSimulationQueue(Entity entity)
    {
        Debug.Assert(EntityManager.IsAlive(entity));
        
        _entitiesToUpdate.Enqueue(entity);
    }

    public ValueTask Update()
    {
        return ValueTask.CompletedTask;
    }
}