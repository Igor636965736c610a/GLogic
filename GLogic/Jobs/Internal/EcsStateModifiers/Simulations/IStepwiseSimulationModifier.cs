using GLogicECS.Components;

namespace GLogic.Jobs.Internal.EcsStateModifiers.Simulations;

public interface IStepwiseSimulationModifier
{
    void AddToSimulationQueue(Entity entity);
}