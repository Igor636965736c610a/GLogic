using GLogic.Components.Common;
using GLogic.Jobs.Renderer;

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
                X = (int)Math.Round(Position.X * zoom + cameraShift.X, 0),
                Y = (int)Math.Round(Position.Y * zoom + cameraShift.Y, 0)
            },
            Size = new Vector2Int
            {
                X = (int)Math.Round(Size.X * zoom, 0),
                Y = (int)Math.Round(Size.Y * zoom, 0)
            }
        };
        // return new Area
        // {
        //     Position = new Vector2Int
        //     {
        //         X = (int)(Position.X * zoom) + cameraShift.X,
        //         Y = (int)(Position.Y * zoom) + cameraShift.Y,
        //     },
        //     Size = new Vector2Int
        //     {
        //         X = (int)(Size.X * zoom),
        //         Y = (int)(Size.Y * zoom),
        //     }
        // };
    }
}