using GLogic.Imports;
using SDL2;

namespace GLogic.Jobs.Renderer;

public sealed class BackgroundRenderer
{
    private readonly IntPtr _renderer;
    private readonly IRendererStateAccess _rendererStateAccess;

    public BackgroundRenderer(IRendererStateAccess rendererStateAccess, IntPtr renderer)
    {
        _renderer = renderer;
        _rendererStateAccess = rendererStateAccess;
    }

    public void RenderGrid()
    {
        SDL.SDL_SetRenderDrawColor(_renderer, 39, 39, 39, 0);

        var factor = (int)Math.Round(10 * _rendererStateAccess.Zoom);
        var firstLineX =
            RendererApi.CalculateShiftRelatively(10, _rendererStateAccess.Zoom, _rendererStateAccess.CameraShift.X);
        var firstLineY =
            RendererApi.CalculateShiftRelatively(10, _rendererStateAccess.Zoom, _rendererStateAccess.CameraShift.Y);
        
        var thickLineX = firstLineX / factor;
        for (int i = firstLineX % factor; i < RendererApi.WindowSize.Size.X; i += factor)
        {
            var gridLineColor = new SDL.SDL_Color
            {
                r = 39,
                g = 39,
                b = 39,
                a = thickLineX % 5 == 0 ? (byte)220 : (byte)90
            };
            
            SDL2Gfx.AaLineRgba(_renderer, i, 0, i, RendererApi.WindowSize.Size.Y, gridLineColor);

            thickLineX--;
        }

        var thickLineY = firstLineY / factor;
        for (int i = firstLineY % factor; i < RendererApi.WindowSize.Size.Y; i += factor)
        {
            var gridLineColor = new SDL.SDL_Color
            {
                r = 39,
                g = 39,
                b = 39,
                a = thickLineY % 5 == 0 ? (byte)220 : (byte)90
            };
            
            SDL2Gfx.AaLineRgba(_renderer, 0, i, RendererApi.WindowSize.Size.X, i, gridLineColor);

            thickLineY--;
        }
    }
}