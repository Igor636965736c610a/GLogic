﻿using GLogic.Data;
using GLogic.Data.Panels;
using GLogic.Data.State;
using GLogic.Data.TextureStorage;
using GLogicECS.Api;
using GLogicGlobal.Common;
using SDL2;

namespace GLogic.Jobs.Renderer;

public sealed class RendererApi : IRendererConfig
{
    public static readonly Area RenderArea;
    private readonly LGateRenderer _lGateRenderer;
    private readonly LayoutRenderer _layoutRenderer;
    private readonly TextureStorageProvider _textureStorageProvider;
    private readonly WireRenderer _wireRenderer;
    private readonly BackgroundRenderer _backgroundRenderer;

    private float _zoom;

    static RendererApi()
    {
        RenderArea = new Area(new Vector2Int(-200, -200), new Vector2Int(1680, 1120));
    }

    public RendererApi(IntPtr renderer, LayoutArrangement layoutArrangement, TextureStorageProvider textureStorageProvider)
    {
        _lGateRenderer = new LGateRenderer(this, renderer, textureStorageProvider.LGateTextureStorage);
        _wireRenderer = new WireRenderer(this, renderer, textureStorageProvider);
        _layoutRenderer = new LayoutRenderer(renderer, layoutArrangement, textureStorageProvider.LeftPanelTextureStorage, textureStorageProvider.TopPanelTextureStorage);
        _textureStorageProvider = new TextureStorageProvider(renderer);
        _backgroundRenderer = new BackgroundRenderer(this, renderer);
        _zoom = 1f;
    }

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

    public Vector2Int CameraShift { get; private set; }

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
        => new()
        {
            X = (int)((cursor.X - CameraShift.X) / Zoom),
            Y = (int)((cursor.Y - CameraShift.Y) / Zoom)
        };

    public void RenderEntities()
    {
        RenderBackgroundEntities();
        RenderFrontEntities();
    }

    public void RenderMenu()
    {
        _layoutRenderer.Render();
    }

    public void RenderGrid()
    {
        _backgroundRenderer.RenderGrid();
    }

    private void RenderBackgroundEntities()
    {
        var renderArea = RenderArea.ResizeBackgroundRelatively(Zoom, CameraShift);

        foreach (var wireComp in EntityQuery.AABB_Entities(ComponentManager.IterWireComponents(),
                     renderArea))
        {
            var p1 = new Vector2Int(
                CalculateShiftRelatively(wireComp.P1.X, Zoom, CameraShift.X),
                CalculateShiftRelatively(wireComp.P1.Y, Zoom, CameraShift.Y)
            );
            var p2 = new Vector2Int(
                CalculateShiftRelatively(wireComp.P2.X, Zoom, CameraShift.X),
                CalculateShiftRelatively(wireComp.P2.Y, Zoom, CameraShift.Y)
            );
            var state = ComponentManager.GetStateComponent(wireComp.Entity).State;

            _wireRenderer.RenderStaticWire(new WireRendererInfo(p1, p2, state));
        }
    }

    private void RenderFrontEntities()
    {
        var renderArea = RenderArea.ResizeBackgroundRelatively(Zoom, CameraShift);

        foreach (var lGateComp in EntityQuery.AABB_Entities(ComponentManager.IterLGateComponents(),
                     renderArea))
        {
            var entity = lGateComp.Entity;

            var transformComponent = ComponentManager.GetTransformComponent(entity);
            var rect = new Area
            {
                Position = transformComponent.Position,
                Size = transformComponent.Size
            }.ResizeObjectPlacedOnBackgroundRelatively(Zoom, CameraShift);

            var typeComponent = ComponentManager.GetEntityTypeComponent(entity);
            var stateComponent = ComponentManager.GetStateComponent(entity);

            _lGateRenderer.RenderStaticLGate(rect, typeComponent.Type, LGatePlacementF.Neutral, stateComponent.State);
        }

        RenderChosenEntity();
    }

    public static int CalculateShiftRelatively(int value, float zoom, int shift)
        => (int)Math.Round(value * zoom + shift);

    private void RenderChosenEntity()
    {
        switch (MenuState.ChosenLGate)
        {
            case LeftPanelOption.AND:
            case LeftPanelOption.OR:
            case LeftPanelOption.NOT:
            case LeftPanelOption.XOR:
            case LeftPanelOption.NAND:
            case LeftPanelOption.NOR:
            case LeftPanelOption.XNOR:
            case LeftPanelOption.LowConstant:
            case LeftPanelOption.HighConstant:
            case LeftPanelOption.LedOutput:
                _lGateRenderer.RenderChosenLGateFromMenu(MenuState.ChosenLGate);
                break;
            case LeftPanelOption.Wire:
                _wireRenderer.RenderChosenWireFromMenuOption();
                break;
            case LeftPanelOption.Delete:
            case LeftPanelOption.None:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public readonly record struct Area(Vector2Int Position, Vector2Int Size)
{
    public Area ResizeBackgroundRelatively(float zoom, Vector2Int cameraShift)
        => new()
        {
            Position = new Vector2Int((int)((Position.X - cameraShift.X) / zoom),
                (int)((Position.Y - cameraShift.Y) / zoom)),
            Size = new Vector2Int((int)(Size.X / zoom), (int)(Size.Y / zoom))
        };

    public Area ResizeObjectPlacedOnBackgroundRelatively(float zoom, Vector2Int cameraShift)
        => new()
        {
            Position = new Vector2Int
            {
                X = RendererApi.CalculateShiftRelatively(Position.X, zoom,
                    cameraShift.X),
                Y = RendererApi.CalculateShiftRelatively(Position.Y, zoom,
                    cameraShift.Y)
            },
            Size = new Vector2Int
            {
                X = RendererApi.CalculateShiftRelatively(Size.X, zoom, 0),
                Y = RendererApi.CalculateShiftRelatively(Size.Y, zoom, 0)
            }
        };
    
    public static implicit operator SDL.SDL_Rect(Area area) => new()
    {
        x = area.Position.X,
        y = area.Position.Y,
        w = area.Size.X,
        h = area.Size.Y,
    };
}

public interface IRendererConfig : IRendererStateAccess
{
    void ChangeRelativelyToCursorZoom(float factor, Vector2Int cursor);
    void ShiftCamera(Vector2Int shiftVector);
}

public interface IRendererStateAccess
{
    Vector2Int CameraShift { get; }
    float Zoom { get; }
    Vector2Int GetRelativeShiftedCursor(Vector2Int cursor);
}