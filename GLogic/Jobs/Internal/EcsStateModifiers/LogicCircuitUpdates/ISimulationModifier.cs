using GLogicECS.Components;

namespace GLogic.Jobs.Internal.EcsStateModifiers.LogicCircuitUpdates;

public interface ISimulationModifier
{
    void AddToSimulationQueue(Entity entity);
}