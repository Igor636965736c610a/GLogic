using GLogic.Jobs;
using SDL2;

namespace GLogic;

internal static class Program
{
    public static void Main(string[] args)
    {
        SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING);
        SDL_ttf.TTF_Init();

        var window = SDL.SDL_CreateWindow(
            "GLogic",
            SDL.SDL_WINDOWPOS_CENTERED,
            SDL.SDL_WINDOWPOS_CENTERED,
            1280,
            720,
            SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);
        var renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
        
        var quit = false;
        while (!quit)
        {
            while (SDL.SDL_PollEvent(out var e) != 0)
            {
                var mouseButton = SDL.SDL_GetMouseState(out int cursorX, out int cursorY);

                if (e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN)
                {
                    UserActionsHandler.HandleMousePollEvent(cursorX, cursorY, mouseButton);
                }
                
                if (e.type == SDL.SDL_EventType.SDL_QUIT)
                {
                    quit = true;
                }
            }
            
            Menu.Draw(renderer);
        }

        SDL.SDL_DestroyRenderer(renderer);
        SDL.SDL_DestroyWindow(window);
        SDL.SDL_Quit();
    }
}