using GLogic.Components.Common;
using SDL2;

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

    public void RenderStaticWire(Vector2Int p1, Vector2Int p2, bool value)
    {
        SDL.SDL_SetRenderDrawColor(_renderer, 255, 0, 0, 0);
        SDL.SDL_RenderDrawLine(_renderer, p1.X, p1.Y, p2.X, p2.Y);
    }
}