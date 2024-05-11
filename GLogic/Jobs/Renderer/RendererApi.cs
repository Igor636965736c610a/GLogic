using GLogicECS.Api;
using GLogicECS.Components.Common;
using GLogicGlobal.Common;

namespace GLogic.Jobs.Renderer;

public sealed class RendererApi : IRendererConfig
{
    public static readonly Area WindowSize;
    public static readonly Area RenderArea;
    private readonly LGateRenderer _lGateRenderer;
    private readonly MenuRenderer _menuRenderer;
    private readonly TextureStorage _textureStorage;
    private readonly WireRenderer _wireRenderer;

    private float _zoom;

    static RendererApi()
    {
        WindowSize = new Area(new Vector2Int(0, 0), new Vector2Int(1280, 720));
        RenderArea = new Area(new Vector2Int(-200, -200), new Vector2Int(1680, 1120));
    }

    public RendererApi(IntPtr renderer, TextureStorage textureStorage)
    {
        _lGateRenderer = new LGateRenderer(this, renderer, textureStorage);
        _wireRenderer = new WireRenderer(this, renderer, textureStorage);
        _menuRenderer = new MenuRenderer(renderer, textureStorage);
        _textureStorage = new TextureStorage(renderer);
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
        _menuRenderer.Render();
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

            _wireRenderer.RenderStaticWire(new WireRendererInfo(p1, p2, true));
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
            var lGate = _textureStorage.ConvertToLGate(typeComponent.Type, stateComponent.State);

            var renderInfo = new LGateRenderInfo(rect, lGate, Placement.Neutral, stateComponent.State);
            _lGateRenderer.RenderStaticLGate(renderInfo);
        }

        RenderChosenEntity();
    }

    public static int CalculateShiftRelatively(int value, float zoom, int shift)
        => (int)Math.Round(value * zoom + shift, 0);

    private void RenderChosenEntity()
    {
        switch (UserActionsHandler.ChosenMenuOption)
        {
            case MenuOption.AND:
            case MenuOption.OR:
            case MenuOption.NOT:
            case MenuOption.XOR:
            case MenuOption.NAND:
            case MenuOption.NOR:
            case MenuOption.XNOR:
            case MenuOption.LowConstant:
            case MenuOption.HighConstant:
            case MenuOption.LedOutput:
                _lGateRenderer.RenderChosenLGateFromMenuOption();
                break;
            case MenuOption.Wire:
                _wireRenderer.RenderChosenWireFromMenuOption();
                break;
            case MenuOption.Delete:
            case MenuOption.None:

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
                    cameraShift.Y),
            },
            Size = new Vector2Int
            {
                X = RendererApi.CalculateShiftRelatively(Size.X, zoom, 0),
                Y = RendererApi.CalculateShiftRelatively(Size.Y, zoom, 0),
            }
        };

    public EcsArea ToEcsArea() => new(Position, Size);
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