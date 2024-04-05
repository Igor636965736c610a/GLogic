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
}