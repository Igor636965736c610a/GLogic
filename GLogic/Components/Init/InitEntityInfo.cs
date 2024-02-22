using System.Numerics;

namespace GLogic.Components.Init;

public record InitEntityInfo(InitTransformComponentInfo TransformComponentInfo, InitIoComponentInfo IoComponentInfo, IoType IoType);