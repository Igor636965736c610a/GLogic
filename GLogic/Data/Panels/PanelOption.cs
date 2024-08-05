using GLogic.Jobs.Renderer;

namespace GLogic.Data.Panels;

public sealed record PanelOption<T>(Area Rect, T Option) where T : Enum;