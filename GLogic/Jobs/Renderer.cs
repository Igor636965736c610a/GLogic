using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using GLogic.Components;
using GLogic.Components.Common;
using GLogic.Components.System;
using SDL2;

namespace GLogic.Jobs;

public sealed class Renderer : IRendererConfig
{
    public Renderer(IntPtr renderer, TextureStorage textureStorage)
    {
        _renderer = renderer;
        _textureStorage = textureStorage;
        _zoom = 1f;
    }

    static Renderer()
    {
        LGateRectColor = new SDL.SDL_Color { r = 90, g = 90, b = 90, a = 90 };
        WindowSize = new(new Vector2Int(0, 0), new Vector2Int(1280, 720));
        RenderArea = new(new Vector2Int(-200, -200), new Vector2Int(1680, 1120));
    }

    private static readonly SDL.SDL_Color LGateRectColor;
    private readonly IntPtr _renderer;
    private readonly TextureStorage _textureStorage;

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

    public static readonly Area WindowSize;
    public static readonly Area RenderArea;
    public Vector2Int CameraShift { get; private set; }

    public void RenderEntities()
    {
        RenderBackgroundEntities();
        RenderFrontEntities();
    }

    private void RenderBackgroundEntities()
    {
    }

    private void RenderFrontEntities()
    {
        var renderArea = RenderArea.ResizeRelatively(Zoom, CameraShift);

        foreach (var entity in EntityQuery.AABB_Entities(ArchetypeManager.IterLGateComponents().Select(x => x.Entity),
                     renderArea))
        {
            var rect = EntityManager.GetTransformComponent(entity).ResizeRelatively(Zoom, CameraShift);
            var ioType = EntityManager.GetEntityTypeComponent(entity); //TODO
            RenderLGate(rect, (MenuOption)ioType.Type, LGateRectColor);
        }

        RenderChosenMenuOption();
    }

    public void ChangeRelativelyToCursorZoom(float factor, Vector2Int cursor)
    {
        var previousZoom = Zoom;
        Zoom += factor;

        CameraShift = new Vector2Int
        {
            X = (int)(cursor.X - Zoom / previousZoom * (cursor.X - CameraShift.X)),
            Y = (int)(cursor.Y - Zoom / previousZoom * (cursor.Y - CameraShift.Y))
        };
    }

    public void ShiftCamera(Vector2Int shiftVector)
    {
        CameraShift = new Vector2Int(CameraShift.X + shiftVector.X, CameraShift.Y + shiftVector.Y);
    }

    public Vector2Int GetRelativeShiftedCursor(Vector2Int cursor)
    {
        return new Vector2Int
        {
            X = (int)((cursor.X - CameraShift.X) / Zoom),
            Y = (int)((cursor.Y - CameraShift.Y) / Zoom)
        };
    }

