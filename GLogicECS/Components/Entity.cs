using GLogicECS.Components.Common;

namespace GLogicECS.Components;

public readonly struct Entity : IEquatable<Entity>
{
    public readonly uint Id;

    public Entity()
    {
        Id = IdStructure.MakeInvalidId();
    }

    public Entity(uint id)
    {
        Id = id;
    }

    public bool Equals(Entity other)
        => Id == other.Id;

    public override bool Equals(object? obj)
        => obj is Entity other && Equals(other);

    public override int GetHashCode()
        => (int)Id;
}