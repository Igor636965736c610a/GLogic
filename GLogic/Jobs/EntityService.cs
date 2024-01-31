using System.Diagnostics;
using GLogic.Components;
using GLogic.Components.Common;

namespace GLogic.Jobs;

public static class EntityService
{
    public static void AddLGate(Vector2Int cursor, IoType ioType, bool value)
    {
        var initIoComponent = new InitIoComponentInfo(ioType, 
            new Entity { Id = uint.MaxValue },
            new Entity { Id = uint.MaxValue }, 0, value);
        var size = new Vector2Int(100, 50);
        var position = new Vector2Int
        {
            X = (int)(cursor.X + Renderer.CameraShift.X - (size.X * Renderer.Zoom / 2)),
            Y = (int)(cursor.Y + Renderer.CameraShift.Y - (size.Y * Renderer.Zoom / 2)),
        };
        var initTransformComponent = new InitTransformComponentInfo(position, size);
        var initEntity = new InitEntityInfo(initTransformComponent, initIoComponent);

        EntityManager.CreateEntity(initEntity);
    }
}