using GLogic.Jobs.Renderer;
using GLogicGlobal.Common;

namespace GLogic.Data;

public static class AppSettings
{
    public static readonly Vector2Int WindowSize;
    public const int Fps = 60;

    static AppSettings()
    {
        WindowSize = new Vector2Int(1280, 720);
    }
}