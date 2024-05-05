using GLogic.Components.Common;

namespace GLogicECS.Components.Init;

public record InitTransformComponent(Vector2Int Position, Vector2Int Size)
{
    public TransformComponent ToTransformComponent(Entity entity)
        => new()
        {
            Entity = entity,
            Position = Position,
            Size = Size
        };
}