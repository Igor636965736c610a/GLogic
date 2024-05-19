using GLogic.Jobs.AppUpdaters;

namespace GLogic.Jobs.Internal.EcsStateModifiers.LogicCircuitUpdates.Simulations;

internal sealed class InstantSimulation : ICircuitUpdate, IInstantSimulationModifier
{
    public InstantSimulation()
    {
        //TODO
    }
    
    public ValueTask Update()
        => throw new NotImplementedException();

    public void Reset()
    {
        throw new NotImplementedException();
    }
}