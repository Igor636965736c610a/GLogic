using GLogic.Jobs.Renderer;
using GLogicECS.Components;
using GLogicGlobal.Common;

namespace GLogic.Jobs.Internal;

public interface IUserActionExecutor
{
    Entity LGateToMove { get; set; }
    void ClickExecute(Vector2Int adjustedCursorPosition, MenuOption chosenMenuOption);
    void HeldExecute(Vector2Int adjustedCursorPosition);
}