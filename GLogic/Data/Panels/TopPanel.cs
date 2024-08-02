using GLogic.Jobs.Renderer;
using GLogicGlobal.Common;

namespace GLogic.Data.Panels;

public sealed class TopPanel
{
    public TopPanel()
    {
        Rect = new Area(new Vector2Int(150, 0), new Vector2Int(AppSettings.WindowSize.X - 150, 100));
        Options = InitTopPanelOptions();
    }
    
    public IEnumerable<PanelOption<TopPanelOption>> Options { get; init; }
    public Area Rect { get; }

    private IEnumerable<PanelOption<TopPanelOption>> InitTopPanelOptions()
    {
        return new List<PanelOption<TopPanelOption>>()
        {
            new(
                new Area(
                    new Vector2Int(
                        AppSettings.WindowSize.X - 250,
                        20),
                    new Vector2Int(
                        AppSettings.WindowSize.X - 200,
                        60)
                ),
                TopPanelOption.ResetButton),
            new(
                new Area(
                    new Vector2Int(
                        AppSettings.WindowSize.X - 400,
                        20),
                    new Vector2Int(
                        AppSettings.WindowSize.X - 300,
                        60)
                ),
                TopPanelOption.ToInstantSimButton),
            new(
                new Area(
                    new Vector2Int(
                        AppSettings.WindowSize.X - 500,
                        20),
                    new Vector2Int(
                        AppSettings.WindowSize.X - 400,
                        60)
                    ),
                TopPanelOption.ToStepwiseSimButton)
        };
    }
}

public enum TopPanelOption
{
    ResetButton,
    ToInstantSimButton,
    ToStepwiseSimButton,
}