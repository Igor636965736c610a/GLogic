using GLogic.Jobs.Renderer;
using SDL2;

namespace GLogic.Extensions;

public static class NativeExtensions
{
    public static SDL.SDL_Rect FromArea(this SDL.SDL_Rect rect, Area area)
    {
        return new SDL.SDL_Rect
        {
            x = area.Position.X,
            y = area.Position.Y,
            w = area.Size.X,
            h = area.Size.Y,
        };
    }
}