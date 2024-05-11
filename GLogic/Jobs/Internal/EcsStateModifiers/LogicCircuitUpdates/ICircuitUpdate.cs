namespace GLogic.Jobs.Internal.EcsStateModifiers.LogicCircuitUpdates;

public interface ICircuitUpdate
{ 
    ValueTask Update();
}