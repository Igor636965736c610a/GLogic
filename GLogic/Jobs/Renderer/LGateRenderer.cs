using GLogic.Components.Common;
using GLogicECS.Api;
using GLogicECS.Components.Common;
using SDL2;

namespace GLogic.Jobs.Renderer;

public sealed class LGateRenderer
{
    private readonly IRendererStateAccess _rendererStateAccess;
    private readonly IntPtr _renderer;
    private readonly TextureStorage _textureStorage;

    public LGateRenderer(IRendererStateAccess rendererStateAccess, IntPtr renderer, TextureStorage textureStorage)
    {
        _rendererStateAccess = rendererStateAccess;
        _renderer = renderer;
        _textureStorage = textureStorage;
    }

    public void RenderStaticLGate(Area rect, LGate lGate, Placement placement, bool state)
    {
        var sdlRect = new SDL.SDL_Rect
        {
            x = rect.Position.X,
            y = rect.Position.Y,
            w = rect.Size.X, 
            h = rect.Size.Y
        };
        
        var texture = _textureStorage.GetLGateTexture(lGate, state, placement);
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
                    ComponentManager.IterLGateComponents().Select(x => x.Entity)
                );
                
                var lGate = _textureStorage.ConvertToLGate(UserActionsHandler.ChosenMenuOption);
                var rect = new Area(info.position, EntityService.RectLGateSize)
                    .ResizeObjectPlacedOnBackgroundRelatively(
                        _rendererStateAccess.Zoom,
                        _rendererStateAccess.CameraShift
                    );
                
                RenderStaticLGate(rect, lGate, info.placement, false);
                break;
            case MenuOption.Wire:
            case MenuOption.Delete:
            case MenuOption.None:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void RenderShiftingLGate()
    {
        if (!IdStructure.IsValid(UserActionsHandler.LGateToMove.Id))
        {
            return;
        }

        SDL.SDL_GetMouseState(out var x, out var y);

        var info = EntityService.GetDynamicLGateParamsToRender(
            _rendererStateAccess.GetRelativeShiftedCursor(new Vector2Int(x, y)), 
            ComponentManager.IterLGateComponents().Select(x => x.Entity)
            );
        
        var rect = new Area(info.position, EntityService.RectLGateSize)
                 .ResizeObjectPlacedOnBackgroundRelatively(
                     _rendererStateAccess.Zoom,
                     _rendererStateAccess.CameraShift
                 );
    }
}