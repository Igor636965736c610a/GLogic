using GLogicGlobal.Common;
using SDL2;

namespace GLogic.Data.State;

public static class InputState
{
    public static Vector2Int CursorPosition { get; private set; }
    public static Vector2Int RelativeCursorPosition { get; private set; }
    public static bool MouseRightButtonState { get; set; }
    public static bool MouseLeftButtonState { get; set; }
    public static bool ShiftKeyState { get; set; }

    public static void UpdateCursorPosition()
    {
        SDL.SDL_GetMouseState(out int x, out int y);
        SDL.SDL_GetRelativeMouseState(out int relativeX, out int relativeY);
        CursorPosition = new Vector2Int(x, y);
        RelativeCursorPosition = new Vector2Int(relativeX, relativeY);
    }
}