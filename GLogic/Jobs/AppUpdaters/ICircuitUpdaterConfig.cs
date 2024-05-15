using GLogic.Jobs.Internal;

namespace GLogic.Jobs.AppUpdaters;

public interface ICircuitUpdaterConfig
{
    void ToStepWiseSimulation(out IUserActionExecutor userActionExecutor);

    void ToBathSimulation(out IUserActionExecutor userActionExecutor);
}