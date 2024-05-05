namespace GLogic.Components.Common;

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
}