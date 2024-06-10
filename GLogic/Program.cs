using System.Diagnostics;
using GLogic.Jobs;
using GLogic.Jobs.AppUpdaters;
using GLogic.Jobs.Internal;
using GLogic.Jobs.Internal.EcsStateModifiers;
using GLogic.Jobs.Renderer;
using GLogicECS.Components;
using GLogicGlobal.Common;
using SDL2;

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
var userActionHandler = new UserActionsHandler(appRenderer, circuitUpdater, userActionExecutor);

const int fps = 60;
const int desiredDelta = 1000 / fps;



// List<Entity> XX = new List<Entity>(1000*500);
// var stopW = new Stopwatch();
// for (int i = 0; i < 1000; i++)
// {
//     stopW.Start();
//     for (int j = 0; j < 500; j++)
//     {
//         var entity = EntityService.AddLGate(new Vector2Int(i * (EntityService.RectLGateSize.X + 5), j * (EntityService.RectLGateSize.Y + 5)), IoType.NOR, true);
//         // if (i < 10)
//         // {
//         //     continue;
//         // }
//         XX.Add(entity);
//         //var comp = ComponentManager.GetTransformComponent(entity).Position;
//         //var position = new Vector2Int(comp.X + 5, comp.Y + 5);
//         //EntityService.RemoveEntity(position);
//     }
//     // Console.WriteLine(stopW.Elapsed.Seconds);
//     float x = stopW.Elapsed.Milliseconds / 1000f;
//     Console.WriteLine($"{(i * 1000):#,0} all entities - {x:F2} milliseconds to add 1000 entities");
// }
// Console.WriteLine(stopW.ElapsedMilliseconds);
// stopW.Stop();



var frameCount = 0;
var startTime = SDL.SDL_GetTicks();
var lastTime = startTime;
uint time = 0;

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
                SDL.SDL_GetMouseState(out var cursorX, out var cursorY);
                
                var cursor = new Vector2Int(cursorX, cursorY);
                userActionHandler.HandleMouseWheel(cursor, e.wheel.y);

                break;
        }
    }

    SDL.SDL_GetRelativeMouseState(out var relativeX, out var relativeY);
    userActionHandler.HandleMouseHeldAction(new Vector2Int(relativeX, relativeY));
    
    await circuitUpdater.CurrentUpdateCtx.Update(time);

    SDL.SDL_SetRenderDrawColor(sdlRenderer, 1, 1, 1, 255);
    SDL.SDL_RenderClear(sdlRenderer);

    appRenderer.RenderEntities();
    appRenderer.RenderMenu();

    SDL.SDL_RenderPresent(sdlRenderer);

    var currentTime = SDL.SDL_GetTicks();
    var delta = currentTime - startLoop;
    if (delta < desiredDelta)
    {
        SDL.SDL_Delay(desiredDelta - delta);
    }
    var endLoop = SDL.SDL_GetTicks();
    time = endLoop - startLoop; 
    
    // FPS COUNTER
    frameCount++;
    if (currentTime - lastTime >= 1000)
    {
        Console.WriteLine($"FPS: {frameCount / ((currentTime - lastTime) / 1000.0)}");

        frameCount = 0;
        lastTime = currentTime;
    }
}

SDL.SDL_DestroyRenderer(sdlRenderer);
SDL.SDL_DestroyWindow(window);
SDL.SDL_Quit();