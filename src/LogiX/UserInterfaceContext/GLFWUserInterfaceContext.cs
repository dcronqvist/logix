using System;
using DotGL;
using DotGLFW;
using LogiX.Graphics.Textures;

namespace LogiX.UserInterfaceContext;

public class GLFWUserInterfaceContext : IUserInterfaceContext<InputState, Keys, ModifierKeys, MouseButton>
{
    private Window _windowHandle;

    public event EventHandler<(int, int)> WindowSizeChanged;
    public event EventHandler<(MouseButton, InputState, ModifierKeys)> MouseButtonChanged;
    public event EventHandler<int> MouseWheelScrolled;
    public event EventHandler<char> CharacterTyped;
    public event EventHandler<(Keys, int, InputState, ModifierKeys)> KeyChanged;

    public GLFWUserInterfaceContext()
    {
        Glfw.Init();
        Glfw.WindowHint(Hint.ClientAPI, ClientAPI.OpenGLAPI);
        Glfw.WindowHint(Hint.ContextVersionMajor, 3);
        Glfw.WindowHint(Hint.ContextVersionMinor, 3);
        Glfw.WindowHint(Hint.OpenGLProfile, OpenGLProfile.CoreProfile);
        Glfw.WindowHint(Hint.DoubleBuffer, true);
        Glfw.WindowHint(Hint.Decorated, true);
        Glfw.WindowHint(Hint.OpenGLForwardCompat, true);
        Glfw.WindowHint(Hint.Resizable, true);

        int width = 1280;
        int height = 720;

        _windowHandle = Glfw.CreateWindow(width, height, "LogiX", Monitor.NULL, Window.NULL);

        Monitor primaryMonitor = Glfw.GetPrimaryMonitor();
        Glfw.GetMonitorWorkarea(primaryMonitor, out _, out _, out var ww, out var wh);

        int windowCenteredX = ww / 2 - width / 2;
        int windowCenteredY = wh / 2 - height / 2;
        Glfw.SetWindowPos(_windowHandle, windowCenteredX, windowCenteredY);

        Glfw.MakeContextCurrent(_windowHandle);
        GL.Import(Glfw.GetProcAddress);

        Glfw.SwapInterval(1);

        Glfw.SetFramebufferSizeCallback(_windowHandle, (window, width, height) =>
        {
            WindowSizeChanged?.Invoke(this, (width, height));
        });

        Glfw.SetMouseButtonCallback(_windowHandle, (window, button, state, mods) =>
        {
            MouseButtonChanged?.Invoke(this, (button, state, mods));
        });

        Glfw.SetScrollCallback(_windowHandle, (window, xOffset, yOffset) =>
        {
            MouseWheelScrolled?.Invoke(this, (int)yOffset);
        });

        Glfw.SetCharCallback(_windowHandle, (window, codePoint) =>
        {
            CharacterTyped?.Invoke(this, (char)codePoint);
        });

        Glfw.SetKeyCallback(_windowHandle, (window, key, scanCode, state, mods) =>
        {
            KeyChanged?.Invoke(this, (key, scanCode, state, mods));
        });
    }

    public bool ContextRequestsClose()
    {
        return Glfw.WindowShouldClose(_windowHandle);
    }

    public void Destroy()
    {
        Glfw.Terminate();
    }

    public InputState GetKeyState(Keys key)
    {
        return Glfw.GetKey(_windowHandle, key);
    }

    public InputState GetMouseButtonState(MouseButton button)
    {
        return Glfw.GetMouseButton(_windowHandle, button);
    }

    public int GetMousePositionInWindowX()
    {
        Glfw.GetCursorPos(_windowHandle, out var x, out _);
        return (int)Math.Floor(x);
    }

    public int GetMousePositionInWindowY()
    {
        Glfw.GetCursorPos(_windowHandle, out _, out var y);
        return (int)Math.Floor(y);
    }

    public double GetTimeSinceLaunch()
    {
        return Glfw.GetTime();
    }

    public int GetWindowHeight()
    {
        Glfw.GetFramebufferSize(_windowHandle, out var width, out var height);
        return height;
    }

    public int GetWindowWidth()
    {
        Glfw.GetFramebufferSize(_windowHandle, out var width, out var height);
        return width;
    }

    public bool IsWindowFocused()
    {
        return Glfw.GetWindowAttrib(_windowHandle, WindowAttrib.Focused);
    }

    public void PerformFrame()
    {
        Glfw.SwapBuffers(_windowHandle);
        Glfw.PollEvents();
    }

    public void SetWindowTitle(string title)
    {
        Glfw.SetWindowTitle(_windowHandle, title);
    }

    public void SetWindowIcon(ITexture2D iconTexture)
    {
        byte[] textureData = iconTexture.GetPixelData();

        Glfw.SetWindowIcon(_windowHandle, new Image[]
        {
            new Image
            {
                Width = iconTexture.Width,
                Height = iconTexture.Height,
                Pixels = textureData
            }
        });
    }
}
