using GLogic.Jobs.Internal;
using GLogic.Jobs.Internal.EcsStateModifiers.LogicCircuitUpdates.Simulations;

namespace GLogic.Jobs.AppUpdaters;

public sealed class CircuitUpdater : ICircuitUpdaterConfig
{
    private const uint DefaultCallInterval = 1000;
    
    public CircuitUpdater InitDefault(out IUserActionExecutor userActionExecutor)
    {
        userActionExecutor = ToStepWiseSimulation();
        
        return this;
    }

    public ICircuitUpdate CurrentUpdateCtx { get; private set; } = null!;
    
    public IUserActionExecutor ToStepWiseSimulation()
    {
        var stepwiseSimulation = new StepwiseSimulation(DefaultCallInterval).InitExecutionQueue();
        CurrentUpdateCtx = stepwiseSimulation;
        
        return new UserActionExecutorInStepwiseSimMode(stepwiseSimulation);
    }

    public IUserActionExecutor ToInstantSimulation()
    {
        return null!; //TODO
    }
}