    private void RenderChosenLGate()
    {
        SDL.SDL_GetMouseState(out var x, out var y);
        var chosenLGatePosition =
            EntityService.CenterRectPositionToCursor(GetRelativeShiftedCursor(new Vector2Int(x, y)));
        SDL.SDL_SetRenderDrawColor(_renderer, 181, 14, 0, 8);
        Area rect;
        var rectColor = new SDL.SDL_Color { r = 201, g = 242, b = 155, a = 1 };;

        if (!UserActionsHandler.ShiftKeyState)
        {
            if (IdStructure.IsValid(EntityService.CheckArea(
                    chosenLGatePosition, ArchetypeManager.IterLGateComponents().Select(x => x.Entity)).Id)
               )
            {
                rectColor = new SDL.SDL_Color { r = 255, g = 99, b = 71, a = 80 };
            }
            rect = new TransformComponent
                    {
                        Position = chosenLGatePosition,
                        Size = EntityService.RectLGateSize,
                        Entity = new Entity { Id = IdStructure.MakeInvalidId() }
                    }
                    .ResizeRelatively(Zoom, CameraShift);

            RenderLGate(rect, UserActionsHandler.ChosenMenuOption, rectColor);

            return;
        }

        var overlapArea = EntityService.GetLGateOverlapArea(chosenLGatePosition);
        var overlap = EntityService.GetEntityWithBiggestOverlap(
            out var entityInOverlapArea,
            overlapArea,
            ArchetypeManager.IterLGateComponents().Select(x => x.Entity)
        );

        if (!overlap)
        {
            return;
        }

        Debug.Assert(entityInOverlapArea.HasValue);

        var adjustedPosition = EntityService.AdjustEntityPosition(chosenLGatePosition, entityInOverlapArea.Value);

        if (IdStructure.IsValid(EntityService.CheckArea(
                adjustedPosition,
                ArchetypeManager.IterLGateComponents().Select(x => x.Entity)).Id)
           )
        {
            rectColor = new SDL.SDL_Color { r = 255, g = 99, b = 71, a = 80 };
        }
        
        rect = new TransformComponent
                {
                    Position = adjustedPosition,
                    Size = EntityService.RectLGateSize,
                    Entity = new Entity { Id = IdStructure.MakeInvalidId() }
                }
                .ResizeRelatively(Zoom, CameraShift);

        RenderLGate(rect, UserActionsHandler.ChosenMenuOption, rectColor);
    }

    private void RenderLGate(Area rect, MenuOption lGate, SDL.SDL_Color rectColor)
    {
        var sdlRect = new SDL.SDL_Rect
        {
            x = rect.Position.X,
            y = rect.Position.Y,
            w = rect.Size.X,
            h = rect.Size.Y
        };
        
        SDL.SDL_SetRenderDrawColor(_renderer, rectColor.r, rectColor.g, rectColor.b, rectColor.a);
        SDL.SDL_RenderFillRect(_renderer, ref sdlRect);
        var texture = _textureStorage.GetLGateTexture(lGate);
        SDL.SDL_RenderCopy(_renderer, texture, (nint)null, ref sdlRect);
    }

    private void RenderWithChosenWire(Area renderArea)
    {
    }

    private void RenderChosenMenuOption()
    {
        switch (UserActionsHandler.ChosenMenuOption)
        {
            case MenuOption.AND:
            {
                RenderChosenLGate();
                break;
            }
            case MenuOption.OR:
            {
                RenderChosenLGate();
                break;
            }
            case MenuOption.NOT:
            {
                RenderChosenLGate();
                break;
            }
            case MenuOption.XOR:
            {
                RenderChosenLGate();
                break;
            }
            case MenuOption.NAND:
            {
                RenderChosenLGate();
                break;
            }
            case MenuOption.NOR:
            {
                RenderChosenLGate();
                break;
            }
            case MenuOption.XNOR:
            {
                RenderChosenLGate();
                break;
            }
            case MenuOption.Input1:
            {
                RenderChosenLGate();
                break;
            }
            case MenuOption.Input0:
            {
                RenderChosenLGate();
                break;
            }
            case MenuOption.Output:
            {
                RenderChosenLGate();
                break;
            }
            case MenuOption.Wire:
            {
                break;
            }
            case MenuOption.Delete:
            {
                break;
            }
            case MenuOption.None:
            {
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public readonly record struct Area(Vector2Int Position, Vector2Int Size)
{
    public Area ResizeRelatively(float zoom, Vector2Int cameraShift)
    {
        return new Area
        {
            Position = new Vector2Int((int)((Position.X - cameraShift.X) / zoom),
                (int)((Position.Y - cameraShift.Y) / zoom)),
            Size = new Vector2Int((int)(Size.X / zoom), (int)(Size.Y / zoom))
        };
    }
}

public interface IRendererConfig : IRendererStateAccess
{
    void ChangeRelativelyToCursorZoom(float factor, Vector2Int cursor);
    void ShiftCamera(Vector2Int shiftVector);
    Vector2Int GetRelativeShiftedCursor(Vector2Int cursor);
}

public interface IRendererStateAccess
{
    public Vector2Int CameraShift { get; }
    public float Zoom { get; }
}