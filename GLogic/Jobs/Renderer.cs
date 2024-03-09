﻿using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using GLogic.Components;
using GLogic.Components.Common;
using GLogic.Components.System;
using SDL2;

namespace GLogic.Jobs;

public sealed class Renderer : IRendererConfig
{
    public Renderer()
    {
        _zoom = 1f;
    }
    private float _zoom;
    public float Zoom
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
    public Area WindowSize { get; } = new(new Vector2Int(0, 0), new Vector2Int(1280, 720));
    public Area RenderArea { get; } = new(new Vector2Int(-200, -200), new Vector2Int(1680, 1120));
    public Vector2Int CameraShift { get; private set; }
    
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
        var renderArea = RenderArea.ResizeRelatively(Zoom, CameraShift);
        
        foreach (var entity in EntityQuery.AABB_Entities(ArchetypeManager.IterLGateComponents().Select(x => x.Entity), renderArea))
        {
            RenderEntity(entity, renderer);
        }
        if (UserActionsHandler.ChosenLGate == LGate.Wire)
        {
            
        }
        else if (UserActionsHandler.ChosenLGate != LGate.None)
        {
            RenderChosenLGate(renderer);
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
    
    public Vector2Int GetRelativeShiftedCursor(Vector2Int cursor, Vector2Int rectSize)
    {
        return new Vector2Int
        {
            X = (int)((cursor.X - CameraShift.X) / Zoom - (rectSize.X / 2f)),
            Y = (int)((cursor.Y - CameraShift.Y) / Zoom - (rectSize.Y / 2f)),
        };
    }
    
    private void RenderChosenLGate(IntPtr renderer)
    {
        SDL.SDL_GetMouseState(out int x, out int y);
        var chosenLGatePosition = GetRelativeShiftedCursor(new Vector2Int(x, y), EntityService.RectLGateSize);
        SDL.SDL_SetRenderDrawColor(renderer, 181, 14, 0, 8);
        if (!UserActionsHandler.ShiftKeyState)
        {
            if (!IdStructure.IsValid(EntityService.CheckArea(chosenLGatePosition).Id))
            {
                SDL.SDL_SetRenderDrawColor(renderer, 201, 242, 155, 1);
            }
            RenderRect(renderer, chosenLGatePosition);
            
            return;
        }
        
        var overlapArea = EntityService.GetLGateOverlapArea(chosenLGatePosition);

        var overlap = EntityService.GetEntityWithBiggestOverlap(out TransformComponent? entityInOverlapArea, overlapArea);
        
        if (!overlap)
        {
            return;
        }
        Debug.Assert(entityInOverlapArea.HasValue);

        var adjustedPosition = EntityService.AdjustEntityPosition(chosenLGatePosition, entityInOverlapArea.Value);
        if (!IdStructure.IsValid(EntityService.CheckArea(adjustedPosition).Id))
        {
            SDL.SDL_SetRenderDrawColor(renderer, 201, 242, 155, 1);
        }

        RenderRect(renderer, adjustedPosition);
    }

    private void RenderRect(IntPtr renderer, Vector2Int position)
    {
        var chosenLGate =
            new TransformComponent { Position = position, Size = EntityService.RectLGateSize, Entity = new Entity { Id = IdStructure.MakeInvalidId() } }
                .ResizeRelatively(Zoom, CameraShift);
        var sdlRect = new SDL.SDL_Rect
        {
            x = chosenLGate.Position.X,
            y = chosenLGate.Position.Y,
            w = chosenLGate.Size.X,
            h = chosenLGate.Size.Y,
        };
        
        SDL.SDL_RenderFillRect(renderer, ref sdlRect);
    }

    private void RenderWithChosenWire(Area renderArea, IntPtr renderer)
    {
        
    }

    private void RenderEntity(Entity entity, IntPtr renderer)
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

public interface IRendererConfig : IRendererStateAccess
{
    void ChangeRelativelyToCursorZoom(float factor, Vector2Int cursor);
    void ShiftCamera(Vector2Int shiftVector);
    Vector2Int GetRelativeShiftedCursor(Vector2Int cursor, Vector2Int rectSize);
}

public interface IRendererStateAccess
{
    Area WindowSize { get; }
    Area RenderArea { get; }
    public Vector2Int CameraShift { get; }
    public float Zoom { get; }
}