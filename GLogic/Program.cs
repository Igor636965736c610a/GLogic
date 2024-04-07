using GLogic.Components.Common;
using GLogic.Jobs;
using GLogic.Jobs.Renderer;
using SDL2;

// const int test = args[0] is not null ? Int32.Parse(args[0]) : 60;

SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING);
SDL_ttf.TTF_Init();

var window = SDL.SDL_CreateWindow(
    "GLogic",
    SDL.SDL_WINDOWPOS_CENTERED,
    SDL.SDL_WINDOWPOS_CENTERED,
    RendererApi.WindowSize.Size.X,
    RendererApi.WindowSize.Size.Y,
    SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);

var sdlRenderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
var textures = new TextureStorage(sdlRenderer);
var appRenderer = new RendererApi(sdlRenderer, textures);
var userActionHandler = new UserActionsHandler(appRenderer);

const int fps = 60;
const int desiredDelta = 1000 / fps;

var quit = false;
while (!quit)
{
    var startLoop = SDL.SDL_GetTicks();

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
            SDL.SDL_GetMouseState(out var cursorX, out var cursorY);
            var cursor = new Vector2Int(cursorX, cursorY);
            userActionHandler.HandleMouseWheel(cursor, e.wheel.y);
        }
    }

    SDL.SDL_GetRelativeMouseState(out var relativeX, out var relativeY);
    userActionHandler.HandleMouseHeldAction(new Vector2Int(relativeX, relativeY));

    SDL.SDL_SetRenderDrawColor(sdlRenderer, 1, 1, 1, 255);
    SDL.SDL_RenderClear(sdlRenderer);

    appRenderer.RenderEntities();
    appRenderer.RenderMenu();
    
    SDL.SDL_RenderPresent(sdlRenderer);

    var delta = SDL.SDL_GetTicks() - startLoop;
    if (delta < desiredDelta)
    {
        SDL.SDL_Delay(desiredDelta - delta);
        //Console.WriteLine($"{delta} ---- {desiredDelta}");
    }
    else
    {
        Console.WriteLine("Performance issue");
    }
}

SDL.SDL_DestroyRenderer(sdlRenderer);
SDL.SDL_DestroyWindow(window);
SDL.SDL_Quit();