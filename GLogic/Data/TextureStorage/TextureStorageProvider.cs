using System.Collections.Immutable;
using GLogic.Data.Panels;
using GLogicECS.Components;
using SDL2;

namespace GLogic.Data.TextureStorage;

public sealed class TextureStorageProvider
{
    public readonly LGateTextureStorage LGateTextureStorage;
    public readonly LeftPanelTextureStorage LeftPanelTextureStorage;

    public TextureStorageProvider(IntPtr renderer)
    {
        LGateTextureStorage = new LGateTextureStorage(renderer);
        LeftPanelTextureStorage = new LeftPanelTextureStorage(renderer);
    }
}