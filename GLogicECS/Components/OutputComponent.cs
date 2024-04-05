namespace GLogicECS.Components;

public struct EntityOutputComponent
{
    public Entity Entity;
    public List<Entity> Outputs;
    public bool State;

    public EntityOutputComponent(Entity entity, bool state)
    {
        Entity = entity;
        Outputs = new();
        State = state;
    }
}