using System.Collections.Immutable;
using GLogic.Jobs.Renderer;
using GLogicECS.Components;
using NetEscapades.EnumGenerators;
using SDL2;

namespace GLogic.Jobs;

public sealed class TextureStorage
{
    private readonly ImmutableArray<IntPtr> _lGateTextures;
    private readonly ImmutableArray<IntPtr> _menuTextures;

    public TextureStorage(IntPtr renderer)
    {
        _lGateTextures = InitLGateTextures(renderer).ToImmutableArray();
        _menuTextures = InitMenuTextures(renderer).ToImmutableArray();
    }

    public IntPtr GetLGateTexture(LGate lGate, bool state, Placement placement)
        => _lGateTextures[(int)GetLGateTextureIndex(lGate, state, placement)];

    public IntPtr GetMenuOptionTexture(MenuOptionT menuOption, bool isChecked)
        => _menuTextures[(int)GetMenuOptionTextureIndex(menuOption, isChecked)];

    public LGate ConvertToLGate(IoType ioType, bool state) => ioType switch
    {
        IoType.AND => LGate.AND,
        IoType.OR => LGate.OR,
        IoType.NOT => LGate.NOT,
        IoType.XOR => LGate.XOR,
        IoType.NAND => LGate.NAND,
        IoType.NOR => LGate.NOR,
        IoType.XNOR => LGate.XNOR,
        IoType.Constant => state ? LGate.HighConstant : LGate.LowConstant,
        IoType.LedOutput => LGate.LedOutput,
        IoType.Wire => throw new InvalidOperationException("Accessing a non-existent texture"),
        _ => throw new ArgumentOutOfRangeException(nameof(ioType), ioType, null)
    };

    public LGate ConvertToLGate(MenuOption menuOption) => menuOption switch
    {
        MenuOption.AND => LGate.AND,
        MenuOption.OR => LGate.OR,
        MenuOption.NOT => LGate.NOT,
        MenuOption.XOR => LGate.XOR,
        MenuOption.NAND => LGate.NAND,
        MenuOption.NOR => LGate.NOR,
        MenuOption.XNOR => LGate.XNOR,
        MenuOption.LowConstant => LGate.LowConstant,
        MenuOption.HighConstant => LGate.HighConstant,
        MenuOption.LedOutput => LGate.LedOutput,
        MenuOption.Wire => throw new InvalidOperationException("Accessing a non-existent texture"),
        MenuOption.Delete => throw new InvalidOperationException("Accessing a non-existent texture"),
        MenuOption.None => throw new InvalidOperationException("Accessing a non-existent texture"),
        _ => throw new ArgumentOutOfRangeException(nameof(menuOption), menuOption, null)
    };

    private LGateTexture GetLGateTextureIndex(LGate g, bool state, Placement placement)
        => (LGateTexture)(
            (int)g
            | (int)(state ? LGateTexture.StateOn : LGateTexture.StateOff)
            | (int)placement
        );

    private MenuOptionTexture GetMenuOptionTextureIndex(MenuOptionT mo, bool isChecked)
        => (MenuOptionTexture)(
            (int)mo
            | (int)(isChecked ? IsChecked.Yes : IsChecked.No)
        );

    private IntPtr[] InitLGateTextures(IntPtr renderer)
    {
        var font = SDL_ttf.TTF_OpenFont("Oswald-Light.ttf", 100);
        var initArray = new IntPtr[0b0111_1111];

        for (var i = 0; i < Enum.GetNames(typeof(LGate)).Length; i++)
        {
            CreateLGateTextures(renderer, font, initArray, (LGate)i, true);

            CreateLGateTextures(renderer, font, initArray, (LGate)i, false);
        }

        SDL_ttf.TTF_CloseFont(font);

        return initArray;
    }

    private void CreateLGateTextures(IntPtr renderer, IntPtr font, IntPtr[] initArray, LGate gate, bool state)
    {
        var textColor = state
            ? new SDL.SDL_Color { r = 255, g = 0, b = 0, a = 255 }
            : new SDL.SDL_Color { r = 0, g = 0, b = 0, a = 255 };
        var bgColorInvalid = new SDL.SDL_Color { r = 255, g = 0, b = 0, a = 215 };
        var bgColorValid = new SDL.SDL_Color { r = 42, g = 214, b = 56, a = 215 };
        var bgColorNeutral = new SDL.SDL_Color { r = 26, g = 29, b = 49, a = 215 };

        AddLGateTexture(renderer, font, initArray, gate, state, Placement.Invalid, textColor, bgColorInvalid);

        AddLGateTexture(renderer, font, initArray, gate, state, Placement.Valid, textColor, bgColorValid);

        AddLGateTexture(renderer, font, initArray, gate, state, Placement.Neutral, textColor, bgColorNeutral);
    }

