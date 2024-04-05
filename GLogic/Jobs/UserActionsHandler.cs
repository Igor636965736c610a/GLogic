using System.Diagnostics;
using GLogic.Components;
using GLogic.Components.Common;
using GLogic.Components.System;
using GLogic.Jobs.Renderer;
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
        var adjustedCursorPosition = _rendererConfig.GetRelativeShiftedCursor(cursor);

        var markedEntity = EntityQuery.AABB_Entities(
            ArchetypeManager.IterLGateComponents().Select(x => x.Entity),
            adjustedCursorPosition
        ).FirstOrDefault(new Entity { Id = IdStructure.MakeInvalidId() });

        LGateToMove = markedEntity;
        Console.WriteLine(LGateToMove.Id);
    }

    private static void SetChosenLGate(Vector2Int cursor)
    {
        var i = 0;
        foreach (var option in Menu.MenuOptions)
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
            {
                adjustedCursorPosition = EntityService.CenterRectPositionToCursor(adjustedCursorPosition);

                if (ShiftKeyState)
                {
                    var newAdjustedCursorPosition = AdjustDisplayLGatePosition(
                        adjustedCursorPosition,
                        EntityService.GetLGateOverlapArea(adjustedCursorPosition),
                        ArchetypeManager.IterLGateComponents().Select(x => x.Entity)
                    );

                    if (!newAdjustedCursorPosition.canPut)
                    {
                        return;
                    }

                    adjustedCursorPosition = newAdjustedCursorPosition.position;
                }

                EntityService.AddLGate(adjustedCursorPosition, IoType.AND, false);

                break;
            }
            case MenuOption.OR:
            {
                adjustedCursorPosition = EntityService.CenterRectPositionToCursor(adjustedCursorPosition);

                if (ShiftKeyState)
                {
                    var newAdjustedCursorPosition = AdjustDisplayLGatePosition(
                        adjustedCursorPosition,
                        EntityService.GetLGateOverlapArea(adjustedCursorPosition),
                        ArchetypeManager.IterLGateComponents().Select(x => x.Entity)
                    );

                    if (!newAdjustedCursorPosition.canPut)
                    {
                        return;
                    }

                    adjustedCursorPosition = newAdjustedCursorPosition.position;
                }

                EntityService.AddLGate(adjustedCursorPosition, IoType.OR, false);

                break;
            }
            case MenuOption.NOT:
            {
                adjustedCursorPosition = EntityService.CenterRectPositionToCursor(adjustedCursorPosition);

                if (ShiftKeyState)
                {
                    var newAdjustedCursorPosition = AdjustDisplayLGatePosition(
                        adjustedCursorPosition,
                        EntityService.GetLGateOverlapArea(adjustedCursorPosition),
                        ArchetypeManager.IterLGateComponents().Select(x => x.Entity)
                    );

                    if (!newAdjustedCursorPosition.canPut)
                    {
                        return;
                    }

                    adjustedCursorPosition = newAdjustedCursorPosition.position;
                }

                EntityService.AddLGate(adjustedCursorPosition, IoType.NOT, false);

                break;
            }
            case MenuOption.XOR:
            {
                adjustedCursorPosition = EntityService.CenterRectPositionToCursor(adjustedCursorPosition);

                if (ShiftKeyState)
                {
                    var newAdjustedCursorPosition = AdjustDisplayLGatePosition(
                        adjustedCursorPosition,
                        EntityService.GetLGateOverlapArea(adjustedCursorPosition),
                        ArchetypeManager.IterLGateComponents().Select(x => x.Entity)
                    );

                    if (!newAdjustedCursorPosition.canPut)
                    {
                        return;
                    }

                    adjustedCursorPosition = newAdjustedCursorPosition.position;
                }

                EntityService.AddLGate(adjustedCursorPosition, IoType.XOR, false);

                break;
            }
            case MenuOption.NAND:
            {
                adjustedCursorPosition = EntityService.CenterRectPositionToCursor(adjustedCursorPosition);

                if (ShiftKeyState)
                {
                    var newAdjustedCursorPosition = AdjustDisplayLGatePosition(
                        adjustedCursorPosition,
                        EntityService.GetLGateOverlapArea(adjustedCursorPosition),
                        ArchetypeManager.IterLGateComponents().Select(x => x.Entity)
                    );

                    if (!newAdjustedCursorPosition.canPut)
                    {
                        return;
                    }

                    adjustedCursorPosition = newAdjustedCursorPosition.position;
                }

                EntityService.AddLGate(adjustedCursorPosition, IoType.NAND, false);

                break;
            }
            case MenuOption.NOR:
            {
                adjustedCursorPosition = EntityService.CenterRectPositionToCursor(adjustedCursorPosition);

                if (ShiftKeyState)
                {
                    var newAdjustedCursorPosition = AdjustDisplayLGatePosition(
                        adjustedCursorPosition,
                        EntityService.GetLGateOverlapArea(adjustedCursorPosition),
                        ArchetypeManager.IterLGateComponents().Select(x => x.Entity)
                    );

                    if (!newAdjustedCursorPosition.canPut)
                    {
                        return;
                    }

                    adjustedCursorPosition = newAdjustedCursorPosition.position;
                }

                EntityService.AddLGate(adjustedCursorPosition, IoType.NOR, false);

                break;
            }
            case MenuOption.XNOR:
            {
                adjustedCursorPosition = EntityService.CenterRectPositionToCursor(adjustedCursorPosition);

                if (ShiftKeyState)
                {
                    var newAdjustedCursorPosition = AdjustDisplayLGatePosition(
                        adjustedCursorPosition,
                        EntityService.GetLGateOverlapArea(adjustedCursorPosition),
                        ArchetypeManager.IterLGateComponents().Select(x => x.Entity)
                    );

                    if (!newAdjustedCursorPosition.canPut)
                    {
                        return;
                    }

                    adjustedCursorPosition = newAdjustedCursorPosition.position;
                }

                EntityService.AddLGate(adjustedCursorPosition, IoType.XNOR, false);

                break;
            }
            case MenuOption.Input1:
            {
                adjustedCursorPosition = EntityService.CenterRectPositionToCursor(adjustedCursorPosition);

                if (ShiftKeyState)
                {
                    var newAdjustedCursorPosition = AdjustDisplayLGatePosition(
                        adjustedCursorPosition,
                        EntityService.GetLGateOverlapArea(adjustedCursorPosition),
                        ArchetypeManager.IterLGateComponents().Select(x => x.Entity)
                    );

                    if (!newAdjustedCursorPosition.canPut)
                    {
                        return;
                    }

                    adjustedCursorPosition = newAdjustedCursorPosition.position;
                }

                EntityService.AddLGate(adjustedCursorPosition, IoType.Input, true);

                break;
            }
            case MenuOption.Input0:
            {
                adjustedCursorPosition = EntityService.CenterRectPositionToCursor(adjustedCursorPosition);

                if (ShiftKeyState)
                {
                    var newAdjustedCursorPosition = AdjustDisplayLGatePosition(
                        adjustedCursorPosition,
                        EntityService.GetLGateOverlapArea(adjustedCursorPosition),
                        ArchetypeManager.IterLGateComponents().Select(x => x.Entity)
                    );

                    if (!newAdjustedCursorPosition.canPut)
                    {
                        return;
                    }

                    adjustedCursorPosition = newAdjustedCursorPosition.position;
                }

                EntityService.AddLGate(adjustedCursorPosition, IoType.Input, false);

                break;
            }
            case MenuOption.Output:
            {
                adjustedCursorPosition = EntityService.CenterRectPositionToCursor(adjustedCursorPosition);
                if (ShiftKeyState)
                {
                    var newAdjustedCursorPosition = AdjustDisplayLGatePosition(
                        adjustedCursorPosition,
                        EntityService.GetLGateOverlapArea(adjustedCursorPosition),
                        ArchetypeManager.IterLGateComponents().Select(x => x.Entity)
                    );

                    if (!newAdjustedCursorPosition.canPut)
                    {
                        return;
                    }

                    adjustedCursorPosition = newAdjustedCursorPosition.position;
                }

                EntityService.AddLGate(adjustedCursorPosition, IoType.Output, false);

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

    private (bool canPut, Vector2Int position) AdjustDisplayLGatePosition(Vector2Int position, Area overlapArea,
        IEnumerable<Entity> entities)
    {
        var overlap = EntityService.GetEntityWithBiggestOverlap(
            out var entityInOverlapArea,
            overlapArea,
            entities
        );

        if (!overlap)
        {
            return (false, position);
        }

        Debug.Assert(entityInOverlapArea.HasValue);

        return (true, EntityService.AdjustEntityPosition(position, entityInOverlapArea.Value));
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
        if (!IdStructure.IsValid(LGateToMove.Id))
        {
            return;
        }

        SDL.SDL_GetMouseState(out var x, out var y);
        var adjustedPosition = EntityService.CenterRectPositionToCursor(
            _rendererConfig.GetRelativeShiftedCursor(new Vector2Int(x, y))
        );

        if (ShiftKeyState)
        {
            var newAdjustedPosition = AdjustDisplayLGatePosition(
                adjustedPosition,
                EntityService.GetLGateOverlapArea(adjustedPosition),
                ArchetypeManager.IterLGateComponents().Select(x => x.Entity).Where(z => z.Id != LGateToMove.Id)
            );

            if (!newAdjustedPosition.canPut)
            {
                return;
            }

            if (!IdStructure.IsValid(
                    EntityService.CheckArea(newAdjustedPosition.position,
                        ArchetypeManager.IterLGateComponents()
                            .Select(x => x.Entity)
                            .Where(z => z.Id != LGateToMove.Id)).Id
                )
               )
            {
                EntityService.UpdateEntityPosition(LGateToMove, newAdjustedPosition.position);
            }
        }
        else
        {
            var newAdjustedPosition = AdjustDisplayLGatePosition(
                adjustedPosition,
                new Area(adjustedPosition, EntityService.RectLGateSize),
                ArchetypeManager.IterLGateComponents().Select(x => x.Entity).Where(z => z.Id != LGateToMove.Id)
            );

            if (!IdStructure.IsValid(
                    EntityService.CheckArea(newAdjustedPosition.position,
                        ArchetypeManager.IterLGateComponents()
                            .Select(x => x.Entity)
                            .Where(z => z.Id != LGateToMove.Id)).Id
                )
               )
            {
                EntityService.UpdateEntityPosition(LGateToMove, newAdjustedPosition.position);
            }
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