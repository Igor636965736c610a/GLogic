using System.Diagnostics;
using GLogic.Data;
using GLogic.Jobs.AppUpdaters;
using GLogic.Jobs.Internal;
using GLogic.Jobs.Internal.EcsStateModifiers;
using GLogic.Jobs.Renderer;
using GLogicECS.Components;
using GLogicECS.Components.Common;
using GLogicGlobal.Common;
using SDL2;
// ReSharper disable InconsistentNaming

namespace GLogic.Jobs;

public sealed class UserActionsHandler
{
    private readonly IRendererConfig _rendererConfig;
    private readonly ICircuitUpdaterConfig _circuitUpdaterConfig;
    private readonly IUserActionExecutor _userActionExecutor;
    private readonly LayoutArrangement _layoutArrangement;

    public UserActionsHandler(IRendererConfig rendererConfig, ICircuitUpdaterConfig circuitUpdaterConfig,
        IUserActionExecutor userActionExecutor, LayoutArrangement layoutArrangement)
    {
        ChosenLeftPanelOptions = LeftPanelOptions.None;
        _userActionExecutor = userActionExecutor;
        _circuitUpdaterConfig = circuitUpdaterConfig;
        _rendererConfig = rendererConfig;
        _layoutArrangement = layoutArrangement;
    }

    public static LeftPanelOptions ChosenLeftPanelOptions { get; private set; }
    public static bool MouseRightButtonState { get; private set; }
    public static bool MouseLeftButtonState { get; private set; }
    public static bool ShiftKeyState { get; private set; }

    public void HandleMouseWheel(Vector2Int cursor, int wheelY)
    {
        _rendererConfig.ChangeRelativelyToCursorZoom((float)(wheelY * 0.1), cursor);
    }
    
    public void HandleMouseUpPollEvent(Vector2Int cursor, uint mouseButton)
    {
        switch (mouseButton)
        {
            case SDL.SDL_BUTTON_LEFT:
                _userActionExecutor.HeldEntity = new Entity(IdStructure.MakeInvalidId());
                MouseLeftButtonState = false;
                break;
            case SDL.SDL_BUTTON_RIGHT:
                MouseRightButtonState = false;
                break;
        }
    }

    public void HandleMouseDownPollEvent(Vector2Int cursor, uint mouseButton)
    {
        switch (mouseButton)
        {
            case SDL.SDL_BUTTON_LEFT when !MouseRightButtonState:
                LeftClickDown(cursor);

                MouseLeftButtonState = true;
                break;
            case SDL.SDL_BUTTON_RIGHT:
                MouseRightButtonState = true;
                break;
        }
    }

    public void HandleKeyDownEvent(SDL.SDL_Keycode key)
    {
        if (key == SDL.SDL_Keycode.SDLK_LSHIFT)
        {
            ShiftKeyState = true;
        }
    }

    public void HandleKeyUpEvent(SDL.SDL_Keycode key)
    {
        if (key == SDL.SDL_Keycode.SDLK_LSHIFT)
        {
            ShiftKeyState = false;
        }
    }

    private void LeftClickDown(Vector2Int cursor)
    {
        switch (GetUIComponent(cursor))
        {
            case UIComponent.LeftPanel:
                WireService.Reset();
                SetChosenLGate(cursor);
                
                break;
            case UIComponent.TopPanel:
                break;
            case UIComponent.WorkingArea:
                DoUserAction(cursor);
                
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void SetChosenLGate(Vector2Int cursor)
    {
        var i = 0;
        foreach (var option in _layoutArrangement.LeftPanelOptions)
        {
            if (
                cursor.X >= option.Position.X
                && cursor.X <= option.Position.X + option.Size.X
                && cursor.Y >= option.Position.Y
                && cursor.Y < option.Position.Y + option.Size.Y
            )
            {
                if (ChosenLeftPanelOptions == (LeftPanelOptions)i)
                {
                    ChosenLeftPanelOptions = LeftPanelOptions.None;
                }
                else
                {
                    ChosenLeftPanelOptions = (LeftPanelOptions)i;
                }
            }
            i++;
        }
    }

    private void DoUserAction(Vector2Int cursor)
    {
        var adjustedCursorPosition = _rendererConfig.GetRelativeShiftedCursor(cursor);

        _userActionExecutor.ClickExecute(adjustedCursorPosition, ChosenLeftPanelOptions);
    }

    public void HandleMouseHeldAction(Vector2Int relativeCursorPosition)
    {
        if (MouseRightButtonState)
        {
            HandleMouseRightHeldAction(relativeCursorPosition);
        }
        else if (MouseLeftButtonState)
        {
            HandleMouseLeftHeldAction();
        }
    }

    private void HandleMouseRightHeldAction(Vector2Int relativeCursorPosition)
    {
        Debug.Assert(MouseRightButtonState);

        _rendererConfig.ShiftCamera(relativeCursorPosition);
    }

    private void HandleMouseLeftHeldAction()
    {
        SDL.SDL_GetMouseState(out var x, out var y);

        _userActionExecutor.HeldExecute(_rendererConfig.GetRelativeShiftedCursor(new Vector2Int(x, y)));
    }

    private UIComponent GetUIComponent(Vector2Int cursor)
    {
        if (cursor.X <= _layoutArrangement.LeftPanelRect.Size.X)
        {
            return UIComponent.LeftPanel;
        }

        else if (cursor.Y <= _layoutArrangement.TopPanelRect.Size.Y)
        {
            return UIComponent.TopPanel;
        }

        else
        {
            return UIComponent.WorkingArea;
        }
    }

    private enum UIComponent
    {
        LeftPanel,
        TopPanel,
        WorkingArea
    }
}