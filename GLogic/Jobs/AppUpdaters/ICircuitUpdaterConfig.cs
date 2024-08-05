using GLogic.Jobs.Internal;

namespace GLogic.Jobs.AppUpdaters;

public interface ICircuitUpdaterConfig
{
    IUserActionExecutor ToStepWiseSimulation();

    IUserActionExecutor ToInstantSimulation();
    void SetUpdateCallInterval(uint interval);
    void Reset();
    void TerminateBackgroundUpdater();
}