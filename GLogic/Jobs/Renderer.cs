using System.Collections.Immutable;
using GLogic.Components;
using GLogic.Components.Common;
using SDL2;

namespace GLogic.Jobs;

public sealed class Renderer : IRendererConfig
{
    public Renderer()
    {
        EntitiesToRender = ImmutableList<Entity>.Empty;
    }
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

    public ImmutableList<Entity> EntitiesToRender { get; private set; }
    public Area WindowSize = new(1280, 720);
    public Area RenderArea = new(1480, 920); 
    public Vector2Int CameraShift { get; private set; }

    public void UpdateEntitiesToRender()
    {
        EntitiesToRender = GetEntitiesToRender().ToImmutableList();
    }
    private IEnumerable<Entity> GetEntitiesToRender()
    {
        var transformComponents = EntityManager.IterTransformComponents();
        var min = new Vector2Int { X = CameraShift.X - Zoom, Y = CameraShift.Y - Zoom };
        var max = new Vector2Int { X = CameraShift.X + RenderArea.Width + Zoom, Y = CameraShift.Y + RenderArea.High + Zoom };
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
    public void RenderEntities(IntPtr renderer)
    {
        SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
        var frontEntities = RenderBackgroundEntities(renderer).ToImmutableList();
        RenderFrontEntities(frontEntities, renderer);
    }
    private IEnumerable<Entity> RenderBackgroundEntities(IntPtr renderer) // It return front entities to render
    {
        foreach (var entity in EntitiesToRender)
        {
            if (EntityManager.GetIoComponent(entity).IoType == IoType.Wire)
            {
                yield return entity;
                continue;
            }

            var transformComp = EntityManager.GetTransformComponent(entity);
            var rect = new SDL.SDL_Rect
            {
                x = transformComp.Position.X,
                y = transformComp.Position.Y,
                w = transformComp.Size.X + Zoom,
                h = transformComp.Size.Y + Zoom,
            };
            SDL.SDL_RenderFillRect(renderer, ref rect);
        }
    }
    private void RenderFrontEntities(IEnumerable<Entity> entities, IntPtr renderer)
    {
        
    }
    public void ZoomIn()
        => Zoom -= 10;
    public void ZoomOut()
        => Zoom += 10;
    public void ShiftCamera(Vector2Int shiftVector)
    {
        CameraShift = new Vector2Int { X = CameraShift.X + shiftVector.X, Y = CameraShift.Y + shiftVector.Y };
    }
}
public readonly record struct Area(int Width, int High);

public interface IRendererConfig
{
    void ZoomIn();
    void ZoomOut();
    void ShiftCamera(Vector2Int shiftVector);
}