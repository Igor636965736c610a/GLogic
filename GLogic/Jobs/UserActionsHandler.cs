using System.Diagnostics;
using GLogic.Components;
using GLogic.Components.Common;
using GLogic.Components.System;
using SDL2;

namespace GLogic.Jobs;

public class UserActionsHandler
{
    private readonly IRendererConfig _rendererConfig;
    public static LGate ChosenLGate { get; private set; }
    public static Entity LGateToMove { get; private set; }
    public static bool MouseRightButtonState { get; private set; }
    public static bool ShiftKeyState { get; private set; }

    public UserActionsHandler(IRendererConfig rendererConfig)
    {
        _rendererConfig = rendererConfig;
    }
    
    static UserActionsHandler()
    {
        ChosenLGate = LGate.None;
        MouseRightButtonState = false;
    }
    
    #region Click
    
    public void HandleMouseUpPollEvent(Vector2Int cursor, uint mouseButton)
    {
        if (mouseButton == SDL.SDL_BUTTON_LEFT)
        {
            LGateToMove = new Entity { Id = IdStructure.MakeInvalidId() };
        }

        if (mouseButton == SDL.SDL_BUTTON_RIGHT)
        {
            MouseRightButtonState = false;
        }
    }
    public void HandleMouseDownPollEvent(Vector2Int cursor, uint mouseButton)
    {
        if (mouseButton == SDL.SDL_BUTTON_LEFT && !MouseRightButtonState)
        {
            LeftClickDown(cursor);
        }
        if (mouseButton == SDL.SDL_BUTTON_RIGHT)
        {
            MouseRightButtonState = true;
        }
    }
    public void HandleKeyDownEvent(SDL.SDL_Keycode key)
    {
        if (key == SDL.SDL_Keycode.SDLK_LSHIFT)
        {
            ShiftKeyState = true;
        }
    }
    public void HandleKeyUpEvent(SDL.SDL_Keycode key)
    {
        if (key == SDL.SDL_Keycode.SDLK_LSHIFT)
        {
            ShiftKeyState = false;
        }
    }
    
    private void LeftClickDown(Vector2Int cursor)
    {
        if (cursor.X <= Menu.Width)
        {
            SetChosenLGate(cursor);
        }
        else if (ChosenLGate == LGate.None)
        {
            MarkSomeEntity(cursor);
        }
        else
        {
            UserActions(cursor);
        }
    }
    private void MarkSomeEntity(Vector2Int cursor)
    {
        var markedEntity = new Entity { Id = IdStructure.MakeInvalidId() };
        var adjustedCursorPosition = _rendererConfig.GetRelativeShiftedCursor(cursor, EntityService.RectLGateSize);
        
        

        LGateToMove = markedEntity;
    }
    private static void SetChosenLGate(Vector2Int cursor)
    {
        var i = 0;
        foreach (var option in Menu.MenuOptions)
        {
            if (cursor.X >= option.Position.X && cursor.X <= option.Position.X + option.Size.X
                                             && cursor.Y >= option.Position.Y 
                                             && cursor.Y < option.Position.Y + option.Size.Y)
            {
                if (ChosenLGate == (LGate)i)
                {
                    ChosenLGate = LGate.None;
                }
                else
                {
                    ChosenLGate = (LGate)i;
                }
                
                Console.WriteLine(ChosenLGate.ToStringFast());
            }
            i++;
        }
    }
    private void UserActions(Vector2Int cursor)
    {
        var adjustedCursorPosition = _rendererConfig.GetRelativeShiftedCursor(cursor, EntityService.RectLGateSize);
        if (ShiftKeyState)
        {
            var overlapArea = EntityService.GetLGateOverlapArea(adjustedCursorPosition);
            var overlap = EntityService.GetEntityWithBiggestOverlap(out TransformComponent? entityInOverlapArea,
                overlapArea);
            
            if (!overlap)
            {
                return;
            }
            Debug.Assert(entityInOverlapArea.HasValue);
            
            adjustedCursorPosition = EntityService.AdjustEntityPosition(adjustedCursorPosition, entityInOverlapArea.Value);
        }
        switch (ChosenLGate)
        {
            case LGate.AND:
            {
                EntityService.AddLGate(adjustedCursorPosition, IoType.AND, false);
                break;
            }
            case LGate.OR:
            {
                EntityService.AddLGate(adjustedCursorPosition, IoType.OR, false);
                break;
            }
            case LGate.NOT:
            {
                EntityService.AddLGate(adjustedCursorPosition, IoType.NOT, false);
                break;
            }
            case LGate.XOR:
            {
                EntityService.AddLGate(adjustedCursorPosition, IoType.XOR, false);
                break;
            }
            case LGate.NAND:
            {
                EntityService.AddLGate(adjustedCursorPosition, IoType.NAND, false);
                break;
            }
            case LGate.NOR:
            {
                EntityService.AddLGate(adjustedCursorPosition, IoType.NOR, false);
                break;
            }
            case LGate.XNOR:
            {
                EntityService.AddLGate(adjustedCursorPosition, IoType.XNOR, false);
                break;
            }
            case LGate.Input1:
            {
                EntityService.AddLGate(adjustedCursorPosition, IoType.Input, true);
                break;
            }
            case LGate.Input0:
            {
                EntityService.AddLGate(adjustedCursorPosition, IoType.Input, false);
                break;
            }
            case LGate.Output:
            {
                EntityService.AddLGate(adjustedCursorPosition, IoType.Output, false);
                break;
            }
            case LGate.Wire:
            {
                break;
            }
            case LGate.Delete:
            {
                break;
            }
            case LGate.None:
            {
                Debug.Fail("Critical error while creating entity");
                throw new InvalidProgramException("Critical error while creating entity");
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    #endregion

    #region Held

    public void HandleMouseWheel(Vector2Int cursor, int wheelY, IRendererConfig rendererConfig)
    {
        rendererConfig.ChangeRelativelyToCursorZoom((float)(wheelY * 0.1), cursor);
    }

    #endregion

    #region MouseHeld

    public void HandleMouseRightHeldAction(Vector2Int relativeCursorPosition, IRendererConfig rendererConfig)
    {
        Debug.Assert(MouseRightButtonState);
        rendererConfig.ShiftCamera(relativeCursorPosition);
    }

    #endregion
}
