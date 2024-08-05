namespace GLogic.Data.TextureStorage;

public sealed class TextureStorageProvider
{
    public readonly LGateTextureStorage LGateTextureStorage;
    public readonly LeftPanelTextureStorage LeftPanelTextureStorage;
    public readonly TopPanelTextureStorage TopPanelTextureStorage;

    public TextureStorageProvider(IntPtr renderer)
    {
        TopPanelTextureStorage = new TopPanelTextureStorage(renderer);
        LGateTextureStorage = new LGateTextureStorage(renderer);
        LeftPanelTextureStorage = new LeftPanelTextureStorage(renderer);
    }
}