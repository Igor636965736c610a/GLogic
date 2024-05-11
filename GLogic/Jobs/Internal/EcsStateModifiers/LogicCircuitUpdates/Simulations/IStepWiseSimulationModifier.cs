using GLogicECS.Components;

namespace GLogic.Jobs.Internal.EcsStateModifiers.LogicCircuitUpdates.Simulations;

public interface IStepWiseSimulationModifier
{
    void AddToSimulationQueue(Entity entity);
}