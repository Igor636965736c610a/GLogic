using System.Numerics;

namespace GLogic;

public class AppStatus
{
    public float Zoom { get; set; }
    public uint Frame { get; set; }
    public Vector2 CursorPosition { get; set; }
    public Vector2 CameraField { get; set; }
}