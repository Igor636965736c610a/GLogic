using GLogicECS.Collections;

namespace GLogicECS.Components.System;

public class InputComponent
{
    public Entity Entity;
    public SmallList<Entity> Inputs;

    public InputComponent(Entity entity, SmallList<Entity> inputs)
    {
        Entity = entity;
        Inputs = inputs;
    }
}