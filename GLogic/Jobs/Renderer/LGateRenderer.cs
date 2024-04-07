using System.Diagnostics;
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

    public void RenderLGate(Area rect, LGate lGate, Placement placement, bool state)
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

    private void RenderChosenLGate()
    {
        SDL.SDL_GetMouseState(out var x, out var y);
        var chosenLGatePosition =
            EntityService.CenterRectPositionToCursor(_rendererStateAccess.GetRelativeShiftedCursor(new Vector2Int(x, y)));
        SDL.SDL_SetRenderDrawColor(_renderer, 181, 14, 0, 8);

        if (UserActionsHandler.ShiftKeyState)
        {
            RenderAdjustedLGate(chosenLGatePosition);
        }
        else
        {
            RenderNotAdjustedLGate(chosenLGatePosition);
        }
    }

    private void RenderNotAdjustedLGate(Vector2Int position)
    {
        var placement = Placement.Valid;
        if (IdStructure.IsValid(EntityService.CheckArea(
                position, ComponentManager.IterLGateComponents().Select(x => x.Entity)).Id)
           )
        {
            placement = Placement.Invalid;
        }

        var rect = new Area(position, EntityService.RectLGateSize)
            .ResizeObjectPlacedOnBackgroundRelatively(
                _rendererStateAccess.Zoom,
                _rendererStateAccess.CameraShift
            );

        var lGate = _textureStorage.ConvertToLGate(UserActionsHandler.ChosenMenuOption);
        
        RenderLGate(rect, lGate, placement, false);
    }
    private void RenderAdjustedLGate(Vector2Int position)
    {
        var placement = Placement.Valid;
        var overlapArea = EntityService.GetLGateOverlapArea(position);
        var overlap = EntityService.GetEntityWithBiggestOverlap(
            out var entityInOverlapArea,
            overlapArea,
            ComponentManager.IterLGateComponents().Select(x => x.Entity)
        );

        if (!overlap)
        {
            return;
        }

        Debug.Assert(entityInOverlapArea.HasValue);

        var adjustedPosition = EntityService.AdjustEntityPosition(position, entityInOverlapArea.Value);

        if (IdStructure.IsValid(EntityService.CheckArea(
                adjustedPosition,
                ComponentManager.IterLGateComponents().Select(x => x.Entity)).Id)
           )
        {
            placement = Placement.Invalid;
        }
        
        var rect = new Area(adjustedPosition, EntityService.RectLGateSize)
            .ResizeObjectPlacedOnBackgroundRelatively(
                _rendererStateAccess.Zoom,
                _rendererStateAccess.CameraShift
            );
        var lGate = _textureStorage.ConvertToLGate(UserActionsHandler.ChosenMenuOption);

        RenderLGate(rect, lGate, placement, false);
    }
    
}