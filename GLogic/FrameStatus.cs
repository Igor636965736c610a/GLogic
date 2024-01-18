using System.Numerics;

namespace GLogic;

public class FrameStatus
{
    public bool Loop { get; set; }
    public float Zoom { get; set; }
    public uint Frame { get; set; }
    public Vector2 CursorPosition { get; set; }
    public Vector2 CameraField { get; set; }
    public CurrentMenuOption CurrentMenuOption { get; set; }
}