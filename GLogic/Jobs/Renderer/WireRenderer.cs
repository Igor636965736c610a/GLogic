namespace GLogic.Jobs.Renderer;

public sealed class WireRenderer
{
    private readonly IRendererStateAccess _rendererStateAccess;

    public WireRenderer(IRendererStateAccess rendererStateAccess)
    {
        _rendererStateAccess = rendererStateAccess;
    }
}