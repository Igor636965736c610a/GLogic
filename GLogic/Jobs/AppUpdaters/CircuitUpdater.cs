using GLogic.Jobs.Internal;
using GLogic.Jobs.Internal.EcsStateModifiers.LogicCircuitUpdates.Simulations;

namespace GLogic.Jobs.AppUpdaters;

public sealed class CircuitUpdater : ICircuitUpdaterConfig
{
    public CircuitUpdater InitDefault(out IUserActionExecutor userActionExecutor)
    {
        ToStepWiseSimulation(out userActionExecutor);
        
        return this;
    }

    public ICircuitUpdate CurrentUpdateCtx { get; private set; } = null!;
    
    public void ToStepWiseSimulation(out IUserActionExecutor userActionExecutor)
    {
        CurrentUpdateCtx = new StepWiseSimulation(out userActionExecutor).InitExecutionQueue();
    }

    public void ToBathSimulation(out IUserActionExecutor userActionExecutor)
    {
        userActionExecutor = null!; //TODO
    }
}