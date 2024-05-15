using GLogic.Jobs.AppUpdaters;

namespace GLogic.Jobs.Internal.EcsStateModifiers.LogicCircuitUpdates.Simulations;

internal sealed class BathSimulation : ICircuitUpdate, IBathSimulationModifier
{
    public BathSimulation(out IUserActionExecutor userActionExecutor)
    {
        userActionExecutor = null!; //TODO
    }
    
    public ValueTask Update()
        => throw new NotImplementedException();

    public void Reset()
    {
        throw new NotImplementedException();
    }
}