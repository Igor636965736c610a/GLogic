namespace GLogicECS.Components;

public struct Entity : IEquatable<Entity>
{
    public uint Id;

    public Entity(uint id)
    {
        Id = id;
    }

    public bool Equals(Entity other)
    {
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is Entity other && Equals(other);
    }

    public override int GetHashCode()
    {
        return (int)Id;
    }
}