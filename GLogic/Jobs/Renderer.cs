using GLogic.Components;
using GLogic.Components.Common;

namespace GLogic.Jobs;

public sealed class Renderer
{
    private int _zoom = 0;
    public int Zoom
    {
        get => _zoom;
        private set
        {
            var newZoom = _zoom + value;
            _zoom = newZoom switch
            {
                < 0 => 0,           //in
                > 1000 => 1000,     //out
                _ => newZoom
            };
        }
    }
    public Area WindowSize = new(1280, 720);
    public Area RenderArea = new(1480, 920); 
    public Vector2Int CameraShift { get; private set; }
    
    public IEnumerable<Entity> EntitiesToRender()
    {
        var transformComponents = EntityManager.IterTransformComponents();
        var min = new Vector2Int { X = CameraShift.X - _zoom, Y = CameraShift.Y - _zoom };
        var max = new Vector2Int { X = CameraShift.X + RenderArea.Width + _zoom, Y = CameraShift.Y + RenderArea.High + _zoom };
        foreach (var transformComponent in transformComponents)
        {
            if (!EntityManager.IsAlive(transformComponent.Entity))
                continue;
            var center = new Vector2Int
            {
                X = transformComponent.Position.X + transformComponent.Size.X / 2,
                Y = transformComponent.Position.Y + transformComponent.Size.Y / 2,
            };
            if (center.X > min.X && center.X < max.X && center.Y > min.Y && center.Y < max.Y) //AABB
            {
                yield return transformComponent.Entity;
            }
        }
    }
    public void ZoomIn()
        => _zoom -= 10;
    public void ZoomOut()
        => _zoom += 10;
    public void ShiftCamera(Vector2Int shiftVector)
    {
        CameraShift = new Vector2Int { X = CameraShift.X + shiftVector.X, Y = CameraShift.Y + shiftVector.Y };
    }    
}
public readonly record struct Area(int Width, int High); 


