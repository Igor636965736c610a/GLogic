namespace GLogic.Jobs.Internal.EcsStateModifiers.Simulations;

public interface ICircuitUpdate
{
    ValueTask Update(uint deltaTime);
    void Reset();
    void SetInterval(uint interval);
}