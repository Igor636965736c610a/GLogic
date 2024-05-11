using System.Diagnostics;
using GLogicECS.Api;
using GLogicECS.Components;

namespace GLogic.Jobs.Internal.EcsStateModifiers.LogicCircuitUpdates.Simulations;

public class StepWiseSimulation : ICircuitUpdate, IStepWiseSimulationModifier
{
    private readonly Queue<Entity> _entitiesToUpdate;
    
    public StepWiseSimulation(out IUserActionExecutor userActionExecutor)
    {
        _entitiesToUpdate = new Queue<Entity>();
        userActionExecutor = new UserActionExecutorInStepWiseSimMode(this);
    }

    public StepWiseSimulation InitExecutionTree()
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