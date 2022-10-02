using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX;

public static class Input
{
    public static Dictionary<Keys, bool> currentKeyboardState;
    public static Dictionary<Keys, bool> previousKeyboardState;

    public static Dictionary<MouseButton, bool> currentMouseState;
    public static Dictionary<MouseButton, bool> previousMouseState;

    public static double currentMouseScroll;
    public static double previousMouseScroll;

    public static Vector2 currentMousePosition;
    public static Vector2 previousMousePosition;

    public static event EventHandler<char> OnChar;
    public static event EventHandler OnBackspace;
    public static event EventHandler<float> OnScroll;
    public static event EventHandler<Tuple<char, ModifierKeys>> OnCharMods;
    public static event EventHandler OnEnterPressed;
    public static event EventHandler<Tuple<Keys, ModifierKeys>> OnKeyPressOrRepeat;

    public static void Init()
    {
        currentKeyboardState = GetKeyboardState();
        previousKeyboardState = currentKeyboardState;

        currentMouseState = GetMouseState();
        previousMouseState = currentMouseState;

        currentMousePosition = GetMousePositionInWindow();
        previousMousePosition = currentMousePosition;

        Glfw.SetCharCallback(DisplayManager.WindowHandle, (Window, codePoint) =>
        {
            OnChar?.Invoke(null, (char)codePoint);
        });

        Glfw.SetKeyCallback(DisplayManager.WindowHandle, (Window, key, scanCode, state, mods) =>
        {
            if (key == Keys.Backspace)
            {
                if (state != InputState.Release)
                {
                    OnBackspace?.Invoke(null, EventArgs.Empty);
                }
            }
            else if (key == Keys.Enter)
            {
                if (state != InputState.Release)
                {
                    OnEnterPressed?.Invoke(null, EventArgs.Empty);
                }
            }

            string s = Glfw.GetKeyName(key, scanCode);
            if (s != "" && mods != 0 && (state.HasFlag(InputState.Press) || state.HasFlag(InputState.Repeat)))
            {
                OnCharMods?.Invoke(null, new Tuple<char, ModifierKeys>(s[0], mods));
            }

            if ((state.HasFlag(InputState.Press) || state.HasFlag(InputState.Repeat)))
            {
                OnKeyPressOrRepeat?.Invoke(null, new Tuple<Keys, ModifierKeys>(key, mods));
            }
        });

        Glfw.SetScrollCallback(DisplayManager.WindowHandle, (window, x, y) =>
        {
            currentMouseScroll += y;
            OnScroll?.Invoke(null, (float)y);
        });
    }

    public static Dictionary<Keys, bool> GetKeyboardState()
    {
        Keys[] keys = Enum.GetValues<Keys>();
        Dictionary<Keys, bool> dic = new Dictionary<Keys, bool>();
        foreach (Keys key in keys)
        {
            if (key != Keys.Unknown)
            {
                dic.Add(key, Glfw.GetKey(DisplayManager.WindowHandle, key) == InputState.Press);
            }
        }
        return dic;
    }

    public static Dictionary<MouseButton, bool> GetMouseState()
    {
        MouseButton[] mouseButtons = Enum.GetValues<MouseButton>();
        Dictionary<MouseButton, bool> dic = new Dictionary<MouseButton, bool>();

        foreach (MouseButton button in mouseButtons)
        {
            if (!dic.ContainsKey(button))
            {
                dic.Add(button, Glfw.GetMouseButton(DisplayManager.WindowHandle, button) == InputState.Press);
            }
        }

        return dic;
    }

    public static void Begin()
    {
        currentKeyboardState = GetKeyboardState();
        currentMouseState = GetMouseState();
        currentMousePosition = GetMousePositionInWindow();
    }

    public static void End()
    {
        previousKeyboardState = currentKeyboardState;
        previousMouseState = currentMouseState;
        previousMouseScroll = currentMouseScroll;
        previousMousePosition = currentMousePosition;
    }

    public static bool IsKeyDown(Keys key)
    {
        return currentKeyboardState[key];
    }

    public static bool IsKeyPressed(Keys key)
    {
        return currentKeyboardState[key] && !previousKeyboardState[key];
    }

    public static bool IsKeyReleased(Keys key)
    {
        return !currentKeyboardState[key] && previousKeyboardState[key];
    }

    public static bool IsMouseButtonDown(MouseButton button)
    {
        return currentMouseState[button];
    }

    public static bool IsMouseButtonPressed(MouseButton button)
    {
        return currentMouseState[button] && !previousMouseState[button];
    }

    public static bool IsMouseButtonReleased(MouseButton button)
    {
        return !currentMouseState[button] && previousMouseState[button];
    }

    public static Vector2 GetMousePosition(Camera2D offsetCamera)
    {
        Vector2 windowSize = DisplayManager.GetWindowSizeInPixels();
        Vector2 topLeft = offsetCamera.TopLeft;

        Glfw.GetCursorPosition(DisplayManager.WindowHandle, out double x, out double y);

        return topLeft + (new Vector2((float)x, (float)y)) / offsetCamera.Zoom;
    }

    public static Vector2 GetMousePositionInWindow()
    {
        Glfw.GetCursorPosition(DisplayManager.WindowHandle, out double x, out double y);
        return new Vector2((float)x, (float)y);
    }

    public static Vector2 GetMouseWindowDelta()
    {
        return currentMousePosition - previousMousePosition;
    }

    public static int GetScroll()
    {
        return (int)(currentMouseScroll - previousMouseScroll);
    }
}
