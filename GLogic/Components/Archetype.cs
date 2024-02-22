namespace GLogic.Components;

public struct Archetype
{
    public Entity Entity;
    public ArchetypeOption ArchetypeOption;
    public int IndexOfArchetypeOption;
}

public enum ArchetypeOption
{
    Wire,
    LGate,
}