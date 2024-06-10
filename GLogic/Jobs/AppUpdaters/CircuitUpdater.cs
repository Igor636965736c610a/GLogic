using GLogic.Jobs.Internal;
using GLogic.Jobs.Internal.EcsStateModifiers.LogicCircuitUpdates.Simulations;
using GLogicECS.Api;

namespace GLogic.Jobs.AppUpdaters;

public sealed class CircuitUpdater : ICircuitUpdaterConfig
{
    private const uint DefaultCallInterval = 1000;
    public ICircuitUpdate CurrentUpdateCtx { get; private set; } = null!;
    
    public CircuitUpdater InitDefault(out IUserActionExecutor userActionExecutor)
    {
        userActionExecutor = ToInstantSimulation();
        
        return this;
    }

    public IUserActionExecutor ToStepWiseSimulation()
    {
        var stepwiseSimulation = new StepwiseSimulation(DefaultCallInterval).InitExecutionQueue();
        CurrentUpdateCtx = stepwiseSimulation;
        
        return new UserActionExecutorInStepwiseSimMode(stepwiseSimulation);
    }

    public IUserActionExecutor ToInstantSimulation()
    {
        var instantSimulation = new InstantSimulation(EntityManager.EntitiesCount(), DefaultCallInterval);
        CurrentUpdateCtx = instantSimulation;

        return new UserActionExecutorInInstantSimMode(instantSimulation);
    }

    public void SetUpdateCallInterval(uint interval)
    {
        throw new NotImplementedException();
    }
}