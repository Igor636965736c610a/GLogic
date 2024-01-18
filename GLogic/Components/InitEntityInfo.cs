using System.Numerics;

namespace GLogic.Components;

public record InitEntityInfo(TransformComponent TransformComponent, IoType IoType, Entity Input1, Entity Input2, uint Frame);