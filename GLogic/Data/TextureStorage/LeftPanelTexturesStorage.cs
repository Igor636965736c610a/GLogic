using System.Collections.Immutable;
using NetEscapades.EnumGenerators;
using SDL2;

namespace GLogic.Data.TextureStorage;

public sealed class LeftPanelTextureStorage
{
    private readonly ImmutableArray<IntPtr> _leftPanelOptionTextures;

    public LeftPanelTextureStorage(IntPtr renderer)
    {
        _leftPanelOptionTextures = InitLeftPanelOptionTextures(renderer).ToImmutableArray();
    }

    public IntPtr GetLeftPanelOptionTexture(LeftPanelOptF leftPanelOpt, bool isChecked)
        => _leftPanelOptionTextures[(int)GetLeftPanelOptionTextureIndex(leftPanelOpt, isChecked)];
    
    private IntPtr[] InitLeftPanelOptionTextures(IntPtr renderer)
    {
        var font = SDL_ttf.TTF_OpenFont("Oswald-Light.ttf", 100);
        var initArray = new IntPtr[0b11_1111];

        var chosenOptionTextColor = new SDL.SDL_Color { r = 255, g = 0, b = 0, a = 255 };
        var standardOptionTextColor = new SDL.SDL_Color { r = 0, g = 0, b = 0, a = 255 };
        for (var i = 0; i < Enum.GetNames(typeof(LeftPanelOptF)).Length; i++)
        {
            var bgColor = i % 2 == 0
                ? new SDL.SDL_Color { r = 75, g = 75, b = 75, a = 255 }
                : new SDL.SDL_Color { r = 100, g = 100, b = 100, a = 255 };

            AddLeftPanelOptionTexture(renderer, font, initArray, (LeftPanelOptF)i, true, chosenOptionTextColor, bgColor);

            AddLeftPanelOptionTexture(renderer, font, initArray, (LeftPanelOptF)i, false, standardOptionTextColor, bgColor);
        }

        SDL_ttf.TTF_CloseFont(font);

        return initArray;
    }

    private void AddLeftPanelOptionTexture(IntPtr renderer, IntPtr font, IntPtr[] initArray, LeftPanelOptF leftPanelOpt,
        bool isChecked, SDL.SDL_Color textColor, SDL.SDL_Color bgColor)
    {
        var surface = SDL_ttf.TTF_RenderText_Shaded(font, leftPanelOpt.ToStringFast(), textColor, bgColor);
        var texture = SDL.SDL_CreateTextureFromSurface(renderer, surface);
        var index = (int)GetLeftPanelOptionTextureIndex(leftPanelOpt, isChecked);
        initArray[index] = texture;
        SDL.SDL_FreeSurface(surface);
    }
    
    private LeftPanelOptTextureF GetLeftPanelOptionTextureIndex(LeftPanelOptF mo, bool isChecked)
        => (LeftPanelOptTextureF)(
            (int)mo
            | (int)(isChecked ? LeftPanelOptCheckStatusF.Yes : LeftPanelOptCheckStatusF.No)
        );
    
    ~LeftPanelTextureStorage()
    {
        foreach (var texture in _leftPanelOptionTextures)
        {
            SDL.SDL_DestroyTexture(texture);
        }
    }
}
[Flags]
public enum LeftPanelOptTextureF
{
    // option bits 0-4
    AND = 0b00_0000,
    OR = 0b00_0001,
    NOT = 0b00_0010,
    XOR = 0b00_0011,
    NAND = 0b00_0100,
    NOR = 0b00_0101,
    XNOR = 0b00_0110,
    LowConstant = 0b00_0111,
    HighConstant = 0b00_1000,
    LedOutput = 0b00_1001,
    Wire = 0b00_1010,
    Delete = 0b00_1011,
    None = 0b00_1111,

    // state bits 6
    UnChecked = 0b00_0000,
    Checked = 0b10_0000
}

[Flags]
[EnumExtensions]
public enum LeftPanelOptF
{
    AND = LeftPanelOptTextureF.AND,
    OR = LeftPanelOptTextureF.OR,
    NOT = LeftPanelOptTextureF.NOT,
    XOR = LeftPanelOptTextureF.XOR,
    NAND = LeftPanelOptTextureF.NAND,
    NOR = LeftPanelOptTextureF.NOR,
    XNOR = LeftPanelOptTextureF.XNOR,
    LowConstant = LeftPanelOptTextureF.LowConstant,
    HighConstant = LeftPanelOptTextureF.HighConstant,
    LedOutput = LeftPanelOptTextureF.LedOutput,
    Wire = LeftPanelOptTextureF.Wire,
    Delete = LeftPanelOptTextureF.Delete,
    None = LeftPanelOptTextureF.None
}

[Flags]
public enum LeftPanelOptCheckStatusF
{
    Yes = LeftPanelOptTextureF.Checked,
    No = LeftPanelOptTextureF.UnChecked
}