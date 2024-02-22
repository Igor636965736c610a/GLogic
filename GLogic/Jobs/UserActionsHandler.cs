using System.Diagnostics;
using GLogic.Components;
using GLogic.Components.Common;
using GLogic.Components.System;
using SDL2;

namespace GLogic.Jobs;

public static class UserActionsHandler
{
    public static LGate ChosenLGate { get; private set; }
    public static Entity LGateToMove { get; private set; }
    public static bool MouseRightButtonState { get; private set; }

    static UserActionsHandler()
    {
        ChosenLGate = LGate.None;
        MouseRightButtonState = false;
    }
    
    #region Click
    
    public static void HandleMouseUpPollEvent(Vector2Int cursor, uint mouseButton)
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
    public static void HandleMouseDownPollEvent(Vector2Int cursor, uint mouseButton)
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
    
    private static void LeftClickDown(Vector2Int cursor)
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
    private static void MarkSomeEntity(Vector2Int cursor)
    {
        var markedEntity = new Entity { Id = IdStructure.MakeInvalidId() };
        foreach (var transformComponent in EntityManager.IterTransformComponents())
        {
            if (!EntityManager.IsAlive(transformComponent.Entity))
            {
                continue;
            }
            var positionX = transformComponent.Position.X * Renderer.Zoom; //cursor instead of this
            var positionY = transformComponent.Position.Y * Renderer.Zoom; //cursor instead of this
            var sizeX = transformComponent.Size.X * Renderer.Zoom;
            var sizeY = transformComponent.Size.Y * Renderer.Zoom;
            if (cursor.X > positionX
                && cursor.X < positionX + sizeX
                && cursor.Y > positionY
                && cursor.Y < positionY + sizeY)
            {
                markedEntity = transformComponent.Entity;
                Debug.Assert(EntityManager.IsAlive(transformComponent.Entity), "Marking entity witch is not alive");
                break;
            }
        }

        LGateToMove = markedEntity;
        //Console.WriteLine(LGateToMove.Id);
        
        // var transformComponents = EntityManager.IterTransformComponents();
        // LGateToMove = transformComponents.FirstOrDefault(x => 
        //     EntityManager.IsAlive(x.Entity)
        //     && cursorX > x.Position.X 
        //     && cursorX < x.Position.X + x.Size.X
        //     && cursorY > x.Position.Y
        //     && cursorY < x.Position.Y + x.Size.Y).Entity;
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
    private static void UserActions(Vector2Int cursor)
    {
        switch (ChosenLGate)
        {
            case LGate.AND:
            {
                EntityService.AddLGate(cursor, IoType.AND, false);
                break;
            }
            case LGate.OR:
            {
                EntityService.AddLGate(cursor, IoType.OR, false);
                break;
            }
            case LGate.NOT:
            {
                EntityService.AddLGate(cursor, IoType.NOT, false);
                break;
            }
            case LGate.XOR:
            {
                EntityService.AddLGate(cursor, IoType.XOR, false);
                break;
            }
            case LGate.NAND:
            {
                EntityService.AddLGate(cursor, IoType.NAND, false);
                break;
            }
            case LGate.NOR:
            {
                EntityService.AddLGate(cursor, IoType.NOR, false);
                break;
            }
            case LGate.XNOR:
            {
                EntityService.AddLGate(cursor, IoType.XNOR, false);
                break;
            }
            case LGate.Input1:
            {
                EntityService.AddLGate(cursor, IoType.Input, true);
                break;
            };
            case LGate.Input0:
            {
                EntityService.AddLGate(cursor, IoType.Input, false);
                break;
            }
            case LGate.Output:
            {
                EntityService.AddLGate(cursor, IoType.Output, false);
                break;
            }
            case LGate.Wire:
            {
                //test//
               // Renderer._zoom -= 0.1f;
                break;
            }
            case LGate.Delete:
            {
                //test//
                //Renderer._zoom += 0.1f;
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

    public static void HandleMouseWheel(Vector2Int cursor, int wheelY, IRendererConfig rendererConfig)
    {
        rendererConfig.ChangeRelativelyToCursorZoom((float)(wheelY * 0.1), cursor);
    }

    #endregion

    #region MouseHeld

    public static void HandleMouseRightHeldAction(Vector2Int relativeCursorPosition, IRendererConfig rendererConfig)
    {
        Debug.Assert(MouseRightButtonState);
        rendererConfig.ShiftCamera(relativeCursorPosition);
    }

    #endregion
}
