using GLogic.Jobs.Renderer;
using GLogicGlobal.Common;

namespace GLogic.Data.Panels;

public sealed class TopPanel
{
    public TopPanel()
    {
        Rect = new Area(new Vector2Int(150, 0), new Vector2Int(AppSettings.WindowSize.X - 150, 100));
        ResetButton = new Area(
            new Vector2Int(
                AppSettings.WindowSize.X - 250,
                20),
            new Vector2Int(
                AppSettings.WindowSize.X - 200,
                60)
        );
    }
    
    public Area Rect { get; }
    public Area ResetButton { get; }
}