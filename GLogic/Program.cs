using System.Diagnostics;
using System.Runtime.InteropServices;
using GLogic.Jobs;
using GLogic.Jobs.AppUpdaters;
using GLogic.Jobs.Internal;
using GLogic.Jobs.Internal.EcsStateModifiers.LogicCircuitUpdates.Simulations;
using GLogic.Jobs.Renderer;
using GLogicECS.Components;
using GLogicGlobal.Common;
using SDL2;

// const int test = args[0] is not null ? Int32.Parse(args[0]) : 60;

SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING);
SDL_ttf.TTF_Init();

const string appName = "GLogic";
var window = SDL.SDL_CreateWindow(
    appName,
    SDL.SDL_WINDOWPOS_CENTERED,
    SDL.SDL_WINDOWPOS_CENTERED,
    RendererApi.WindowSize.Size.X,
    RendererApi.WindowSize.Size.Y,
    SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN
);

// ReSharper disable once InconsistentNaming
const int index_of_the_rendering_driver_to_initialize = -1; // -1 to initialize the first one supporting the requested flags

var sdlRenderer = SDL.SDL_CreateRenderer(window, index_of_the_rendering_driver_to_initialize, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
var textures = new TextureStorage(sdlRenderer);
var appRenderer = new RendererApi(sdlRenderer, textures);
var circuitUpdater = new CircuitUpdater().InitDefault(out IUserActionExecutor userActionExecutor);
var userActionHandler = new UserActionsHandler(appRenderer, userActionExecutor);

const string SDL2GfxLib = "libSDL2_gfx-1-0-0.dll";

// test
[DllImport(SDL2GfxLib, CallingConvention = CallingConvention.Cdecl)]
static extern int aalineRGBA(IntPtr renderer, int x1, int y1, int x2, int y2, byte r, byte g, byte b, byte a);
// ~test

const int fps = 60;
const int desiredDelta = 1000 / fps;

// var stopW = new Stopwatch();
// for (int i = 0; i < 50; i++)
// {
//     stopW.Start();
//     for (int j = 0; j < 505; j++)
//     {
//         var entity = EntityService.AddLGate(new Vector2Int(i * (EntityService.RectLGateSize.X + 5), j * (EntityService.RectLGateSize.Y + 5)), IoType.NOR, true);
//         // if (i < 10)
//         // {
//         //     continue;
//         // }
//
//         //var comp = ComponentManager.GetTransformComponent(entity).Position;
//         //var position = new Vector2Int(comp.X + 5, comp.Y + 5);
//         //EntityService.RemoveEntity(position);
//     }
//     // Console.WriteLine(stopW.Elapsed.Seconds);
//     float x = stopW.Elapsed.Milliseconds / 1000f;
//     Console.WriteLine($"{(i * 1000):#,0} all entities - {x:F2} milliseconds to add 1000 entities");
//     stopW.Reset();
// }
// stopW.Stop();

var frameCount = 0;
var startTime = SDL.SDL_GetTicks();
var lastTime = startTime;
var i = 0;

var quit = false;
while (!quit)
{
    var startLoop = SDL.SDL_GetTicks();

    while (SDL.SDL_PollEvent(out var e) != 0)
    {
        switch (e.type)
        {
            case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                userActionHandler.HandleMouseUpPollEvent(new Vector2Int(e.button.x, e.button.y), e.button.button);

                break;
            case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                userActionHandler.HandleMouseDownPollEvent(new Vector2Int(e.button.x, e.button.y), e.button.button);

                break;
            case SDL.SDL_EventType.SDL_KEYDOWN:
                userActionHandler.HandleKeyDownEvent(e.key.keysym.sym);

                break;
            case SDL.SDL_EventType.SDL_KEYUP:
                userActionHandler.HandleKeyUpEvent(e.key.keysym.sym);

                break;
            case SDL.SDL_EventType.SDL_QUIT:
                quit = true;

                break;
            case SDL.SDL_EventType.SDL_MOUSEWHEEL:
            {
                SDL.SDL_GetMouseState(out var cursorX, out var cursorY);
                var cursor = new Vector2Int(cursorX, cursorY);
                userActionHandler.HandleMouseWheel(cursor, e.wheel.y);

                break;
            }
        }
    }

    SDL.SDL_GetRelativeMouseState(out var relativeX, out var relativeY);
    userActionHandler.HandleMouseHeldAction(new Vector2Int(relativeX, relativeY));

    //SANDBOX CODE
    if (i == 60)
    {
        i = 0;
        circuitUpdater.CurrentUpdateCtx.Update();
    }
    else
    {
        i++;
    }
    //~SANDBOX CODE

    SDL.SDL_SetRenderDrawColor(sdlRenderer, 1, 1, 1, 255);
    SDL.SDL_RenderClear(sdlRenderer);

    appRenderer.RenderEntities();
    appRenderer.RenderMenu();

    SDL.SDL_RenderPresent(sdlRenderer);

    //SANDBOX CODE
    var currentTime = SDL.SDL_GetTicks();
    frameCount++;
    if (currentTime - lastTime >= 1000)
    {
        var fpss = frameCount / ((currentTime - lastTime) / 1000.0);
        Console.WriteLine($"FPS: {fpss}");

        frameCount = 0;
        lastTime = currentTime;
    }

    var delta = SDL.SDL_GetTicks() - startLoop;
    if (delta < desiredDelta)
    {
        SDL.SDL_Delay(desiredDelta - delta);
        //Console.WriteLine($"{delta} ---- {desiredDelta}");
    }
    else
    {
        Console.WriteLine($"Performance issue : {delta} ---- {desiredDelta}");
    }
    //~SANDBOX CODE
}

SDL.SDL_DestroyRenderer(sdlRenderer);
SDL.SDL_DestroyWindow(window);
SDL.SDL_Quit();