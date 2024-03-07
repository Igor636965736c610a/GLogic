using System.Diagnostics;
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
        var userActionHandler = new UserActionsHandler(appRenderer);
        var window = SDL.SDL_CreateWindow(
            "GLogic",
            SDL.SDL_WINDOWPOS_CENTERED,
            SDL.SDL_WINDOWPOS_CENTERED,
            appRenderer.WindowSize.Size.X,
            appRenderer.WindowSize.Size.Y,
            SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);
        var sdlRenderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
        
        const int targetFrameRate = 60;
        const long targetFrameTime = 1000 / targetFrameRate;

        var stopwatch = new Stopwatch();
        var quit = false;
        while (!quit)
        {
            while (SDL.SDL_PollEvent(out var e) != 0)
            {
                if (e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONUP)
                {
                    userActionHandler.HandleMouseUpPollEvent(new Vector2Int(e.button.x, e.button.y), e.button.button);
                }
                if (e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN)
                {
                    userActionHandler.HandleMouseDownPollEvent(new Vector2Int(e.button.x, e.button.y), e.button.button);
                }
                if (e.type == SDL.SDL_EventType.SDL_KEYDOWN)
                {
                    userActionHandler.HandleKeyDownEvent(e.key.keysym.sym);
                }
                if (e.type == SDL.SDL_EventType.SDL_KEYUP)
                {
                    userActionHandler.HandleKeyUpEvent(e.key.keysym.sym);
                }
                if (e.type == SDL.SDL_EventType.SDL_QUIT)
                {
                    quit = true;
                }
                if (e.type == SDL.SDL_EventType.SDL_MOUSEWHEEL)
                {
                    SDL.SDL_GetMouseState(out int cursorX, out int cursorY);
                    var cursor = new Vector2Int(cursorX, cursorY);
                    userActionHandler.HandleMouseWheel(cursor, e.wheel.y, appRenderer);
                }
            }
            
            SDL.SDL_GetRelativeMouseState(out int relativeX, out int relativeY);
            if (UserActionsHandler.MouseRightButtonState)
            {
                userActionHandler.HandleMouseRightHeldAction(new Vector2Int(relativeX, relativeY), appRenderer);
            }
            
            SDL.SDL_SetRenderDrawColor(sdlRenderer, 1, 1, 1, 255);
            SDL.SDL_RenderClear(sdlRenderer);
            appRenderer.RenderEntities(sdlRenderer);
            
            Menu.Draw(sdlRenderer);
            
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            var sleepTime = (int)(targetFrameTime - elapsedMilliseconds);

            if (sleepTime > 0)
            {
                Thread.Sleep(sleepTime);
            }
            else
            {
                Console.WriteLine("Performance issue");
            }
        }

        SDL.SDL_DestroyRenderer(sdlRenderer);
        SDL.SDL_DestroyWindow(window);
        SDL.SDL_Quit();
    }
}