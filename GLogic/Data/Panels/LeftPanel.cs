using GLogic.Jobs.Renderer;
using GLogicGlobal.Common;

namespace GLogic.Data.Panels;

public sealed class LeftPanel
{
    public LeftPanel()
    {
        Rect = new Area(new Vector2Int(0, 0), new Vector2Int(150, AppSettings.WindowSize.Y));
        Options = GetLeftPanelOptions().ToList();
    }
    
    public Area Rect { get; }
    public IEnumerable<Area> Options { get; }

    private static IEnumerable<Area> GetLeftPanelOptions()
    {
        for (var i = 0; i < Enum.GetNames(typeof(LeftPanelOption)).Length - 1; i++)
        {
            var y = 30 + 55 * i;
            yield return new Area(new Vector2Int(10, y), new Vector2Int(130, 55));
        }
    }
}