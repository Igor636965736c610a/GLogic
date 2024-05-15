namespace GLogic.Jobs.AppUpdaters;

public interface ICircuitUpdate
{ 
    ValueTask Update();
    void Reset();
}