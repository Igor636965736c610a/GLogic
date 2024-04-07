using System.Collections;
using System.Diagnostics;
using GLogicECS.Components;
using GLogicECS.Components.Common;
using GLogicECS.Components.System;
using GLogicECS.Components.System.DoubleIndexing;

namespace GLogicECS.Api;

public static class ComponentManager
{
    #region Get
    
    
    public static TransformComponent GetTransformComponent(Entity entity)
        => EntitySystem.TransformComponents[(int)IdStructure.Index(entity.Id)];

    public static OutputComponent GetIoComponent(Entity entity)
        => EntitySystem.OutputComponents[(int)IdStructure.Index(entity.Id)];

    public static EntityTypeComponent GetEntityTypeComponent(Entity entity)
        => EntitySystem.EntityTypeComponents[(int)IdStructure.Index(entity.Id)];

    public static StateComponent GetStateComponent(Entity entity)
        => EntitySystem.StateComponents[(int)IdStructure.Index(entity.Id)];
    
    public static InputComponent GetInputComponent(Entity entity)
        => EntitySystem.InputComponents[(int)IdStructure.Index(entity.Id)];
    
    public static OutputComponent GetOutputComponent(Entity entity)
        => EntitySystem.OutputComponents[(int)IdStructure.Index(entity.Id)];
    
    public static WireComponent GetWireComponent(Entity entity)
        => WireComponentSystem.Get(entity);
    
    public static LGateComponent GetLGateComponent(Entity entity)
        => LGateComponentSystem.Get(entity);
    
    public static int GetInputsCountForIoType(IoType ioType)
        => EntitySystem.GetInputsCount(ioType);
    
    
    #endregion

    #region Iter
    
    
    public static IEnumerable<TransformComponent> IterTransformComponents()
        => EntitySystem.TransformComponents;

    public static IEnumerable<OutputComponent> IterIoComponents()
        => EntitySystem.OutputComponents;

    public static IEnumerable<EntityTypeComponent> IterEntityTypeComponents()
        => EntitySystem.EntityTypeComponents;

    public static IEnumerable<InputComponent> IterInputComponents()
        => EntitySystem.InputComponents;

    public static IEnumerable<OutputComponent> IterOutputComponents()
        => EntitySystem.OutputComponents;

    public static IEnumerable<StateComponent> IterStateComponents()
        => EntitySystem.StateComponents;

    public static IEnumerable<LGateComponent> IterLGateComponents()
        => LGateComponentSystem.IterLGateComponents();

    public static IEnumerable<WireComponent> IterWireComponents()
        => WireComponentSystem.IterWireComponents();


    #endregion

    #region Update
    

    public static void UpdateTransformComponent(TransformComponent transformComponent)
    {
        var index = (int)IdStructure.Index(transformComponent.Entity.Id);
        Debug.Assert(EntitySystem.IsAlive(transformComponent.Entity));
        EntitySystem.TransformComponents[index] = transformComponent;
    }
    
    public static void UpdateIoComponent(OutputComponent outputComponent)
    {
        var index = (int)IdStructure.Index(outputComponent.Entity.Id);
        Debug.Assert(EntitySystem.IsAlive(outputComponent.Entity));
        EntitySystem.OutputComponents[index] = outputComponent;
    }
    
    public static void UpdateEntityTypeComponent(EntityTypeComponent entityTypeComponent)
    {
        var index = (int)IdStructure.Index(entityTypeComponent.Entity.Id);
        Debug.Assert(EntitySystem.IsAlive(entityTypeComponent.Entity));
        EntitySystem.EntityTypeComponents[index] = entityTypeComponent;
    }
    
    public static void UpdateInputComponent(InputComponent inputComponent)
    {
        var index = (int)IdStructure.Index(inputComponent.Entity.Id);
        Debug.Assert(EntitySystem.IsAlive(inputComponent.Entity));
        EntitySystem.InputComponents[index] = inputComponent;
    }
    
    public static void UpdateOutputComponent(OutputComponent outputComponent)
    {
        var index = (int)IdStructure.Index(outputComponent.Entity.Id);
        Debug.Assert(EntitySystem.IsAlive(outputComponent.Entity));
        EntitySystem.OutputComponents[index] = outputComponent;
    }
    
    public static void UpdateStateComponent(StateComponent stateComponent)
    {
        var index = (int)IdStructure.Index(stateComponent.Entity.Id);
        Debug.Assert(EntitySystem.IsAlive(stateComponent.Entity));
        EntitySystem.StateComponents[index] = stateComponent;
    }
    
    public static void UpdateLGateComponent(LGateComponent lGateComponent)
    {
        Debug.Assert(EntitySystem.IsAlive(lGateComponent.Entity));
        LGateComponentSystem.Update(lGateComponent);
    }
    
    public static void UpdateWireComponent(WireComponent wireComponent)
    {
        Debug.Assert(EntitySystem.IsAlive(wireComponent.Entity));
        WireComponentSystem.Update(wireComponent);
    }
    
    
    #endregion
}