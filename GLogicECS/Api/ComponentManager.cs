using GLogicECS.Components;
using GLogicECS.Components.Common;
using GLogicECS.Components.System;
using GLogicECS.Components.System.DoubleIndexing;

namespace GLogicECS.Api;

public static class EcsManager
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
    
    
    #endregion

    #region Iter
    
    
    public static IEnumerable<TransformComponent> IterTransformComponents()
        => EntitySystem.TransformComponents;

    public static IEnumerable<OutputComponent> IterIoComponents()
        => EntitySystem.OutputComponents;

    public static IEnumerable<EntityTypeComponent> IterEntityTypeComponents()
        => EntitySystem.EntityTypeComponents;
    
    
    #endregion

    public static void UpdateIoComponent(OutputComponent outputComponent)
    {
        var index = (int)IdStructure.Index(outputComponent.Entity.Id);
        EntitySystem.OutputComponents[index] = outputComponent;
    }

    public static void UpdateTransformComponent(TransformComponent transformComponent)
    {
        var index = (int)IdStructure.Index(transformComponent.Entity.Id); //TODO Assertions
        EntitySystem.TransformComponents[index] = transformComponent;
    }
}