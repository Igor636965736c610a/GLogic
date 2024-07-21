using GLogic.Data;
using GLogic.Jobs.Internal.EcsStateModifiers;
using GLogicECS.Api;
using GLogicGlobal.Common;
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
        SDL.SDL_Rect sdlRect = info.Rect;
        var texture = _textureStorage.GetLGateTexture(info.LGate, info.State, info.Placement);

        SDL.SDL_RenderCopy(_renderer, texture, (nint)null, ref sdlRect);
    }

    public void RenderChosenLGateFromMenu(LeftPanelOptions option)
    {
        SDL.SDL_GetMouseState(out var x, out var y);

        var info = EntityService.GetDynamicLGateParamsToRender(
            _rendererStateAccess.GetRelativeShiftedCursor(new Vector2Int(x, y)),
            ComponentManager.IterLGateComponents()
        );

        var lGate = _textureStorage.ConvertToLGate(option);
        var rect = new Area(info.position, EntityService.RectLGateSize)
            .ResizeObjectPlacedOnBackgroundRelatively(
                _rendererStateAccess.Zoom,
                _rendererStateAccess.CameraShift
            );
        var renderInfo = new LGateRenderInfo(rect, lGate, info.placement, false);

        RenderStaticLGate(renderInfo);
    }
}

public readonly record struct LGateRenderInfo(Area Rect, LGate LGate, Placement Placement, bool State);