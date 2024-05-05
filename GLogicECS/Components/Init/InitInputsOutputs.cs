using GLogicECS.Collections;
using GLogicECS.Components.Common;

namespace GLogicECS.Components.Init;

public record struct InitInputsOutputs(SmallList<ConnectionInfo> Inputs, SmallList<ConnectionInfo> Outputs);