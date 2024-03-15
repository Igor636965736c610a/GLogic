using System.Collections.Immutable;
using GLogic.Components;
using SDL2;

namespace GLogic.Jobs;

public sealed class TextureStorage
{
    private static readonly int LGatesCountInIoTypeEnum;
    private static readonly int MenuOptionsCount;
    private readonly ImmutableArray<IntPtr> _lGateTextures;
    private readonly ImmutableArray<IntPtr> _menuTextures;
    
    public TextureStorage(IntPtr renderer)
    {
        _lGateTextures = InitLGateTextures(renderer).ToImmutableArray();
        _menuTextures = InitMenuTextures(renderer).ToImmutableArray();
    }

    static TextureStorage()
    {
        LGatesCountInIoTypeEnum = Enum.GetNames(typeof(MenuOption)).Length - 2;
        MenuOptionsCount = Enum.GetNames(typeof(MenuOption)).Length - 1;
    }

    public IntPtr GetLGateTexture(MenuOption lGate)
    {
        if ((int)lGate > LGatesCountInIoTypeEnum)
        {
            throw new InvalidProgramException("ioType is not valid");
        }

        return _lGateTextures[(int)lGate];
    }

    public IntPtr GetMenuRectTexture(MenuOption menuOption, bool isChecked)
    {
        if ((int)menuOption > MenuOptionsCount)
        {
            throw new InvalidProgramException("menuOption is not valid");
        }

        return isChecked ? _menuTextures[(int)menuOption + MenuOptionsCount] : _menuTextures[(int)menuOption];
    }

    private IEnumerable<IntPtr> InitLGateTextures(IntPtr renderer)
    {
        var font = SDL_ttf.TTF_OpenFont("Oswald-Light.ttf", 100);
        var textColor = new SDL.SDL_Color { r = 255, g = 255, b = 255, a = 0 };
        for (int i = 0; i < LGatesCountInIoTypeEnum; i++)
        {
            var surface = SDL_ttf.TTF_RenderText_Solid(font, ((MenuOption)i).ToStringFast(), textColor);
            var texture = SDL.SDL_CreateTextureFromSurface(renderer, surface);
            
            yield return texture;
            
            SDL.SDL_FreeSurface(surface);
        }
        
        SDL_ttf.TTF_CloseFont(font);
    }

    private IEnumerable<IntPtr> InitMenuTextures(IntPtr renderer)
    {
        var font = SDL_ttf.TTF_OpenFont("Oswald-Light.ttf", 100);
        var standardTextColor = new SDL.SDL_Color { r = 255, g = 255, b = 255, a = 255 };
        var chosenLGateTextColor = new SDL.SDL_Color { r = 198, g = 65, b = 36, a = 255 };
        for (int i = 0; i < MenuOptionsCount; i++)
        {
            var option = (MenuOption)i;
            IntPtr surface = SDL_ttf.TTF_RenderText_Solid(font, option.ToStringFast(), standardTextColor);
            var texture = SDL.SDL_CreateTextureFromSurface(renderer, surface);
            SDL.SDL_FreeSurface(surface);
            yield return texture;
        }

        for (int i = MenuOptionsCount; i < MenuOptionsCount * 2; i++)
        {
            var option = (MenuOption)i - MenuOptionsCount;
            var surface = SDL_ttf.TTF_RenderText_Solid(font, option.ToStringFast(), chosenLGateTextColor); 
            var texture = SDL.SDL_CreateTextureFromSurface(renderer, surface);
            SDL.SDL_FreeSurface(surface);
            yield return texture;
        }
        
        SDL_ttf.TTF_CloseFont(font);
    }

    ~TextureStorage()
    {
        foreach (var texture in _lGateTextures)
        {
            SDL.SDL_DestroyTexture(texture);
        }

        foreach (var texture in _menuTextures)
        {
            SDL.SDL_DestroyTexture(texture);
        }
    }
}