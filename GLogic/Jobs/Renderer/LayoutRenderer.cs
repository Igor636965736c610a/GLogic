using GLogic.Data;
using SDL2;

namespace GLogic.Jobs.Renderer;

public sealed class LayoutRenderer
{
    private static readonly SDL.SDL_Color MenuColor = new() { r = 50, g = 50, b = 50, a = 255 };
    private readonly IntPtr _renderer;
    private readonly LayoutArrangement _layoutArrangement;
    private readonly TextureStorage _textureStorage;
    
    public LayoutRenderer(IntPtr renderer, LayoutArrangement layoutArrangement, TextureStorage textureStorage)
    {
        _textureStorage = textureStorage;
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

        var i = 0;
        foreach (var menuOption in _layoutArrangement.LeftPanel.Options)
        {
            SDL.SDL_Rect rect = menuOption;

            var option = (LeftPanelOption)i;
            var texture =
                _textureStorage.GetLeftPanelOptionTexture((LeftPanelOptionT)option,
                    UserActionsHandler.ChosenLeftPanelOption == option);

            SDL.SDL_RenderCopy(_renderer, texture, (nint)null, ref rect);

            i++;
        }
    }

    private void RenderTopPanel()
    {
        SDL.SDL_Rect topPanelRect = _layoutArrangement.TopPanel.Rect;
        
        SDL.SDL_SetRenderDrawColor(_renderer, MenuColor.r, MenuColor.g, MenuColor.b, MenuColor.a);
        SDL.SDL_RenderFillRect(_renderer, ref topPanelRect);
        
        
    }
}