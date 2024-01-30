using GLogic.Components.Common;
using NetEscapades.EnumGenerators;
using SDL2;

namespace GLogic.Jobs;

public static class Menu
{
    private static readonly SDL.SDL_Color FirstRectColor = new() { r = 75, g = 75, b = 75, a = 255 };
    private static readonly SDL.SDL_Color SecondRectColor = new() { r = 100, g = 100, b = 100, a = 128 };
    private static readonly SDL.SDL_Color MenuColor = new() { r = 50, g = 50, b = 50, a = 255 };
    private static readonly SDL.SDL_Color TextColor = new() { r = 255, g = 255, b = 255, a = 255 };
    private static readonly SDL.SDL_Color ChosenLGateColor = new() { r = 198, g = 65, b = 36, a = 255 };

    public const int Width = 150;
    public const int High = 720;

    public static void Draw(IntPtr renderer)
    {
        var menuRect = new SDL.SDL_Rect { x = 0, y = 0, w = Width, h = High };
        SDL.SDL_SetRenderDrawColor(renderer, MenuColor.r, MenuColor.g, MenuColor.b, MenuColor.a);
        SDL.SDL_RenderFillRect(renderer, ref menuRect);
        var i = 0;
        foreach (var menuOption in MenuOptions())
        {
            var rectColor = i % 2 == 0
                ? FirstRectColor
                : SecondRectColor;
            var rect = new SDL.SDL_Rect
            {
                x = menuOption.Position.X,
                y = menuOption.Position.Y,
                w = menuOption.Size.X,
                h = menuOption.Size.Y,
            };
            SDL.SDL_SetRenderDrawColor(renderer, rectColor.r, rectColor.g, rectColor.b, rectColor.a);
            SDL.SDL_RenderFillRect(renderer, ref rect);
            var font = SDL_ttf.TTF_OpenFont("Oswald-Light.ttf", 100);
            var option = (LGate)i;
            var textColor = (LGate)i == UserActionsHandler.ChosenLGate ? ChosenLGateColor : TextColor;
            var surface = SDL_ttf.TTF_RenderText_Solid(font, option.ToStringFast(), textColor);
            var texture = SDL.SDL_CreateTextureFromSurface(renderer, surface);
            SDL.SDL_RenderCopy(renderer, texture, (nint)null, ref rect);
            SDL.SDL_FreeSurface(surface);
            SDL.SDL_DestroyTexture(texture);
            i++;
        }
        SDL.SDL_RenderPresent(renderer);
    }
    public static IEnumerable<MenuCheckRect> MenuOptions()
    {
        for (var i = 0; i <= Enum.GetNames(typeof(LGate)).Length - 2; i++)
        {
            var y = 30 + 55 * i;
            yield return new MenuCheckRect(new Vector2Int(10, y), new Vector2Int(130, 55));
        }
    }
}

public record MenuCheckRect(Vector2Int Position, Vector2Int Size);

[EnumExtensions]
public enum LGate
{
    AND,
    OR,
    NOT,
    XOR,
    NAND,
    NOR,
    XNOR,
    Input1,
    Input0,
    Output,
    Wire,
    Delete,
    None,
}