    private void AddLGateTexture(IntPtr renderer, IntPtr font, IntPtr[] initArray, LGate gate, bool state,
        Placement placement, SDL.SDL_Color textColor, SDL.SDL_Color bgColor)
    {
        var surface = SDL_ttf.TTF_RenderText_Shaded(font, gate.ToStringFast(), textColor, bgColor);
        var texture = SDL.SDL_CreateTextureFromSurface(renderer, surface);
        var index = (int)GetLGateTextureIndex(gate, state, placement);
        initArray[index] = texture;
        SDL.SDL_FreeSurface(surface);
    }

    private IntPtr[] InitMenuTextures(IntPtr renderer)
    {
        var font = SDL_ttf.TTF_OpenFont("Oswald-Light.ttf", 100);
        var initArray = new IntPtr[0b11_1111];

        var chosenOptionTextColor = new SDL.SDL_Color { r = 255, g = 0, b = 0, a = 255 };
        var standardOptionTextColor = new SDL.SDL_Color { r = 0, g = 0, b = 0, a = 255 };
        for (var i = 0; i < Enum.GetNames(typeof(MenuOptionT)).Length; i++)
        {
            var bgColor = i % 2 == 0
                ? new SDL.SDL_Color { r = 75, g = 75, b = 75, a = 255 }
                : new SDL.SDL_Color { r = 100, g = 100, b = 100, a = 255 };

            AddMenuOptionTexture(renderer, font, initArray, (MenuOptionT)i, true, chosenOptionTextColor, bgColor);

            AddMenuOptionTexture(renderer, font, initArray, (MenuOptionT)i, false, standardOptionTextColor, bgColor);
        }

        SDL_ttf.TTF_CloseFont(font);

        return initArray;
    }

    private void AddMenuOptionTexture(IntPtr renderer, IntPtr font, IntPtr[] initArray, MenuOptionT menuOption,
        bool isChecked, SDL.SDL_Color textColor, SDL.SDL_Color bgColor)
    {
        var surface = SDL_ttf.TTF_RenderText_Shaded(font, menuOption.ToStringFast(), textColor, bgColor);
        var texture = SDL.SDL_CreateTextureFromSurface(renderer, surface);
        var index = (int)GetMenuOptionTextureIndex(menuOption, isChecked);
        initArray[index] = texture;
        SDL.SDL_FreeSurface(surface);
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

[Flags]
public enum LGateTexture
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
public enum Placement
{
    Neutral = LGateTexture.PlacementNeutral,
    Valid = LGateTexture.PlacementValid,
    Invalid = LGateTexture.PlacementInvalid
}

[Flags]
[EnumExtensions]
public enum LGate
{
    AND = LGateTexture.AND,
    OR = LGateTexture.OR,
    NOT = LGateTexture.NOT,
    XOR = LGateTexture.XOR,
    NAND = LGateTexture.NAND,
    NOR = LGateTexture.NOR,
    XNOR = LGateTexture.XNOR,
    LowConstant = LGateTexture.LowConstant,
    HighConstant = LGateTexture.HighConstant,
    LedOutput = LGateTexture.LedOutput
}

[Flags]
public enum MenuOptionTexture
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
public enum MenuOptionT
{
    AND = MenuOptionTexture.AND,
    OR = MenuOptionTexture.OR,
    NOT = MenuOptionTexture.NOT,
    XOR = MenuOptionTexture.XOR,
    NAND = MenuOptionTexture.NAND,
    NOR = MenuOptionTexture.NOR,
    XNOR = MenuOptionTexture.XNOR,
    LowConstant = MenuOptionTexture.LowConstant,
    HighConstant = MenuOptionTexture.HighConstant,
    LedOutput = MenuOptionTexture.LedOutput,
    Wire = MenuOptionTexture.Wire,
    Delete = MenuOptionTexture.Delete,
    None = MenuOptionTexture.None
}

[Flags]
public enum IsChecked
{
    Yes = MenuOptionTexture.Checked,
    No = MenuOptionTexture.UnChecked
}