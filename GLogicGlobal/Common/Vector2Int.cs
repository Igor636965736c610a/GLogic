namespace GLogicGlobal.Common;

public readonly struct Vector2Int
{
    public int X { get; init; }
    public int Y { get; init; }

    public Vector2Int(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static bool operator ==(Vector2Int x, Vector2Int y)
        => x.X == y.X && x.Y == y.Y;

    public static bool operator !=(Vector2Int x, Vector2Int y)
        => x.X != y.X && x.Y != y.Y;
    
    public bool Equals(Vector2Int other)
        => X == other.X && Y == other.Y;

    public override bool Equals(object? obj)
        => obj is Vector2Int other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(X, Y);
}