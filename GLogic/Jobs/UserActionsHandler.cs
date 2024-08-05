using System.Diagnostics;
using GLogic.Data;
using GLogic.Data.Panels;
using GLogic.Data.State;
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
    private readonly LayoutArrangement _layoutArrangement;
    private IUserActionExecutor _userActionExecutor;

    public UserActionsHandler(IRendererConfig rendererConfig, ICircuitUpdaterConfig circuitUpdaterConfig,
        IUserActionExecutor userActionExecutor, LayoutArrangement layoutArrangement)
    {
        _userActionExecutor = userActionExecutor;
        _circuitUpdaterConfig = circuitUpdaterConfig;
        _rendererConfig = rendererConfig;
        _layoutArrangement = layoutArrangement;
    }

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
                InputState.MouseLeftButtonState = false;
                break;
            case SDL.SDL_BUTTON_RIGHT:
                InputState.MouseRightButtonState = false;
                break;
        }
    }

    public void HandleMouseDownPollEvent(Vector2Int cursor, uint mouseButton)
    {
        switch (mouseButton)
        {
            case SDL.SDL_BUTTON_LEFT when !InputState.MouseRightButtonState:
                LeftClickDown(cursor);
                
                InputState.MouseLeftButtonState = true;
                break;
            case SDL.SDL_BUTTON_RIGHT:
                InputState.MouseRightButtonState = true;
                break;
        }
    }

    public void HandleKeyDownEvent(SDL.SDL_Keycode key)
    {
        if (key == SDL.SDL_Keycode.SDLK_LSHIFT)
        {
            InputState.ShiftKeyState = true;
        }
    }

    public void HandleKeyUpEvent(SDL.SDL_Keycode key)
    {
        if (key == SDL.SDL_Keycode.SDLK_LSHIFT)
        {
            InputState.ShiftKeyState = false;
        }
    }

    private void LeftClickDown(Vector2Int cursor)
    {
        switch (GetUIComponent(cursor))
        {
            case UIComponent.LeftPanel:
                WireService.Reset();
                LeftPanelClickExecute(cursor);
                
                break;
            case UIComponent.TopPanel:
                WireService.Reset();
                
                TopPanelClickExecute(cursor);
                break;
            case UIComponent.WorkingArea:
                var adjustedCursorPosition = _rendererConfig.GetRelativeShiftedCursor(cursor);
                _userActionExecutor.ClickExecute(adjustedCursorPosition, MenuState.ChosenLGate);
                
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void LeftPanelClickExecute(Vector2Int cursor)
    {
        var option = _layoutArrangement.LeftPanel.GetClickedOption(cursor);
        if (option is null)
        {
            return;
        }
        
        MenuState.ChosenLGate = MenuState.ChosenLGate == option ? LeftPanelOption.None : option.Value;
    }

    private void TopPanelClickExecute(Vector2Int cursor)
    {
        var option = _layoutArrangement.TopPanel.GetClickedOption(cursor);
        if (option is null)
        {
            return;
        }

        switch (option)
        {
            case TopPanelOption.Reset:
                _circuitUpdaterConfig.Reset();
                
                break;
            case TopPanelOption.InstantSim:
                _circuitUpdaterConfig.TerminateBackgroundUpdater();
                {
                    var userActionExecutor = _circuitUpdaterConfig.ToInstantSimulation();
                    _userActionExecutor = userActionExecutor;
                }
                break;
            case TopPanelOption.StepwiseSim:
                _circuitUpdaterConfig.TerminateBackgroundUpdater();
                {
                    var userActionExecutor = _circuitUpdaterConfig.ToStepWiseSimulation();
                    _userActionExecutor = userActionExecutor;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public void HandleMouseHeldAction(Vector2Int relativeCursorPosition)
    {
        if (InputState.MouseRightButtonState)
        {
            HandleMouseRightHeldAction(relativeCursorPosition);
        }
        else if (InputState.MouseLeftButtonState)
        {
            HandleMouseLeftHeldAction();
        }
    }

    private void HandleMouseRightHeldAction(Vector2Int relativeCursorPosition)
    {
        Debug.Assert(InputState.MouseRightButtonState);

        _rendererConfig.ShiftCamera(relativeCursorPosition);
    }

    private void HandleMouseLeftHeldAction()
    {
        SDL.SDL_GetMouseState(out var x, out var y);

        _userActionExecutor.HeldExecute(_rendererConfig.GetRelativeShiftedCursor(new Vector2Int(x, y)));
    }

    private UIComponent GetUIComponent(Vector2Int cursor)
    {
        if (cursor.X <= _layoutArrangement.LeftPanel.Rect.Size.X)
        {
            return UIComponent.LeftPanel;
        }

        else if (cursor.Y <= _layoutArrangement.TopPanel.Rect.Size.Y)
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