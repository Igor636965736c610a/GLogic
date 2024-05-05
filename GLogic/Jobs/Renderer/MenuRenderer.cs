using GLogic.Components.Common;
using NetEscapades.EnumGenerators;
using SDL2;

namespace GLogic.Jobs.Renderer;

public sealed class MenuRenderer
{
    public const int Width = 150;
    public const int Height = 720;
    private static readonly SDL.SDL_Color MenuColor = new() { r = 50, g = 50, b = 50, a = 255 };
    private readonly IntPtr _renderer;

    private readonly TextureStorage _textureStorage;

    static MenuRenderer()
    {
        MenuOptions = GetMenuOptions().ToList();
    }

    public MenuRenderer(IntPtr renderer, TextureStorage textureStorage)
    {
        _textureStorage = textureStorage;
        _renderer = renderer;
    }

    public static IEnumerable<MenuCheckRect> MenuOptions { get; }

    public void Render() //TODO
    {
        var menuRect = new SDL.SDL_Rect { x = 0, y = 0, w = Width, h = Height };
        SDL.SDL_SetRenderDrawColor(_renderer, MenuColor.r, MenuColor.g, MenuColor.b, MenuColor.a);
        SDL.SDL_RenderFillRect(_renderer, ref menuRect);

        var i = 0;
        foreach (var menuOption in MenuOptions)
        {
            var rect = new SDL.SDL_Rect
            {
                x = menuOption.Position.X,
                y = menuOption.Position.Y,
                w = menuOption.Size.X,
                h = menuOption.Size.Y
            };

            var option = (MenuOption)i;
            var texture =
                _textureStorage.GetMenuOptionTexture((MenuOptionT)option,
                    UserActionsHandler.ChosenMenuOption == option);

            SDL.SDL_RenderCopy(_renderer, texture, (nint)null, ref rect);

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
}

public readonly record struct MenuCheckRect(Vector2Int Position, Vector2Int Size);

[EnumExtensions]
public enum MenuOption
{
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
    None
}