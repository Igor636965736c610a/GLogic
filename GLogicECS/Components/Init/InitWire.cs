using GLogicECS.Components.Common;

namespace GLogicECS.Components.Init;

public record InitWire(InitTransformComponent TransformComponent, InitWireComponent WireComponent, ConnectionInfo Input,
    ConnectionInfo Output);