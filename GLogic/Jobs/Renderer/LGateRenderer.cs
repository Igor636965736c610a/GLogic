using GLogic.Data;
using GLogic.Data.Panels;
using GLogic.Data.TextureStorage;
using GLogic.Jobs.Internal.EcsStateModifiers;
using GLogicECS.Api;
using GLogicECS.Components;
using GLogicGlobal.Common;
using SDL2;

namespace GLogic.Jobs.Renderer;

public sealed class LGateRenderer
{
    private readonly IntPtr _renderer;
    private readonly IRendererStateAccess _rendererStateAccess;
    private readonly LGateTextureStorage _lGateTextureStorage;

    public LGateRenderer(IRendererStateAccess rendererStateAccess, IntPtr renderer, LGateTextureStorage lGateTextureStorage)
    {
        _rendererStateAccess = rendererStateAccess;
        _renderer = renderer;
        _lGateTextureStorage = lGateTextureStorage;
    }

    public void RenderStaticLGate(Area rect, IoType ioType, LGatePlacementF lGatePlacementF, bool state)
    {
        var lGateF = _lGateTextureStorage.ConvertToLGateF(ioType, state);
        var lGateRendererInfo = new LGateRenderInfo(rect, lGateF, lGatePlacementF, state);
        
        Render(lGateRendererInfo);
    }
    
    public void RenderStaticLGate(Area rect, LeftPanelOption leftPanelOption, LGatePlacementF lGatePlacementF, bool state)
    {
        var lGateF = _lGateTextureStorage.ConvertToLGateF(leftPanelOption);
        var lGateRendererInfo = new LGateRenderInfo(rect, lGateF, lGatePlacementF, state);
        
        Render(lGateRendererInfo);
    }

    public void RenderChosenLGateFromMenu(LeftPanelOption option)
    {
        SDL.SDL_GetMouseState(out var x, out var y);

        var info = EntityService.GetDynamicLGateParamsToRender(
            _rendererStateAccess.GetRelativeShiftedCursor(new Vector2Int(x, y)),
            ComponentManager.IterLGateComponents()
        );
        
        var rect = new Area(info.position, EntityService.RectLGateSize)
            .ResizeObjectPlacedOnBackgroundRelatively(
                _rendererStateAccess.Zoom,
                _rendererStateAccess.CameraShift
            );

        RenderStaticLGate(rect, option , info.placement, false);
    }

    private void Render(LGateRenderInfo info)
    {
        SDL.SDL_Rect sdlRect = info.Rect;
        var texture = _lGateTextureStorage.GetLGateTexture(info.LGateF, info.State, info.LGatePlacementF);

        SDL.SDL_RenderCopy(_renderer, texture, (nint)null, ref sdlRect);
    }
    private readonly record struct LGateRenderInfo(Area Rect, LGateF LGateF, LGatePlacementF LGatePlacementF, bool State);
}

