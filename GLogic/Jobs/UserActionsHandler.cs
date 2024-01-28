using GLogic.Components;
using SDL2;

namespace GLogic.Jobs;

public static class UserActionsHandler
{
    public static LGate ChosenLGate { get; private set; }
    public static Entity? MovedLGate { get; private set; } 
    
    static UserActionsHandler()
    {
        ChosenLGate = LGate.None;
    }
    
    #region Click
    
    public static void HandleMouseUpPollEvent(int cursorX, int cursorY, uint mouseButton)
    {
        if (mouseButton == SDL.SDL_BUTTON_LEFT)
        {
            LeftClickUp(cursorX, cursorY);
        }
    }
    public static void HandleMouseDownPollEvent(int cursorX, int cursorY, uint mouseButton)
    {
        if (mouseButton == SDL.SDL_BUTTON_LEFT)
        {
            LeftClickDown(cursorX, cursorY);
        }
    }
    public static void SetMovedLGateToNone()
        => MovedLGate = null;

    private static void LeftClickDown(int cursorX, int cursorY)
    {
        if (cursorX <= Menu.Width)
        {
            SetChosenLGate(cursorX, cursorY);
        }
    }
    private static void LeftClickUp(int cursorX, int cursorY)
    {
        if (ChosenLGate != LGate.None || cursorX <= Menu.Width)
            return;
        
        var transformComponents = EntityManager.IterTransformComponents();
        MovedLGate = transformComponents.FirstOrDefault(x => 
            EntityManager.IsAlive(x.Entity)
            && cursorX > x.Position.X 
            && cursorX < x.Position.X + x.Size.X
            && cursorY > x.Position.Y
            && cursorY < x.Position.Y + x.Size.Y).Entity;
    }
    private static void SetChosenLGate(int cursorX, int cursorY)
    {
        var i = 0;
        foreach (var option in Menu.MenuOptions())
        {
            if (cursorX >= option.Position.X && cursorX <= option.Position.X + option.Size.X
                                             && cursorY >= option.Position.Y 
                                             && cursorY < option.Position.Y + option.Size.Y)
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
    
    #endregion

    #region Held

    

    #endregion
}