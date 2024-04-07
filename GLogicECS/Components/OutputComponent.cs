using GLogicECS.Collections;

namespace GLogicECS.Components;

public struct OutputComponent
{
    public Entity Entity;
    public SmallList<Entity> Outputs;
}