namespace GLogic.Jobs.Internal.EcsStateModifiers.LogicCircuitUpdates.Simulations;

public interface ICircuitUpdate
{ 
    ValueTask Update(uint deltaTime);
    void Reset();
    void SetInterval(uint interval);
}