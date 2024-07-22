using System.Diagnostics;
using GLogicECS.Api;
using GLogicECS.Collections;
using GLogicECS.Components;
using GLogicECS.Components.Common;

namespace GLogic.Jobs.Internal.EcsStateModifiers.Simulations;

internal sealed class InstantSimulation : ICircuitUpdate, IInstantSimulationModifier
{
    private readonly List<(bool state, uint frame)> _statesToReplace;

    private uint _callInterval;
    private uint _currentFrame;
    private Task _stateUpdater;
    private uint _timeSinceLastCall;

    public InstantSimulation(int entitiesCount, uint callInterval)
    {
        _statesToReplace = Enumerable.Repeat((false, (uint)0), entitiesCount).ToList();
        _callInterval = callInterval;
        _currentFrame = 1;

        _stateUpdater = Task.Run(UpdateState);
    }

    public async ValueTask Update(uint deltaTime)
    {
        _timeSinceLastCall += deltaTime;
        if (_timeSinceLastCall < _callInterval)
        {
            return;
        }

        await _stateUpdater;

        for (var i = 0; i < ComponentManager.GetLGateComponentsSpan().Length; i++)
        {
            var entity = ComponentManager.GetLGateComponentsSpan()[i].Entity;
            ComponentManager.UpdateStateComponent(ComponentManager.GetStateComponent(entity) with
            {
                State = _statesToReplace[(int)IdStructure.Index(entity.Id)].state
            });
        }

        for (var i = 0; i < ComponentManager.GetWireComponentsSpan().Length; i++)
        {
            var entity = ComponentManager.GetWireComponentsSpan()[i].Entity;
            ComponentManager.UpdateStateComponent(ComponentManager.GetStateComponent(entity) with
            {
                State = _statesToReplace[(int)IdStructure.Index(entity.Id)].state
            });
        }

        _currentFrame++;
        _timeSinceLastCall = 0;

        _stateUpdater = Task.Run(UpdateState);
    }

    public void Reset()
    {
        _currentFrame++;
        _timeSinceLastCall = 0;
        _stateUpdater.Dispose();
        _stateUpdater = Task.Run(UpdateState);
    }
    
    public void TerminateBackgroundUpdater()
    {
        _stateUpdater.Dispose();
    }

    public void SetInterval(uint interval)
    {
        _callInterval = interval;
    }

    public void IncreaseEntityStatesStorage()
    {
        var entitiesCount = EntityManager.EntitiesCount();
        if (entitiesCount <= _statesToReplace.Count)
        {
            return;
        }

        Debug.Assert(entitiesCount - _statesToReplace.Count == 1);

        _statesToReplace.Add((false, 0));
        Reset();
    }

    private void UpdateState()
    {
        Debug.Assert(_statesToReplace.Count == EntityManager.EntitiesCount());

        foreach (var lGateComponent in ComponentManager.IterLGateComponents())
        {
            Debug.Assert(EntityManager.IsAlive(lGateComponent.Entity));

            PenetrateCircuit(lGateComponent.Entity);
        }
    }

    private bool PenetrateCircuit(Entity entity)
    {
        Debug.Assert(EntityManager.IsAlive(entity));

        var index = (int)IdStructure.Index(entity.Id);

        if (_statesToReplace[index].frame == _currentFrame)
        {
            return _statesToReplace[index].state;
        }

        var newState = CalculateLGateValue(entity);

        _statesToReplace[index] = _statesToReplace[index] with { state = newState };

        return newState;
    }

    private bool CalculateLGateValue(Entity entity)
    {
        var index = (int)IdStructure.Index(entity.Id);
        
        if (_statesToReplace[index].frame != _currentFrame)
        {
            _statesToReplace[index] = _statesToReplace[index] with { frame = _currentFrame };
        }
        
        var ioType = ComponentManager.GetEntityTypeComponent(entity).Type;
        var inputs = ComponentManager.GetInputComponent(entity).Inputs;

        switch (ioType)
        {
            case IoType.AND:
                var andValues = GetValueFrom2Inputs(inputs);

                return andValues.inp1 && andValues.inp2;
            case IoType.OR:
                var orValues = GetValueFrom2Inputs(inputs);

                return orValues.inp1 || orValues.inp2;
            case IoType.XOR:
                var xorValues = GetValueFrom2Inputs(inputs);

                return xorValues.inp1 ^ xorValues.inp2;
            case IoType.NAND:
                var nandValues = GetValueFrom2Inputs(inputs);

                return !(nandValues.inp1 && nandValues.inp2);
            case IoType.NOR:
                var norValues = GetValueFrom2Inputs(inputs);

                return !(norValues.inp1 || norValues.inp2);
            case IoType.XNOR:
                var xnorValues = GetValueFrom2Inputs(inputs);

                return !(xnorValues.inp1 ^ xnorValues.inp2);
            case IoType.LedOutput:
                var ledOutputValues = GetValueFrom2Inputs(inputs);

                return ledOutputValues.inp1 && ledOutputValues.inp2;
            case IoType.NOT:
                var inputNot = inputs.Count == 1
                    ? PenetrateCircuit(inputs[0].Entity)
                    : false;

                return !inputNot;
            case IoType.Wire:
                var inputWire = inputs.Count == 1
                    ? PenetrateCircuit(inputs[0].Entity)
                    : false;

                return inputWire;
            case IoType.Constant:
                return ComponentManager.GetStateComponent(entity).State;
            default:
                throw new ArgumentOutOfRangeException(nameof(ioType), ioType, null);
        }
    }

    private (bool inp1, bool inp2) GetValueFrom2Inputs(SmallList<ConnectionInfo> inputs)
    {
        var i1 = inputs.Count >= 1 ? PenetrateCircuit(inputs[0].Entity) : false;
        var i2 = inputs.Count >= 2 ? PenetrateCircuit(inputs[1].Entity) : false;

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
}