namespace GLogic.Jobs;

public static class UserActionsHandler
{
    public static LGate ChosenLGate { get; private set; }
    
    static UserActionsHandler()
    {
        ChosenLGate = LGate.None;
    }

    public static void HandleMousePollEvent(int cursorX, int cursorY)
    {
        if (cursorX <= Menu.Width)
        {
            SetChosenLGate(cursorX, cursorY);
        }
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
}