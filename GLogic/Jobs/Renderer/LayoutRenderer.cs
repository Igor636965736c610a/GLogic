using GLogic.Data;
using GLogic.Data.Panels;
using GLogic.Data.TextureStorage;
using SDL2;

namespace GLogic.Jobs.Renderer;

public sealed class LayoutRenderer
{
    private static readonly SDL.SDL_Color MenuColor = new() { r = 50, g = 50, b = 50, a = 255 };
    private readonly IntPtr _renderer;
    private readonly LayoutArrangement _layoutArrangement;
    private readonly LeftPanelTextureStorage _leftPanelTextureStorage;

    public LayoutRenderer(IntPtr renderer, LayoutArrangement layoutArrangement, LeftPanelTextureStorage leftPanelTextureStorage)
    {
        _leftPanelTextureStorage = leftPanelTextureStorage;
        _layoutArrangement = layoutArrangement;
        _renderer = renderer;
    }

    public void Render()
    { 
        RenderLeftPanel();
    }

    private void RenderLeftPanel()
    {
        SDL.SDL_Rect leftPanelRect = _layoutArrangement.LeftPanel.Rect;

        SDL.SDL_SetRenderDrawColor(_renderer, MenuColor.r, MenuColor.g, MenuColor.b, MenuColor.a);
        SDL.SDL_RenderFillRect(_renderer, ref leftPanelRect);

        foreach (var menuOption in _layoutArrangement.LeftPanel.Options)
        {
            SDL.SDL_Rect rect = menuOption.Rect;

            var texture =
                _leftPanelTextureStorage.GetLeftPanelOptionTexture((LeftPanelOptF)menuOption.Option,
                    UserActionsHandler.ChosenLeftPanelOption == menuOption.Option);

            SDL.SDL_RenderCopy(_renderer, texture, (nint)null, ref rect);
        }
    }

    private void RenderTopPanel()
    {
        SDL.SDL_Rect topPanelRect = _layoutArrangement.TopPanel.Rect;
        
        SDL.SDL_SetRenderDrawColor(_renderer, MenuColor.r, MenuColor.g, MenuColor.b, MenuColor.a);
        SDL.SDL_RenderFillRect(_renderer, ref topPanelRect);
        
        
    }
}