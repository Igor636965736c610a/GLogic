namespace GLogicECS.Components.Common;

public struct ConnectionInfo : IEquatable<ConnectionInfo>
{
    public Entity Entity;
    public int HookNumber;

    public ConnectionInfo(Entity entity, int hookNumber)
    {
        Entity = entity;
        HookNumber = hookNumber;
    }

    public bool Equals(ConnectionInfo other)
        => Entity.Equals(other.Entity);

    public override bool Equals(object? obj)
        => obj is ConnectionInfo other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(Entity, HookNumber);
}