using System.Collections.Immutable;
using GLogic.Data.Panels;
using GLogicECS.Components;
using NetEscapades.EnumGenerators;
using SDL2;

namespace GLogic.Data.TextureStorage;

public sealed class LGateTextureStorage
{
    private readonly ImmutableArray<IntPtr> _lGateTextures;

    public LGateTextureStorage(IntPtr renderer)
    {
        _lGateTextures = InitLGateTextures(renderer).ToImmutableArray();
    }
    
    public IntPtr GetLGateTexture(LGateF lGateF, bool state, LGatePlacementF lGatePlacementF)
        => _lGateTextures[(int)GetLGateTextureIndex(lGateF, state, lGatePlacementF)];

    public LGateF ConvertToLGateF(IoType ioType, bool state) => ioType switch
    {
        IoType.AND => LGateF.AND,
        IoType.OR => LGateF.OR,
        IoType.NOT => LGateF.NOT,
        IoType.XOR => LGateF.XOR,
        IoType.NAND => LGateF.NAND,
        IoType.NOR => LGateF.NOR,
        IoType.XNOR => LGateF.XNOR,
        IoType.Constant => state ? LGateF.HighConstant : LGateF.LowConstant,
        IoType.LedOutput => LGateF.LedOutput,
        IoType.Wire => throw new InvalidOperationException("Accessing a non-existent texture"),
        _ => throw new ArgumentOutOfRangeException(nameof(ioType), ioType, null)
    };

    public LGateF ConvertToLGateF(LeftPanelOption leftPanelOption) => leftPanelOption switch
    {
        LeftPanelOption.AND => LGateF.AND,
        LeftPanelOption.OR => LGateF.OR,
        LeftPanelOption.NOT => LGateF.NOT,
        LeftPanelOption.XOR => LGateF.XOR,
        LeftPanelOption.NAND => LGateF.NAND,
        LeftPanelOption.NOR => LGateF.NOR,
        LeftPanelOption.XNOR => LGateF.XNOR,
        LeftPanelOption.LowConstant => LGateF.LowConstant,
        LeftPanelOption.HighConstant => LGateF.HighConstant,
        LeftPanelOption.LedOutput => LGateF.LedOutput,
        LeftPanelOption.Wire => throw new InvalidOperationException("Accessing a non-existent texture"),
        LeftPanelOption.Delete => throw new InvalidOperationException("Accessing a non-existent texture"),
        LeftPanelOption.None => throw new InvalidOperationException("Accessing a non-existent texture"),
        _ => throw new ArgumentOutOfRangeException(nameof(leftPanelOption), leftPanelOption, null)
    };
    
    private IntPtr[] InitLGateTextures(IntPtr renderer)
    {
        var font = SDL_ttf.TTF_OpenFont("Oswald-Light.ttf", 100);
        var initArray = new IntPtr[0b0111_1111];

        for (var i = 0; i < Enum.GetNames(typeof(LGateF)).Length; i++)
        {
            CreateLGateTextures(renderer, font, initArray, (LGateF)i, true);

            CreateLGateTextures(renderer, font, initArray, (LGateF)i, false);
        }

        SDL_ttf.TTF_CloseFont(font);

        return initArray;
    }

    private void CreateLGateTextures(IntPtr renderer, IntPtr font, IntPtr[] initArray, LGateF gateF, bool state)
    {
        var textColor = state
            ? new SDL.SDL_Color { r = 255, g = 0, b = 0, a = 255 }
            : new SDL.SDL_Color { r = 0, g = 0, b = 0, a = 255 };
        var bgColorInvalid = new SDL.SDL_Color { r = 255, g = 0, b = 0, a = 255 };
        var bgColorValid = new SDL.SDL_Color { r = 42, g = 214, b = 56, a = 255 };
        var bgColorNeutral = new SDL.SDL_Color { r = 255, g = 230, b = 230, a = 255 };

        AddLGateTexture(renderer, font, initArray, gateF, state, LGatePlacementF.Invalid, textColor, bgColorInvalid);

        AddLGateTexture(renderer, font, initArray, gateF, state, LGatePlacementF.Valid, textColor, bgColorValid);

        AddLGateTexture(renderer, font, initArray, gateF, state, LGatePlacementF.Neutral, textColor, bgColorNeutral);
    }

    private void AddLGateTexture(IntPtr renderer, IntPtr font, IntPtr[] initArray, LGateF gateF, bool state,
        LGatePlacementF lGatePlacementF, SDL.SDL_Color textColor, SDL.SDL_Color bgColor)
    {
        var surface = SDL_ttf.TTF_RenderText_Shaded(font, gateF.ToStringFast(), textColor, bgColor);
        var texture = SDL.SDL_CreateTextureFromSurface(renderer, surface);
        var index = (int)GetLGateTextureIndex(gateF, state, lGatePlacementF);
        initArray[index] = texture;
        SDL.SDL_FreeSurface(surface);
    }
    
    private LGateTextureF GetLGateTextureIndex(LGateF g, bool state, LGatePlacementF lGatePlacementF)
        => (LGateTextureF)(
            (int)g
            | (int)(state ? LGateTextureF.StateOn : LGateTextureF.StateOff)
            | (int)lGatePlacementF
        );
    
    ~LGateTextureStorage()
    {
        foreach (var texture in _lGateTextures)
        {
            SDL.SDL_DestroyTexture(texture);
        }
    }
}

[Flags]
public enum LGateTextureF
{
    // gates bits: 0-4
    AND = 0b0000_0000,
    OR = 0b0000_0001,
    NOT = 0b0000_0010,
    XOR = 0b0000_0011,
    NAND = 0b0000_0100,
    NOR = 0b0000_0101,
    XNOR = 0b0000_0110,
    LowConstant = 0b0000_0111,
    HighConstant = 0b0000_1000,
    LedOutput = 0b0000_1001,

    // state bits: 5
    StateOff = 0b0000_0000,
    StateOn = 0b0001_0000,

    // placement bits: 6-7
    PlacementNeutral = 0b0000_0000,
    PlacementValid = 0b0010_0000,
    PlacementInvalid = 0b0100_0000
}

[Flags]
public enum LGatePlacementF
{
    Neutral = LGateTextureF.PlacementNeutral,
    Valid = LGateTextureF.PlacementValid,
    Invalid = LGateTextureF.PlacementInvalid
}

[Flags]
[EnumExtensions]
public enum LGateF
{
    AND = LGateTextureF.AND,
    OR = LGateTextureF.OR,
    NOT = LGateTextureF.NOT,
    XOR = LGateTextureF.XOR,
    NAND = LGateTextureF.NAND,
    NOR = LGateTextureF.NOR,
    XNOR = LGateTextureF.XNOR,
    LowConstant = LGateTextureF.LowConstant,
    HighConstant = LGateTextureF.HighConstant,
    LedOutput = LGateTextureF.LedOutput
}