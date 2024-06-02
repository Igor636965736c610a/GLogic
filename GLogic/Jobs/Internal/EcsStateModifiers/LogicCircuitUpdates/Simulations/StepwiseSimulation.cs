using System.Diagnostics;
using GLogic.Jobs.AppUpdaters;
using GLogicECS.Api;
using GLogicECS.Collections;
using GLogicECS.Components;
using GLogicECS.Components.Common;

namespace GLogic.Jobs.Internal.EcsStateModifiers.LogicCircuitUpdates.Simulations;

internal sealed class StepwiseSimulation : ICircuitUpdate, IStepwiseSimulationModifier
{
    private readonly Queue<Entity> _entitiesToUpdate;
    
    private uint _callInterval;
    private uint _timeSinceLastCall;
    
    public StepwiseSimulation(uint callInterval)
    {
        _entitiesToUpdate = new Queue<Entity>();
        _callInterval = callInterval;
    }

    public StepwiseSimulation InitExecutionQueue()
    {
        AddFirstEntitiesToQueue();
        
        return this;
    }

    public void AddToSimulationQueue(Entity entity)
    {
        Debug.Assert(EntityManager.IsAlive(entity));
        
        _entitiesToUpdate.Enqueue(entity);
    }

    public ValueTask Update(uint deltaTime)
    {
        _timeSinceLastCall += deltaTime;
        if (_timeSinceLastCall < _callInterval)
        {
            return ValueTask.CompletedTask;
        }
        
        var count = _entitiesToUpdate.Count;
        while (count-- > 0)
        {
            var entityToCalculate = _entitiesToUpdate.Dequeue();
            if (!EntityManager.IsAlive(entityToCalculate))
            {
                continue;
            }
            
            var entityTypeComp = ComponentManager.GetEntityTypeComponent(entityToCalculate);
            var inputComp = ComponentManager.GetInputComponent(entityToCalculate);
            var stateComp = ComponentManager.GetStateComponent(entityToCalculate);

            var newValue = CalculateLGateValue(entityTypeComp.Type, inputComp.Inputs);

            ComponentManager.UpdateStateComponent(stateComp with{ State = newValue });
            var outputComp = ComponentManager.GetOutputComponent(entityToCalculate);
            for (int i = 0; i < outputComp.Outputs.Count; i++)
            {
                Debug.Assert(EntityManager.IsAlive(outputComp.Outputs[i].Entity));
                _entitiesToUpdate.Enqueue(outputComp.Outputs[i].Entity);
            }
        }
        
        _timeSinceLastCall = 0;

        return ValueTask.CompletedTask;
    }

    public void Reset()
    {
        InitExecutionQueue();
    }

    private void AddFirstEntitiesToQueue()
    {
        var entityTypes = ComponentManager.IterEntityTypeComponents();

        foreach (var typeComponent in entityTypes)
        {
            if (!EntityManager.IsAlive(typeComponent.Entity))
            {
                continue;
            }

            if (typeComponent.Type != IoType.Constant)
            {
                continue;
            }

            var outputs = ComponentManager.GetOutputComponent(typeComponent.Entity);
            Debug.Assert(EntityManager.IsAlive(outputs.Entity));
            
            for (int i = 0; i < outputs.Outputs.Count; i++)
            {
                var outputEntity = outputs.Outputs[i].Entity;
                
                Debug.Assert(!EntityManager.IsAlive(outputEntity));
                // if (!EntityManager.IsAlive(outputEntity))
                // {
                //     continue;
                // }
                Debug.Assert(ComponentManager.GetEntityTypeComponent(outputEntity).Type == IoType.Wire);
                
                _entitiesToUpdate.Enqueue(outputEntity);
            }
        }
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
                var inputNot = inputs.Count == 1 ? ComponentManager.GetStateComponent(inputs[0].Entity).State : false;

                return !inputNot;
            case IoType.Wire:
                var inputWire = inputs.Count == 1 ? ComponentManager.GetStateComponent(inputs[0].Entity).State : false;
                
                return inputWire;
            case IoType.Constant:
                throw new InvalidProgramException("Constant in tree as a leaf");
            default:
                throw new ArgumentOutOfRangeException(nameof(ioType), ioType, null);
        }
    }

    private (bool i1, bool i2) GetValueFrom2Inputs(SmallList<ConnectionInfo> inputs)
    {
        var i1 = inputs.Count >= 1 ? ComponentManager.GetStateComponent(inputs[0].Entity).State : false;
        var i2 = inputs.Count >= 2 ? ComponentManager.GetStateComponent(inputs[1].Entity).State : false;
        
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