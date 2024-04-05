using System.Diagnostics;
using GLogic.Components;
using GLogic.Components.Common;
using GLogic.Components.System;
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

    public void RenderLGate(Area rect, MenuOption lGate, SDL.SDL_Color rectColor)
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
}