using System.Diagnostics;
using GLogic.Jobs.AppUpdaters;
using GLogicECS.Api;
using GLogicECS.Collections;
using GLogicECS.Components;
using GLogicECS.Components.Common;

namespace GLogic.Jobs.Internal.EcsStateModifiers.LogicCircuitUpdates.Simulations;

internal sealed class InstantSimulation : ICircuitUpdate, IInstantSimulationModifier
{
    private readonly List<bool> _statesToReplace;
    
    private uint _currentFrame;

    private uint _callInterval;
    private uint _timeSinceLastCall; 
    private Task _stateUpdater;

    public InstantSimulation(int entitiesCount, uint callInterval)
    {
        _statesToReplace = Enumerable.Repeat(false, entitiesCount).ToList();
        _callInterval = callInterval;
        _currentFrame = 1;

        InitStateUpdater();
    }

    public async ValueTask Update(uint deltaTime)
    {
        _timeSinceLastCall += deltaTime;
        if (_timeSinceLastCall < _callInterval)
        {
            return;
        }

        await _stateUpdater;

        var i = 0;
        foreach (var stateComponent in ComponentManager.IterStateComponents())
        {
            ComponentManager.UpdateStateComponent(stateComponent with { State = _statesToReplace[i] });
            i++;
        }

        _currentFrame++;
        _timeSinceLastCall = 0;
        
        _stateUpdater = Task.Run(UpdateState);
    }

    public void Reset()
    {
        _currentFrame++;
        _stateUpdater = Task.Run(UpdateState);
    }

    private void UpdateState()
    {
        Debug.Assert(_statesToReplace.Count == EntityManager.EntitiesCount());
        
        foreach (var lGateComponent in ComponentManager.IterLGateComponents())
        {
            Debug.Assert(EntityManager.IsAlive(lGateComponent.Entity));

            var stateComp = ComponentManager.GetStateComponent(lGateComponent.Entity);
            
            PenetrateCircuit(stateComp);
        }
    }

    private bool PenetrateCircuit(StateComponent stateComponent)
    {
        Debug.Assert(EntityManager.IsAlive(stateComponent.Entity));

        var index = (int)IdStructure.Index(stateComponent.Entity.Id);
        
        if (stateComponent.Frame == _currentFrame)
        {
            return _statesToReplace[index];
        }

        var ioType = ComponentManager.GetEntityTypeComponent(stateComponent.Entity).Type;
        var inputs = ComponentManager.GetInputComponent(stateComponent.Entity);
        var newState = CalculateLGateValue(ioType, inputs.Inputs);
        
        _statesToReplace[index] = newState;
        ComponentManager.UpdateStateComponent(stateComponent with { Frame = _currentFrame });

        return newState;
    }
    
    private bool CalculateLGateValue(IoType ioType, SmallList<ConnectionInfo> inputs)
    {
        switch (ioType)
        {
            case IoType.AND:
                var andValues = GetValueFrom2Inputs(inputs);
                
                return andValues.i1 && andValues.i2;
            case IoType.OR:
                var orValues = GetValueFrom2Inputs(inputs);
                
                return orValues.i1 || orValues.i2;
            case IoType.XOR:
                var xorValues = GetValueFrom2Inputs(inputs);
                
                return xorValues.i1 ^ xorValues.i2;
            case IoType.NAND:
                var nandValues = GetValueFrom2Inputs(inputs);
                
                return !(nandValues.i1 && nandValues.i2);
            case IoType.NOR:
                var norValues = GetValueFrom2Inputs(inputs);
                
                return !(norValues.i1 || norValues.i2);
            case IoType.XNOR:
                var xnorValues = GetValueFrom2Inputs(inputs);
                
                return !(xnorValues.i1 ^ xnorValues.i2);
            case IoType.LedOutput:
                var ledOutputValues = GetValueFrom2Inputs(inputs);
                
                return ledOutputValues.i1 && ledOutputValues.i2;
            case IoType.NOT:
                var inputNot = inputs.Count == 1 ? PenetrateCircuit(ComponentManager.GetStateComponent(inputs[0].Entity)) : false;

                return !inputNot;
            case IoType.Wire:
                var inputWire = inputs.Count == 1 ? PenetrateCircuit(ComponentManager.GetStateComponent(inputs[0].Entity)) : false;
                
                return inputWire;
            case IoType.Constant:
                throw new InvalidProgramException("Constant in tree as a leaf");
            default:
                throw new ArgumentOutOfRangeException(nameof(ioType), ioType, null);
        }
    }
    
    private (bool i1, bool i2) GetValueFrom2Inputs(SmallList<ConnectionInfo> inputs)
    {
        var i1 = inputs.Count >= 1 ? PenetrateCircuit(ComponentManager.GetStateComponent(inputs[0].Entity)) : false;
        var i2 = inputs.Count >= 2 ? PenetrateCircuit(ComponentManager.GetStateComponent(inputs[1].Entity)) : false;
        
#if DEBUG
        if (inputs.Count >= 1)
        {
            Debug.Assert(EntityManager.IsAlive(inputs[0].Entity));
        }
        if (inputs.Count >= 2)
        {
            Debug.Assert(EntityManager.IsAlive(inputs[1].Entity));
        }
#endif
        
        return (i1, i2);
    }

    private void InitStateUpdater()
    {
        _stateUpdater = Task.Run(UpdateState);
    }
}