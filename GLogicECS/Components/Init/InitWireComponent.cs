using GLogicECS.Components;

namespace GLogicECS.Components.Init;

public record struct InitWireComponent(bool Increasing, uint InputHookNumber, uint OutputHookNumber);