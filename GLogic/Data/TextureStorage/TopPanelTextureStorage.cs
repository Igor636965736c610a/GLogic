using System.Collections.Immutable;
using NetEscapades.EnumGenerators;
using SDL2;

namespace GLogic.Data.TextureStorage;

public sealed class TopPanelTextureStorage
{
    private readonly ImmutableArray<IntPtr> _topPanelOptionTextures;

    public TopPanelTextureStorage(IntPtr renderer)
    {
        _topPanelOptionTextures = InitTopPanelOptionTextures(renderer).ToImmutableArray();
    }
    
    public IntPtr GetTopPanelOptionTexture(TopPanelOptnF opt, bool isChecked)
        => _topPanelOptionTextures[(int)GetTopPanelOptionTextureIndex(opt, isChecked)];

    private IntPtr[] InitTopPanelOptionTextures(IntPtr renderer)
    {
        var font = SDL_ttf.TTF_OpenFont("Oswald-Light.ttf", 100);
        var initArray = new IntPtr[0b11_1111];

        var chosenOptionTextColor = new SDL.SDL_Color { r = 255, g = 0, b = 0, a = 255 };
        var standardOptionTextColor = new SDL.SDL_Color { r = 0, g = 0, b = 0, a = 255 };
        for (var i = 0; i < Enum.GetNames(typeof(TopPanelOptnF)).Length; i++)
        {
            var bgColor = i % 2 == 0
                ? new SDL.SDL_Color { r = 75, g = 75, b = 75, a = 255 }
                : new SDL.SDL_Color { r = 100, g = 100, b = 100, a = 255 };

            AddTopPanelOptionTexture(renderer, font, initArray, (TopPanelOptnF)i, true, chosenOptionTextColor, bgColor);
            AddTopPanelOptionTexture(renderer, font, initArray, (TopPanelOptnF)i, false, standardOptionTextColor, bgColor);
        }

        SDL_ttf.TTF_CloseFont(font);

        return initArray;
    }

    private void AddTopPanelOptionTexture(IntPtr renderer, IntPtr font, IntPtr[] initArray, TopPanelOptnF topPanelOpt,
        bool isChecked, SDL.SDL_Color textColor, SDL.SDL_Color bgColor)
    {
        var surface = SDL_ttf.TTF_RenderText_Shaded(font, topPanelOpt.ToStringFast(), textColor, bgColor);
        var texture = SDL.SDL_CreateTextureFromSurface(renderer, surface);
        var index = (int)GetTopPanelOptionTextureIndex(topPanelOpt, isChecked);
        initArray[index] = texture;
        SDL.SDL_FreeSurface(surface);
    }
    
    private TopPanelOptTextureF GetTopPanelOptionTextureIndex(TopPanelOptnF opt, bool isChecked)
        => (TopPanelOptTextureF)(
            (int)opt
            | (int)(isChecked ? TopPanelOptCheckStatusF.Checked : TopPanelOptCheckStatusF.UnChecked)
        );
    
    ~TopPanelTextureStorage()
    {
        foreach (var texture in _topPanelOptionTextures)
        {
            SDL.SDL_DestroyTexture(texture);
        }
    }
}

[Flags]
public enum TopPanelOptTextureF
{
    // option bits 0-4
    Reset = 0b00_0000,
    ToInstantSim = 0b00_0001,
    ToStepwiseSim = 0b00_0010,

    // state bit 6
    UnChecked = 0b00_0000,
    Checked = 0b10_0000
}

[Flags]
[EnumExtensions]
public enum TopPanelOptnF
{
    Reset = TopPanelOptTextureF.Reset,
    ToInstantSim = TopPanelOptTextureF.ToInstantSim,
    ToStepwiseSim = TopPanelOptTextureF.ToStepwiseSim,
}

[Flags]
public enum TopPanelOptCheckStatusF
{
    UnChecked = TopPanelOptTextureF.UnChecked,
    Checked = TopPanelOptTextureF.Checked,
}