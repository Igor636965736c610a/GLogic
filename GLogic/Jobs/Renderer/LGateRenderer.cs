using GLogic.Components.Common;
using GLogicECS.Api;
using SDL2;

namespace GLogic.Jobs.Renderer;

public sealed class LGateRenderer
{
    private readonly IntPtr _renderer;
    private readonly IRendererStateAccess _rendererStateAccess;
    private readonly TextureStorage _textureStorage;

    public LGateRenderer(IRendererStateAccess rendererStateAccess, IntPtr renderer, TextureStorage textureStorage)
    {
        _rendererStateAccess = rendererStateAccess;
        _renderer = renderer;
        _textureStorage = textureStorage;
    }

    public void RenderStaticLGate(LGateRenderInfo info)
    {
        var sdlRect = new SDL.SDL_Rect
        {
            x = info.Rect.Position.X,
            y = info.Rect.Position.Y,
            w = info.Rect.Size.X,
            h = info.Rect.Size.Y
        };

        var texture = _textureStorage.GetLGateTexture(info.LGate, info.State, info.Placement);
        SDL.SDL_RenderCopy(_renderer, texture, (nint)null, ref sdlRect);
    }

    public void RenderChosenLGateFromMenuOption()
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
            case MenuOption.Input0:
            case MenuOption.Input1:
            case MenuOption.Output:
                SDL.SDL_GetMouseState(out var x, out var y);

                var info = EntityService.GetDynamicLGateParamsToRender(
                    _rendererStateAccess.GetRelativeShiftedCursor(new Vector2Int(x, y)),
                    ComponentManager.IterLGateComponents()
                );

                var lGate = _textureStorage.ConvertToLGate(UserActionsHandler.ChosenMenuOption);
                var rect = new Area(info.position, EntityService.RectLGateSize)
                    .ResizeObjectPlacedOnBackgroundRelatively(
                        _rendererStateAccess.Zoom,
                        _rendererStateAccess.CameraShift
                    );

                var renderInfo = new LGateRenderInfo(rect, lGate, info.placement, false);
                RenderStaticLGate(renderInfo);

                break;
            case MenuOption.Wire:
            case MenuOption.Delete:
            case MenuOption.None:

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public readonly record struct LGateRenderInfo(Area Rect, LGate LGate, Placement Placement, bool State);