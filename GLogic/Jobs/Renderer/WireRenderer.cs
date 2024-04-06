namespace GLogic.Jobs.Renderer;

public sealed class WireRenderer
{
    private readonly IRendererStateAccess _rendererStateAccess;
    private readonly IntPtr _renderer;
    private readonly TextureStorage _textureStorage;

    public WireRenderer(IRendererStateAccess rendererStateAccess, IntPtr renderer, TextureStorage textureStorage)
    {
        _rendererStateAccess = rendererStateAccess;
        _renderer = renderer;
        _textureStorage = textureStorage;
    }
}