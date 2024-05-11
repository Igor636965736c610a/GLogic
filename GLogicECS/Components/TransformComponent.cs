using GLogicGlobal.Common;

namespace GLogicECS.Components;

public struct TransformComponent
{
    public Entity Entity;
    public Vector2Int Position;
    public Vector2Int Size;
}