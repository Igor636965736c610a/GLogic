using GLogic.Components.Common;

namespace GLogic.Jobs;

public interface IRendererConfig
{
    void ZoomIn();
    void ZoomOut();
    void ShiftCamera(Vector2Int shiftVector);
}