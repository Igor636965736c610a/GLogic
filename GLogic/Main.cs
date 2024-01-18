using SDL2;

namespace GLogic;

internal class Program
{
    public static void Main(string[] args)
    {
        SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING);
        
        var window = SDL.SDL_CreateWindow(
            "SDL Image Example", 
            SDL.SDL_WINDOWPOS_CENTERED,
            SDL.SDL_WINDOWPOS_CENTERED,
            1280,
            720,
            SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);
        var renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);

        DrawLeftMenu(renderer);

        // var destinationRect2 = new SDL.SDL_Rect { x = 200, y = 200, w = -200, h = -200 };

        var quit = false;
        while (!quit)
        {
            while (SDL.SDL_PollEvent(out var e) != 0)
            {
                
                if (e.type == SDL.SDL_EventType.SDL_QUIT)
                {
                    quit = true;
                }
            }
        }

        // Zwolnij zasoby
        SDL.SDL_DestroyRenderer(renderer);
        SDL.SDL_DestroyWindow(window);
        SDL.SDL_Quit();
    }

    private static void DrawLeftMenu(IntPtr renderer)
    {
        SDL.SDL_SetRenderDrawColor(renderer, 50, 50, 50, 255);
        var menuRect = new SDL.SDL_Rect { x = 0, y = 0, w = 150, h = 720 };
        SDL.SDL_RenderFillRect(renderer, ref menuRect);
        var options = MenuLayout.MenuOptions;
        for (var i = 0; i < options.Length; i++)
        {
            var current = options[i];
            var option = new SDL.SDL_Rect
            {
                x = current.Position.X,
                y = current.Position.Y,
                w = current.Size.X,
                h = current.Size.Y
            };
            //SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
            //SDL.SDL_RenderFillRect(renderer, ref option);
            var textColor = new SDL.SDL_Color { r = 255, g = 255, b = 255, a = 255 };
            var font = SDL_ttf.TTF_OpenFont("Oswald-Regular.ttf", 18);
            var text = (CurrentMenuOption)i;
            var surface = SDL_ttf.TTF_RenderText_Solid(font, text.ToStringFast(), new SDL.SDL_Color
            {
                r = 0,
                g = 0,
                b = 255,
                a = 255
            });
            var texture = SDL.SDL_CreateTextureFromSurface(renderer, surface);
            SDL.SDL_RenderCopy(renderer, texture, (nint)null, ref option);
        }
        SDL.SDL_RenderPresent(renderer);
    }
}