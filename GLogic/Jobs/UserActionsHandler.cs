using System.Diagnostics;
using GLogic.Components;
using GLogic.Components.Common;
using SDL2;

namespace GLogic.Jobs;

public sealed class UserActionsHandler
{
    private readonly IRendererConfig _rendererConfig;
    public static MenuOption ChosenMenuOption { get; private set; }
    public static Entity LGateToMove { get; private set; }
    public static bool MouseRightButtonState { get; private set; }
    public static bool ShiftKeyState { get; private set; }

    public UserActionsHandler(IRendererConfig rendererConfig)
    {
        _rendererConfig = rendererConfig;
    }
    
    static UserActionsHandler()
    {
        ChosenMenuOption = MenuOption.None;
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
        else if (ChosenMenuOption == MenuOption.None)
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
        var adjustedCursorPosition = _rendererConfig.GetRelativeShiftedCursor(cursor);
        
        

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
                if (ChosenMenuOption == (MenuOption)i)
                {
                    ChosenMenuOption = MenuOption.None;
                }
                else
                {
                    ChosenMenuOption = (MenuOption)i;
                }
                
                Console.WriteLine(ChosenMenuOption.ToStringFast());
            }
            i++;
        }
    }
    private void UserActions(Vector2Int cursor)
    {
        var adjustedCursorPosition = _rendererConfig.GetRelativeShiftedCursor(cursor);
        switch (ChosenMenuOption)
        {
            case MenuOption.AND:
            {
                var newAdjustedCursorPosition = AdjustCursorPositionWhileAddingEntity(adjustedCursorPosition);
                if (newAdjustedCursorPosition.execute)
                    EntityService.AddLGate(newAdjustedCursorPosition.position, IoType.AND, false);
                
                break;
            }
            case MenuOption.OR:
            {
                var newAdjustedCursorPosition = AdjustCursorPositionWhileAddingEntity(adjustedCursorPosition);
                if (newAdjustedCursorPosition.execute)
                    EntityService.AddLGate(newAdjustedCursorPosition.position, IoType.OR, false);
                
                break;
            }
            case MenuOption.NOT:
            {
                var newAdjustedCursorPosition = AdjustCursorPositionWhileAddingEntity(adjustedCursorPosition);
                if (newAdjustedCursorPosition.execute) 
                    EntityService.AddLGate(newAdjustedCursorPosition.position, IoType.NOT, false);
                
                break;
            }
            case MenuOption.XOR:
            {
                var newAdjustedCursorPosition = AdjustCursorPositionWhileAddingEntity(adjustedCursorPosition);
                if (newAdjustedCursorPosition.execute)
                    EntityService.AddLGate(newAdjustedCursorPosition.position, IoType.XOR, false);
                
                break;
            }
            case MenuOption.NAND:
            {
                var newAdjustedCursorPosition = AdjustCursorPositionWhileAddingEntity(adjustedCursorPosition);
                if (newAdjustedCursorPosition.execute)
                    EntityService.AddLGate(newAdjustedCursorPosition.position, IoType.NAND, false);
                
                break;
            }
            case MenuOption.NOR:
            {
                var newAdjustedCursorPosition = AdjustCursorPositionWhileAddingEntity(adjustedCursorPosition);
                if (newAdjustedCursorPosition.execute)
                    EntityService.AddLGate(newAdjustedCursorPosition.position, IoType.NOR, false);
                
                break;
            }
            case MenuOption.XNOR:
            {
                var newAdjustedCursorPosition = AdjustCursorPositionWhileAddingEntity(adjustedCursorPosition);
                if (newAdjustedCursorPosition.execute)
                    EntityService.AddLGate(newAdjustedCursorPosition.position, IoType.XNOR, false);
                
                break;
            }
            case MenuOption.Input1:
            {
                var newAdjustedCursorPosition = AdjustCursorPositionWhileAddingEntity(adjustedCursorPosition);
                if (newAdjustedCursorPosition.execute)
                    EntityService.AddLGate(newAdjustedCursorPosition.position, IoType.Input, true);
                
                break;
            }
            case MenuOption.Input0:
            {
                var newAdjustedCursorPosition = AdjustCursorPositionWhileAddingEntity(adjustedCursorPosition);
                if (newAdjustedCursorPosition.execute)
                    EntityService.AddLGate(newAdjustedCursorPosition.position, IoType.Input, false);
                
                break;
            }
            case MenuOption.Output:
            {
                var newAdjustedCursorPosition = AdjustCursorPositionWhileAddingEntity(adjustedCursorPosition);
                if (newAdjustedCursorPosition.execute)
                    EntityService.AddLGate(newAdjustedCursorPosition.position, IoType.Output, false);
                
                break;
            }
            case MenuOption.Wire:
            {
                break;
            }
            case MenuOption.Delete:
            {
                EntityService.RemoveEntity(adjustedCursorPosition);
                break;
            }
            case MenuOption.None:
            {
                Debug.Fail("Critical error while creating entity");
                throw new InvalidProgramException("Critical error while creating entity");
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private (bool execute, Vector2Int position) AdjustCursorPositionWhileAddingEntity(Vector2Int position)
    {
        position = EntityService.CenterRectPositionToCursor(position);
        
        if (ShiftKeyState)
        {
            var overlapArea = EntityService.GetLGateOverlapArea(position);
            var overlap = EntityService.GetEntityWithBiggestOverlap(out TransformComponent? entityInOverlapArea,
                overlapArea);
            
            if (!overlap)
            {
                return (false, position);
            }
            Debug.Assert(entityInOverlapArea.HasValue);
            
            return (true, EntityService.AdjustEntityPosition(position, entityInOverlapArea.Value));
        }

        return (true, position);
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
