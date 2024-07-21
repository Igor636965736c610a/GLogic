using GLogic.Data;
using GLogic.Jobs.Renderer;
using GLogicECS.Components;
using GLogicGlobal.Common;

namespace GLogic.Jobs.Internal;

public interface IUserActionExecutor
{
    Entity HeldEntity { get; set; }
    void ClickExecute(Vector2Int adjustedCursorPosition, LeftPanelOptions chosenLeftPanelOptions);
    void HeldExecute(Vector2Int adjustedCursorPosition);
}