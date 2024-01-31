using System.Diagnostics;
using GLogic.Components;
using GLogic.Components.Common;
using SDL2;

namespace GLogic.Jobs;

public static class UserActionsHandler
{
    public static LGate ChosenLGate { get; private set; }
    public static Entity LGateToMove { get; private set; } 
    
    static UserActionsHandler()
    {
        ChosenLGate = LGate.None;
    }
    
    #region Click
    
    public static void HandleMouseUpPollEvent(Vector2Int cursor, uint mouseButton)
    {
        if (mouseButton == SDL.SDL_BUTTON_LEFT)
        {
            LGateToMove = new Entity { Id = IdStructure.MakeInvalidId() };
        }
    }
    public static void HandleMouseDownPollEvent(Vector2Int cursor, uint mouseButton, IEnumerable<Entity> entitiesInArea)
    {
        if (mouseButton == SDL.SDL_BUTTON_LEFT)
        {
            LeftClickDown(cursor, entitiesInArea);
        }
    }
    
    private static void LeftClickDown(Vector2Int cursor, IEnumerable<Entity> entitiesInArea)
    {
        if (cursor.X <= Menu.Width)
        {
            SetChosenLGate(cursor);
        }
        else if (ChosenLGate == LGate.None)
        {
            MarkSomeEntity(cursor, entitiesInArea);
        }
        else
        {
            UserActions(cursor);
        }
    }
    private static void MarkSomeEntity(Vector2Int cursor, IEnumerable<Entity> entitiesInArea)
    {
        var markedEntity = new Entity { Id = IdStructure.MakeInvalidId() };
        foreach (var entity in entitiesInArea)
        {
            var transformComponent = EntityManager.GetTransformComponent(entity);
            var positionX = transformComponent.Position.X * Renderer.Zoom;
            var positionY = transformComponent.Position.Y * Renderer.Zoom;
            var sizeX = transformComponent.Size.X * Renderer.Zoom;
            var sizeY = transformComponent.Size.Y * Renderer.Zoom;
            if (cursor.X > positionX
                && cursor.X < positionX + sizeX
                && cursor.Y > positionY
                && cursor.Y < positionY + sizeY)
            {
                markedEntity = transformComponent.Entity;
                Debug.Assert(EntityManager.IsAlive(entity), "Marking entity witch is not alive");
                break;
            }
        }

        LGateToMove = markedEntity;
        
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
        foreach (var option in Menu.MenuOptions())
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
                break;
            case LGate.Delete:
                break;
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

    

    #endregion
}
