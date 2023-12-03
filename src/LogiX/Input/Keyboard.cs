using System;
using System.Collections.Generic;
using System.Linq;
using DotGLFW;
using LogiX.Eventing;
using LogiX.UserInterfaceContext;

namespace LogiX.Input;

public class Keyboard : IKeyboard<char, Keys, ModifierKeys>
{
    private readonly IUserInterfaceContext<InputState, Keys, ModifierKeys, MouseButton> _userInterfaceContext;

    private Dictionary<Keys, bool> _currentKeyboardState;
    private Dictionary<Keys, bool> _previousKeyboardState;

    public IEventProvider<char> CharacterTypedEventProvider { get; } = new EventProvider<char>();
    public IEventProvider<Keys> KeyPressedEventProvider { get; } = new EventProvider<Keys>();

    public event EventHandler<char> CharacterTyped;
    public event EventHandler<(Keys, ModifierKeys)> KeyPressed;
    public event EventHandler<(Keys, ModifierKeys)> KeyPressedOrRepeated;
    public event EventHandler<(Keys, ModifierKeys)> KeyReleased;

    public Keyboard(
        IUserInterfaceContext<InputState, Keys, ModifierKeys, MouseButton> userInterfaceContext)
    {
        _userInterfaceContext = userInterfaceContext;

        _userInterfaceContext.CharacterTyped += (sender, codePoint) =>
        {
            CharacterTyped?.Invoke(this, codePoint);
            CharacterTypedEventProvider.NotifySubscribers(codePoint);
        };

        _userInterfaceContext.KeyChanged += (sender, e) =>
        {
            var key = e.Item1;
            int scanCode = e.Item2;
            var state = e.Item3;
            var mods = e.Item4;

            if (state.HasFlag(InputState.Press) && !state.HasFlag(InputState.Repeat))
            {
                KeyPressed?.Invoke(this, (key, mods));
                KeyPressedEventProvider.NotifySubscribers(key);
            }
            else if (state.HasFlag(InputState.Press) || state.HasFlag(InputState.Repeat))
            {
                KeyPressedOrRepeated?.Invoke(this, (key, mods));
            }
            else if (state.HasFlag(InputState.Release))
            {
                KeyReleased?.Invoke(this, (key, mods));
            }
        };
    }

    public void Begin()
    {
        _currentKeyboardState = GetKeyboardState();
        _previousKeyboardState ??= _currentKeyboardState;
    }

    public void End() => _previousKeyboardState = _currentKeyboardState;

    public bool IsKeyDown(Keys key) => _currentKeyboardState[key];

    public bool IsKeyPressed(Keys key) => _currentKeyboardState[key] && !_previousKeyboardState[key];

    public bool IsKeyReleased(Keys key) => !_currentKeyboardState[key] && _previousKeyboardState[key];

    public bool IsKeyCombinationPressed(params Keys[] keys)
    {
        foreach (var key in keys.Take(keys.Length - 1))
        {
            if (!IsKeyDown(key))
            {
                return false;
            }
        }

        bool lastPressed = IsKeyPressed(keys.Last());
        var current = _currentKeyboardState.Where(kvp => kvp.Value).Select(kvp => kvp.Key);

        return lastPressed && !current.Except(keys).Any();
    }

    public bool TryGetNextKeyPressed(out Keys key)
    {
        if (_currentKeyboardState.Any(kvp => kvp.Value && _previousKeyboardState[kvp.Key] == false))
        {
            key = _currentKeyboardState.First(kvp => kvp.Value && _previousKeyboardState[kvp.Key] == false).Key;
            return true;
        }
        key = Keys.Unknown;
        return false;
    }

    private Dictionary<Keys, bool> GetKeyboardState()
    {
        var keys = Enum.GetValues<Keys>();
        var dic = new Dictionary<Keys, bool>();
        foreach (var key in keys)
        {
            if (key != Keys.Unknown)
            {
                dic.Add(key, _userInterfaceContext.GetKeyState(key) == InputState.Press);
            }
        }
        return dic;
    }
}
