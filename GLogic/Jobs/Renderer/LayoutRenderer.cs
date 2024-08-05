using GLogic.Data;
using GLogic.Data.Panels;
using GLogic.Data.State;
using GLogic.Data.TextureStorage;
using SDL2;

namespace GLogic.Jobs.Renderer;

public sealed class LayoutRenderer
{
    private static readonly SDL.SDL_Color PanelColor = new() { r = 50, g = 50, b = 50, a = 255 };
    private readonly IntPtr _renderer;
    private readonly LayoutArrangement _layoutArrangement;
    private readonly LeftPanelTextureStorage _leftPanelTextureStorage;
    private readonly TopPanelTextureStorage _topPanelTextureStorage;

    public LayoutRenderer(IntPtr renderer, LayoutArrangement layoutArrangement, LeftPanelTextureStorage leftPanelTextureStorage, TopPanelTextureStorage topPanelTextureStorage)
    {
        _leftPanelTextureStorage = leftPanelTextureStorage;
        _topPanelTextureStorage = topPanelTextureStorage;
        _layoutArrangement = layoutArrangement;
        _renderer = renderer;
    }

    public void Render()
    { 
        RenderLeftPanel();
        RenderTopPanel();
    }

    private void RenderLeftPanel()
    {
        SDL.SDL_Rect leftPanelRect = _layoutArrangement.LeftPanel.Rect;

        SDL.SDL_SetRenderDrawColor(_renderer, PanelColor.r, PanelColor.g, PanelColor.b, PanelColor.a);
        SDL.SDL_RenderFillRect(_renderer, ref leftPanelRect);

        foreach (var panelOpt in _layoutArrangement.LeftPanel.Options)
        {
            SDL.SDL_Rect rect = panelOpt.Rect;

            var texture =
                _leftPanelTextureStorage.GetLeftPanelOptionTexture((LeftPanelOptF)panelOpt.Option,
                    MenuState.ChosenLGate == panelOpt.Option);

            SDL.SDL_RenderCopy(_renderer, texture, (nint)null, ref rect);
        }
    }

    private void RenderTopPanel()
    {
        SDL.SDL_Rect topPanelRect = _layoutArrangement.TopPanel.Rect;
        
        SDL.SDL_SetRenderDrawColor(_renderer, PanelColor.r, PanelColor.g, PanelColor.b, PanelColor.a);
        SDL.SDL_RenderFillRect(_renderer, ref topPanelRect);
        
        foreach (var panelOpt in _layoutArrangement.TopPanel.Options)
        {
            SDL.SDL_Rect rect = panelOpt.Rect;

            var texture =
                _topPanelTextureStorage.GetTopPanelOptionTexture((TopPanelOptnF)panelOpt.Option,
                    false);

            SDL.SDL_RenderCopy(_renderer, texture, (nint)null, ref rect);
        }
    }
}