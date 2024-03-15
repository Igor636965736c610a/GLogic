using GLogic.Components.Common;
using NetEscapades.EnumGenerators;
using SDL2;

namespace GLogic.Jobs;

public class Menu
{
    public Menu(TextureStorage textureStorage)
    {
        _textureStorage = textureStorage;
    }

    private static readonly SDL.SDL_Color[] RectsColorLayout;
    private readonly TextureStorage _textureStorage;
    private static readonly SDL.SDL_Color MenuColor = new() { r = 50, g = 50, b = 50, a = 255 };

    public const int Width = 150;
    public const int Height = 720;
    public static IEnumerable<MenuCheckRect> MenuOptions { get; }

    public void Render(IntPtr renderer)
    {
        var menuRect = new SDL.SDL_Rect { x = 0, y = 0, w = Width, h = Height };
        SDL.SDL_SetRenderDrawColor(renderer, MenuColor.r, MenuColor.g, MenuColor.b, MenuColor.a);
        SDL.SDL_RenderFillRect(renderer, ref menuRect);
        
        var i = 0;
        foreach (var menuOption in MenuOptions)
        {
            var rect = new SDL.SDL_Rect
            {
                x = menuOption.Position.X,
                y = menuOption.Position.Y,
                w = menuOption.Size.X,
                h = menuOption.Size.Y,
            };
            var rectColor = RectsColorLayout[i];
            SDL.SDL_SetRenderDrawColor(renderer, rectColor.r, rectColor.g, rectColor.b, rectColor.a);
            SDL.SDL_RenderFillRect(renderer, ref rect);
            
            var option = (MenuOption)i;
            var texture = _textureStorage.GetMenuRectTexture(option, UserActionsHandler.ChosenMenuOption == option);
            
            SDL.SDL_RenderCopy(renderer, texture, (nint)null, ref rect);

            i++;
        }
    }

    private static IEnumerable<MenuCheckRect> GetMenuOptions()
    {
        for (var i = 0; i <= Enum.GetNames(typeof(MenuOption)).Length - 2; i++)
        {
            var y = 30 + 55 * i;
            yield return new MenuCheckRect(new Vector2Int(10, y), new Vector2Int(130, 55));
        }
    }

    private static IEnumerable<SDL.SDL_Color> InitRectsColorLayout()
    {
        var firstRectColor = new SDL.SDL_Color { r = 75, g = 75, b = 75, a = 255 };
        var secondRectColor = new SDL.SDL_Color { r = 100, g = 100, b = 100, a = 128 };
        
        for (int i = 0; i < MenuOptions.Count(); i++)
        {
            var rectColor = i % 2 == 0
                ? firstRectColor
                : secondRectColor;

            yield return rectColor;
        }
    }

    static Menu()
    {
        MenuOptions = GetMenuOptions().ToList();
        RectsColorLayout = InitRectsColorLayout().ToArray();
    }
}

public readonly record struct MenuCheckRect(Vector2Int Position, Vector2Int Size);

[EnumExtensions]
public enum MenuOption
{
    //textured
    AND,
    OR,
    NOT,
    XOR,
    NAND,
    NOR,
    XNOR,
    Input0,
    Input1,
    Output,
    Wire,
    Delete,
    //textured
    
    None
}