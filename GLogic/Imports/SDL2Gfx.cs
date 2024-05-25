using System.Runtime.InteropServices;
using SDL2;

namespace GLogic.Imports;

public static class SDL2Gfx
{
    const string SDL2GfxLib = "libSDL2_gfx-1-0-0.dll";

    public static int AaLineRgba(IntPtr renderer, int x1, int y1, int x2, int y2, SDL.SDL_Color color)
    {
        return aalineRGBA(renderer, x1, y1, x2, y2, color.r, color.g, color.b, color.a);
    }
    
    [DllImport(SDL2GfxLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern int aalineRGBA(IntPtr renderer, int x1, int y1, int x2, int y2, byte r, byte g, byte b, byte a);
}