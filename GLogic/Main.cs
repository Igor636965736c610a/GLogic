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
            800, 
            600, 
            SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE | SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);
        var renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);

        var image = SDL_image.IMG_Load("obrazek.png");
        if (image == IntPtr.Zero)
        {
            Console.WriteLine("Error while loading image: {0}", SDL.SDL_GetError());
            return;
        }

        var texture = SDL.SDL_CreateTextureFromSurface(renderer, image);
        SDL.SDL_FreeSurface(image);

        if (texture == IntPtr.Zero)
        {
            Console.WriteLine("Error while creating texture: {0}", SDL.SDL_GetError());
            return;
        }

        var destinationRect = new SDL.SDL_Rect { x = 100, y = 100, w = 100, h = 100 };
        
        // var destinationRect2 = new SDL.SDL_Rect { x = 200, y = 200, w = -200, h = -200 };

        
        SDL.SDL_RenderClear(renderer);

        SDL.SDL_RenderCopy(renderer, texture, IntPtr.Zero, ref destinationRect);
        //SDL.SDL_RenderCopy(renderer, texture, IntPtr.Zero, ref destinationRect2);

        SDL.SDL_RenderPresent(renderer);

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
        SDL.SDL_DestroyTexture(texture);
        SDL.SDL_DestroyRenderer(renderer);
        SDL.SDL_DestroyWindow(window);
        SDL.SDL_Quit();
    }
}