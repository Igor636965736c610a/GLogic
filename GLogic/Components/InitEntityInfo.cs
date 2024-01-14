using System.Numerics;

namespace GLogic.Components;

public record struct InitEntityInfo(Vector2 Position, IoType IoType, Entity Input1, Entity Input2, uint Frame);