using System.Collections.Immutable;
using GLogic.Components;
using GLogic.Components.Common;
using GLogic.Components.System;
using SDL2;

namespace GLogic.Jobs;

public sealed class Renderer : IRendererConfig
{
    static Renderer()
    {
        _zoom = 1f;
    }
    private static float _zoom;
    public static float Zoom
    {
        get => _zoom;
        private set
        {
            _zoom = value switch
            {
                < 0.1f => 0.1f,
                > 2f => 2f,
                _ => value
            };
        }
    }

    public Area WindowSize = new(new Vector2Int(0, 0), new Vector2Int(1280, 720));
    public Area RenderArea = new(new Vector2Int(-200, -200), new Vector2Int(1680, 1120));
    public static Vector2Int CameraShift { get; set; }
    
    public void RenderEntities(IntPtr renderer)
    {
        SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
        RenderBackgroundEntities(renderer);
        RenderFrontEntities(renderer);
    }
    private void RenderBackgroundEntities(IntPtr renderer)
    {
    }
    private void RenderFrontEntities(IntPtr renderer)
    {
        var entities = ArchetypeManager.IterLGateComponents();

        var renderArea = RenderArea.ResizeRelatively(Zoom, CameraShift);
        
        foreach (var entity in EntityQuery.AABB_Entities(entities.Select(x => x.Entity), renderArea))
        {
            var transformComp = EntityManager.GetTransformComponent(entity);
            var rect = transformComp.ResizeRelatively(Zoom, CameraShift);
            var sdlRect = new SDL.SDL_Rect
            {
                x = rect.Position.X,
                y = rect.Position.Y,
                w = rect.Size.X,
                h = rect.Size.Y,
            };
            
            SDL.SDL_RenderFillRect(renderer, ref sdlRect);
        }
    }

    public void ChangeRelativelyToCursorZoom(float factor, Vector2Int cursor)
    {
        var previousZoom = Zoom;
        Zoom += factor;
        
        CameraShift = new Vector2Int
        {
            X = (int)(cursor.X - (Zoom / previousZoom) * (cursor.X - CameraShift.X)),
            Y = (int)(cursor.Y - (Zoom / previousZoom) * (cursor.Y - CameraShift.Y)),
        };
    }
    
    public void ShiftCamera(Vector2Int shiftVector)
    {
        CameraShift = new Vector2Int(CameraShift.X + shiftVector.X, CameraShift.Y + shiftVector.Y);
    }
}

public readonly record struct Area(Vector2Int Position, Vector2Int Size)
{
    public Area ResizeRelatively(float zoom, Vector2Int cameraShift)
    {
        return new Area
        {
            Position = new Vector2Int((int)((Position.X - cameraShift.X) / zoom), (int)((Position.Y - cameraShift.Y) / zoom)),
            Size = new Vector2Int((int)(Size.X / zoom), (int)(Size.Y / zoom)),
        };
    }
}

public interface IRendererConfig
{
    void ChangeRelativelyToCursorZoom(float factor, Vector2Int cursor);
    void ShiftCamera(Vector2Int shiftVector);
}