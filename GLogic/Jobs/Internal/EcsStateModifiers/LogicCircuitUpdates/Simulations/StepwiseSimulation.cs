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
    
    public StepwiseSimulation()
    {
        _entitiesToUpdate = new Queue<Entity>();
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

    public ValueTask Update()
    {
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
                
                return andValues.Item1 && andValues.Item2;
            case IoType.OR:
                var orValues = GetValueFrom2Inputs(inputs);
                
                return orValues.Item1 || orValues.Item2;
            case IoType.XOR:
                var xorValues = GetValueFrom2Inputs(inputs);
                
                return xorValues.Item1 ^ xorValues.Item2;
            case IoType.NAND:
                var nandValues = GetValueFrom2Inputs(inputs);
                
                return !(nandValues.Item1 && nandValues.Item2);
            case IoType.NOR:
                var norValues = GetValueFrom2Inputs(inputs);
                
                return !(norValues.Item1 || norValues.Item2);
            case IoType.XNOR:
                var xnorValues = GetValueFrom2Inputs(inputs);
                
                return !(xnorValues.Item1 ^ xnorValues.Item2);
            case IoType.LedOutput:
                var ledOutputValues = GetValueFrom2Inputs(inputs);
                
                return ledOutputValues.Item1 && ledOutputValues.Item2;
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

    private (bool, bool) GetValueFrom2Inputs(SmallList<ConnectionInfo> inputs)
    {
        var i1 = inputs.Count >= 1 ? ComponentManager.GetStateComponent(inputs[0].Entity).State : false;
        if (inputs.Count >= 1)
        {
            Debug.Assert(EntityManager.IsAlive(inputs[0].Entity));
        }
        var i2 = inputs.Count >= 2 ? ComponentManager.GetStateComponent(inputs[1].Entity).State : false;
        if (inputs.Count >= 2)
        {
            Debug.Assert(EntityManager.IsAlive(inputs[1].Entity));
        }

        return (i1, i2);
    }
}