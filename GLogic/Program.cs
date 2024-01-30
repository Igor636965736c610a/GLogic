using System.Diagnostics;
using System.Numerics;
using GLogic.Components.Common;
using GLogic.Jobs;
using SDL2;

namespace GLogic;

internal static class Program
{
    public static void Main(string[] args)
    {
        SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING);
        SDL_ttf.TTF_Init();

        var appRenderer = new Renderer();
        var window = SDL.SDL_CreateWindow(
            "GLogic",
            SDL.SDL_WINDOWPOS_CENTERED,
            SDL.SDL_WINDOWPOS_CENTERED,
            appRenderer.WindowSize.Width,
            appRenderer.WindowSize.High,
            SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);
        var renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
        
        var quit = false;
        while (!quit)
        {
            appRenderer.UpdateEntitiesToRender();
            while (SDL.SDL_PollEvent(out var e) != 0)
            {
                var mouseButton = SDL.SDL_GetMouseState(out int cursorX, out int cursorY);
                var cursor = new Vector2Int(cursorX, cursorY);
                if (e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONUP)
                {
                    UserActionsHandler.HandleMouseUpPollEvent(cursor, mouseButton);
                }
                if (e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN)
                {
                    UserActionsHandler.HandleMouseDownPollEvent(cursor, mouseButton, appRenderer.EntitiesToRender);
                }
                if (e.type == SDL.SDL_EventType.SDL_QUIT)
                {
                    quit = true;
                }
            }
            
            var relativeMouseState = SDL.SDL_GetRelativeMouseState(out int relativeX, out int relativeY);
            if (relativeMouseState != 0 && SDL.SDL_BUTTON(SDL.SDL_BUTTON_LEFT) != 0)
            {
                Console.WriteLine("Left button held");
            }

            SDL.SDL_SetRenderDrawColor(renderer, 1, 1, 1, 255); //testing
            SDL.SDL_RenderClear(renderer);                              //testing
            appRenderer.RenderEntities(renderer);                       //testing
            
            Menu.Draw(renderer);
        }

        SDL.SDL_DestroyRenderer(renderer);
        SDL.SDL_DestroyWindow(window);
        SDL.SDL_Quit();
    }
}