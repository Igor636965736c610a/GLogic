using GLogic.Jobs.Internal;
using GLogic.Jobs.Internal.EcsStateModifiers.Simulations;
using GLogicECS.Api;

namespace GLogic.Jobs.AppUpdaters;

public sealed class CircuitUpdater : ICircuitUpdaterConfig
{
    private const uint DefaultCallInterval = 1000;
    public ICircuitUpdate CurrentUpdateCtx { get; private set; } = null!;

    public IUserActionExecutor ToStepWiseSimulation()
    {
        var stepwiseSimulation = new StepwiseSimulation(DefaultCallInterval).InitExecutionQueue();
        CurrentUpdateCtx = stepwiseSimulation;
        CurrentUpdateCtx.Reset();

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
        CurrentUpdateCtx.SetInterval(interval);
    }

    public void Reset()
    {
        CurrentUpdateCtx.Reset();
    }

    public void TerminateBackgroundUpdater()
    {
        CurrentUpdateCtx.TerminateBackgroundUpdater();
    }

    public CircuitUpdater InitDefault(out IUserActionExecutor userActionExecutor)
    {
        userActionExecutor = ToStepWiseSimulation();

        return this;
    }
}