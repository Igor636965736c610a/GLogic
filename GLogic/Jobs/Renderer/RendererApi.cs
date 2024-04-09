using GLogic.Components.Common;
using GLogicECS.Api;

namespace GLogic.Jobs.Renderer;

public sealed class RendererApi : IRendererConfig
{
    private readonly LGateRenderer _lGateRenderer;
    private readonly WireRenderer _wireRenderer;
    private readonly MenuRenderer _menuRenderer;
    private readonly TextureStorage _textureStorage;
    
    private float _zoom;
    
    
    public RendererApi(IntPtr renderer, TextureStorage textureStorage)
    {
        _lGateRenderer = new LGateRenderer(this, renderer, textureStorage);
        _wireRenderer = new WireRenderer(this, renderer, textureStorage);
        _menuRenderer = new MenuRenderer(renderer, textureStorage);
        _textureStorage = new TextureStorage(renderer);
        _zoom = 1f;
    }

    static RendererApi()
    {
        WindowSize = new(new Vector2Int(0, 0), new Vector2Int(1280, 720));
        RenderArea = new(new Vector2Int(-200, -200), new Vector2Int(1680, 1120));
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

    public static readonly Area WindowSize;
    public static readonly Area RenderArea;
    public Vector2Int CameraShift { get; private set; }

    public void RenderEntities()
    {
        RenderBackgroundEntities();
        RenderFrontEntities();
    }
    
    public void RenderMenu()
    {
        _menuRenderer.Render();   
    }

    private void RenderBackgroundEntities()
    {
    }

    private void RenderFrontEntities()
    {
        var renderArea = RenderArea.ResizeBackgroundRelatively(Zoom, CameraShift);

        foreach (var entity in EntityQuery.AABB_Entities(ComponentManager.IterLGateComponents().Select(x => x.Entity),
                     renderArea))
        {
            var transformComponent = ComponentManager.GetTransformComponent(entity);
            var rect = new Area
            {
                Position = transformComponent.Position,
                Size = transformComponent.Size,
            }.ResizeObjectPlacedOnBackgroundRelatively(Zoom, CameraShift);
            
            var typeComponent = ComponentManager.GetEntityTypeComponent(entity);
            var stateComponent = ComponentManager.GetStateComponent(entity);
            var lGate = _textureStorage.ConvertToLGate(typeComponent.Type, stateComponent.State);

            _lGateRenderer.RenderStaticLGate(rect, lGate, Placement.Neutral, stateComponent.State);
        }

        _lGateRenderer.RenderChosenLGateFromMenuOption();
        
        
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
}


public readonly record struct Area(Vector2Int Position, Vector2Int Size)
{
    public Area ResizeBackgroundRelatively(float zoom, Vector2Int cameraShift)
    {
        return new Area
        {
            Position = new Vector2Int((int)((Position.X - cameraShift.X) / zoom),
                (int)((Position.Y - cameraShift.Y) / zoom)),
            Size = new Vector2Int((int)(Size.X / zoom), (int)(Size.Y / zoom))
        };
    }
    
    public Area ResizeObjectPlacedOnBackgroundRelatively(float zoom, Vector2Int cameraShift)
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
    }
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