using GLogic.Jobs.Renderer;

namespace GLogic.Data.Panels;

public record PanelOption<T>(Area Rect, T Option) where T : Enum;