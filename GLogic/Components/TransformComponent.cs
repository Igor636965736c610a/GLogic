using GLogic.Components.Common;
using GLogic.Jobs;

namespace GLogic.Components;

public struct TransformComponent
{
    public Entity Entity;
    public Vector2Int Position;
    public Vector2Int Size;

    public Area ResizeRelatively(float zoom, Vector2Int cameraShift)
    {
        return new Area
        {
            Position = new Vector2Int
            {
                X = (int)(Position.X * zoom) + cameraShift.X,
                Y = (int)(Position.Y * zoom) + cameraShift.Y,
            },
            Size = new Vector2Int
            {
                X = (int)(Size.X * zoom),
                Y = (int)(Size.Y * zoom),
            }
        };
    }
}