namespace GLogic.Jobs.AppUpdaters;

public interface ICircuitUpdate
{ 
    ValueTask Update(uint deltaTime);
    void Reset();
}