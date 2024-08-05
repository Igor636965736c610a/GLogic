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

    public TopPanelOption? GetClickedOption(Vector2Int cursor)
    {
        foreach (var (optionRect, option) in Options)
        {
            if (cursor.X >= optionRect.Position.X
                && cursor.X <= optionRect.Position.X + optionRect.Size.X
                && cursor.Y >= optionRect.Position.Y
                && cursor.Y < optionRect.Position.Y + optionRect.Size.Y)
            {
                return option;
            }
        }

        return null;
    }
    
    private IEnumerable<PanelOption<TopPanelOption>> InitTopPanelOptions()
    {
        return new List<PanelOption<TopPanelOption>>()
        {
            new(
                new Area(
                    new Vector2Int(
                        AppSettings.WindowSize.X - 350,
                        20),
                    new Vector2Int(
                        100,
                        60)
                ),
                TopPanelOption.Reset),
            new(
                new Area(
                    new Vector2Int(
                        AppSettings.WindowSize.X - 600,
                        20),
                    new Vector2Int(
                        200,
                        60)
                ),
                TopPanelOption.InstantSim),
            new(
                new Area(
                    new Vector2Int(
                        AppSettings.WindowSize.X - 850,
                        20),
                    new Vector2Int(
                        200,
                        60)
                    ),
                TopPanelOption.StepwiseSim)
        };
    }
}

public enum TopPanelOption
{
    Reset,
    InstantSim,
    StepwiseSim,
}