using System.Diagnostics;
using GLogic.Components.Common;
using GLogic.Jobs.Renderer;
using GLogicECS.Api;
using GLogicECS.Components;
using GLogicECS.Components.Common;
using SDL2;

namespace GLogic.Jobs;

public sealed class UserActionsHandler
{
    private readonly IRendererConfig _rendererConfig;
    public static MenuOption ChosenMenuOption { get; private set; }
    public static Entity LGateToMove { get; private set; }
    public static bool MouseRightButtonState { get; private set; }
    public static bool MouseLeftButtonState { get; private set; }
    public static bool ShiftKeyState { get; private set; }

    public UserActionsHandler(IRendererConfig rendererConfig)
    {
        _rendererConfig = rendererConfig;
        LGateToMove = new Entity { Id = IdStructure.MakeInvalidId() };
    }

    static UserActionsHandler()
    {
        ChosenMenuOption = MenuOption.None;
        MouseRightButtonState = false;
    }

    #region MouseClick

    public void HandleMouseUpPollEvent(Vector2Int cursor, uint mouseButton)
    {
        if (mouseButton == SDL.SDL_BUTTON_LEFT)
        {
            LGateToMove = new Entity { Id = IdStructure.MakeInvalidId() };
            MouseLeftButtonState = false;
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

            MouseLeftButtonState = true;
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
        if (cursor.X <= MenuRenderer.Width)
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
        var adjustedCursorPosition = _rendererConfig.GetRelativeShiftedCursor(cursor);

        var markedEntity = EntityQuery.AABB_Entities(
            ComponentManager.IterLGateComponents().Select(x => x.Entity),
            adjustedCursorPosition
        ).FirstOrDefault(new Entity { Id = IdStructure.MakeInvalidId() });

        LGateToMove = markedEntity;
        Console.WriteLine(LGateToMove.Id);
    }

    private static void SetChosenLGate(Vector2Int cursor)
    {
        var i = 0;
        foreach (var option in MenuRenderer.MenuOptions)
        {
            if (
                cursor.X >= option.Position.X
                && cursor.X <= option.Position.X + option.Size.X
                && cursor.Y >= option.Position.Y
                && cursor.Y < option.Position.Y + option.Size.Y
            )
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
            case MenuOption.OR:
            case MenuOption.NOT:
            case MenuOption.XOR:
            case MenuOption.NAND:
            case MenuOption.NOR:
            case MenuOption.XNOR:
            case MenuOption.Output:
                var info1 = EntityService.GetDynamicLGateParamsToRender(
                    adjustedCursorPosition, 
                    ComponentManager.IterLGateComponents().Select(x => x.Entity)
                    );
                
                adjustedCursorPosition = info1.position;
                
                if (info1.placement == Placement.Valid)
                {
                    EntityService.AddLGate(adjustedCursorPosition, GetIoTypeFromMenuOption(ChosenMenuOption), false);
                }
                
                break;
            case MenuOption.Input0:
            case MenuOption.Input1:
                var info2 = EntityService.GetDynamicLGateParamsToRender(
                    adjustedCursorPosition, 
                    ComponentManager.IterLGateComponents().Select(x => x.Entity)
                    );
                
                adjustedCursorPosition = info2.position;

                if (info2.placement == Placement.Valid)
                {
                    EntityService.AddLGate(
                        adjustedCursorPosition, GetIoTypeFromMenuOption(ChosenMenuOption),
                        ChosenMenuOption == MenuOption.Input1
                        );
                }

                break;
            case MenuOption.Wire:
                break;
            case MenuOption.Delete:
                EntityService.RemoveEntity(adjustedCursorPosition);
                break;
            case MenuOption.None:
                Debug.Fail("Critical error while creating entity");
                throw new InvalidProgramException("Critical error while creating entity");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private IoType GetIoTypeFromMenuOption(MenuOption menuOption)
    {
        return menuOption switch
        {
            MenuOption.AND => IoType.AND,
            MenuOption.OR => IoType.OR,
            MenuOption.NOT => IoType.NOT,
            MenuOption.XOR => IoType.XOR,
            MenuOption.NAND => IoType.NAND,
            MenuOption.NOR => IoType.NOR,
            MenuOption.XNOR => IoType.XNOR,
            MenuOption.Input0 => IoType.Input,
            MenuOption.Input1 => IoType.Input,
            MenuOption.Output => IoType.Output,
            _ => throw new ArgumentOutOfRangeException(nameof(menuOption), menuOption, null)
        };
    }
    

    #endregion

    #region MouseHeld

    public void HandleMouseHeldAction(Vector2Int relativeCursorPosition)
    {
        if (MouseRightButtonState)
        {
            HandleMouseRightHeldAction(relativeCursorPosition);
        }
        else if (MouseLeftButtonState)
        {
            HandleMouseLeftHeldAction();
        }
    }

    private void HandleMouseRightHeldAction(Vector2Int relativeCursorPosition)
    {
        Debug.Assert(MouseRightButtonState);

        _rendererConfig.ShiftCamera(relativeCursorPosition);
    }

    private void HandleMouseLeftHeldAction()
    {
        SDL.SDL_GetMouseState(out var x, out var y);
        
        if (!IdStructure.IsValid(LGateToMove.Id))
        {
            return;
        }

        var info = EntityService.GetDynamicLGateParamsToRender(
            _rendererConfig.GetRelativeShiftedCursor(new Vector2Int(x, y)),
            ComponentManager.IterLGateComponents().Select(x => x.Entity).Where(z => z.Id != LGateToMove.Id)
        );

        if (info.placement == Placement.Valid)
        {
            EntityService.UpdateEntityPosition(LGateToMove, info.position);
        }
    }

    #endregion

    #region MouseWheel

    public void HandleMouseWheel(Vector2Int cursor, int wheelY)
    {
        _rendererConfig.ChangeRelativelyToCursorZoom((float)(wheelY * 0.1), cursor);
    }

    #endregion
}