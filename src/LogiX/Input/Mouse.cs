using System;
using System.Collections.Generic;
using System.Numerics;
using DotGLFW;
using LogiX.Eventing;
using LogiX.UserInterfaceContext;

namespace LogiX.Input;

public class Mouse : IMouse<MouseButton>
{
    private readonly IUserInterfaceContext<InputState, Keys, ModifierKeys, MouseButton> _userInterfaceContext;

    private Vector2 _mousePositionInWindow;
    private Vector2 _previousMousePositionInWindow;

    private Dictionary<MouseButton, bool> _currentMouseState;
    private Dictionary<MouseButton, bool> _previousMouseState;

    public IEventProvider<MouseButton> MouseButtonPressedEventProvider { get; } = new EventProvider<MouseButton>();

    public event EventHandler<int> MouseWheelScrolled;

    public Mouse(
        IUserInterfaceContext<InputState, Keys, ModifierKeys, MouseButton> userInterfaceContext)
    {
        _userInterfaceContext = userInterfaceContext;

        _userInterfaceContext.MouseWheelScrolled += (sender, offset) =>
        {
            MouseWheelScrolled?.Invoke(this, offset);
        };

        _userInterfaceContext.MouseButtonChanged += (sender, e) =>
        {
            var button = e.Item1;
            var state = e.Item2;

            if (state.HasFlag(InputState.Press))
            {
                MouseButtonPressedEventProvider.NotifySubscribers(button);
            }
        };
    }

    public void Begin()
    {
        _currentMouseState = GetMouseState();
        _mousePositionInWindow = new Vector2(GetMouseXInWindow(), GetMouseYInWindow());
    }

    public void End()
    {
        _previousMouseState = _currentMouseState;
        _previousMousePositionInWindow = _mousePositionInWindow;
    }

    public int GetMouseXInWindow()
    {
        return _userInterfaceContext.GetMousePositionInWindowX();
    }

    public int GetMouseYInWindow()
    {
        return _userInterfaceContext.GetMousePositionInWindowY();
    }

    public bool IsMouseButtonDown(MouseButton button)
    {
        return _currentMouseState[button];
    }

    public bool IsMouseButtonPressed(MouseButton button)
    {
        return _currentMouseState[button] && !_previousMouseState[button];
    }

    public bool IsMouseButtonReleased(MouseButton button)
    {
        return !_currentMouseState[button] && _previousMouseState[button];
    }

    private Dictionary<MouseButton, bool> GetMouseState()
    {
        MouseButton[] mouseButtons = Enum.GetValues<MouseButton>();
        Dictionary<MouseButton, bool> dic = new Dictionary<MouseButton, bool>();

        foreach (MouseButton button in mouseButtons)
        {
            if (!dic.ContainsKey(button))
            {
                dic.Add(button, _userInterfaceContext.GetMouseButtonState(button) == InputState.Press);
            }
        }

        return dic;
    }

    public Vector2 GetMouseDeltaInWindowAsVector2()
    {
        return _mousePositionInWindow - _previousMousePositionInWindow;
    }
}
