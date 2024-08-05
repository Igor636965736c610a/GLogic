using GLogic.Data.Panels;

namespace GLogic.Data.State;

public static class MenuState
{
    private static TopPanelOption _chosenSimulation = TopPanelOption.StepwiseSim;
    public static LeftPanelOption ChosenLGate { get; set; } = LeftPanelOption.None;

    public static TopPanelOption ChosenSimulation
    {
        get => _chosenSimulation;
        private set => _chosenSimulation = value switch
        {
            TopPanelOption.StepwiseSim => TopPanelOption.StepwiseSim,
            TopPanelOption.InstantSim => TopPanelOption.InstantSim,
            _ => throw new InvalidProgramException("chuj")
        };
    }
}