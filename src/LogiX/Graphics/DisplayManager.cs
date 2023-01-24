using System;
using System.Drawing;
using System.Numerics;
using LogiX.Content;
using LogiX.GLFW;
using LogiX.OpenGL;
using LogiX.Rendering;
using static LogiX.OpenGL.GL;

namespace LogiX.Graphics;

public static class DisplayManager
{
    public static Window WindowHandle { get; set; }
    public static event EventHandler<Vector2> OnFramebufferResize;
    public static event EventHandler<bool> OnToggleFullscreen;
    private static bool manuallySetClose;
    public static int TargetFPS { get; set; }
    public static float FrameTime
    {
        get
        {
            return 1.0f / TargetFPS;
        }
    }

    private static void PrepareContext(bool maximized)
    {
        // Set some common hints for the OpenGL profile creation
        Glfw.WindowHint(Hint.ClientApi, ClientApi.OpenGL);
        Glfw.WindowHint(Hint.ContextVersionMajor, 3);
        Glfw.WindowHint(Hint.ContextVersionMinor, 3);
        Glfw.WindowHint(Hint.OpenglForwardCompatible, true);

        Glfw.WindowHint(Hint.CocoaRetinaFrameBuffer, false);
        Glfw.WindowHint(Hint.ScaleToMonitor, false);
        Glfw.WindowHint(Hint.HiDPIResize, true);

        Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);
        Glfw.WindowHint(Hint.Doublebuffer, true);
        Glfw.WindowHint(Hint.Decorated, true);
        Glfw.WindowHint(Hint.Maximized, maximized);

        manuallySetClose = false;
    }

    public static void SetTargetFPS(int fps)
    {
        TargetFPS = fps;
    }

    private static Window CreateWindow(int width, int height, string title, int minWidth = 1280, int minHeight = 720)
    {
        // Create window, make the OpenGL context current on the thread, and import graphics functions
        Window window = Glfw.CreateWindow(width, height, title, GLFW.Monitor.None, Window.None);

        // Center window
        Rectangle screen = Glfw.PrimaryMonitor.WorkArea;
        int x = (screen.Width - width) / 2;
        int y = (screen.Height - height) / 2;
        Glfw.SetWindowPosition(window, x, y);

        Glfw.MakeContextCurrent(window);
        GL.Import(Glfw.GetProcAddress);

        Glfw.SetWindowSizeLimits(window, minWidth, minHeight, screen.Width, screen.Height);

        return window;
    }

    public static float GetAspectRatio()
    {
        var size = GetWindowSizeInPixels();
        return size.X / size.Y;
    }

    public static void ReleaseGLContext()
    {
        Glfw.MakeContextCurrent(Window.None);
    }

    public static void AcquireGLContext()
    {
        Glfw.MakeContextCurrent(WindowHandle);
    }

    private static readonly object _glLock = new object();

    public static void LockedGLContext(Action action)
    {
        lock (_glLock)
        {
            var oldContext = Glfw.CurrentContext;

            if (oldContext == WindowHandle)
            {
                action();
                return;
            }

            Glfw.MakeContextCurrent(WindowHandle);
            action();
            Glfw.MakeContextCurrent(oldContext);
        }
    }

    public static bool HasGLContext()
    {
        return Glfw.CurrentContext == WindowHandle;
    }

    public unsafe static void InitWindow(int width, int height, string title, bool maximized, int minWidth = 1280, int minHeight = 720)
    {
        PrepareContext(maximized);
        WindowHandle = CreateWindow(width, height, title, minWidth, minHeight);
        Input.Init();

        Glfw.SetWindowMaximizeCallback(WindowHandle, (window, maximized) =>
        {
            Vector2 windowSize = GetWindowSizeInPixels();
            OnFramebufferResize?.Invoke(null, windowSize);
            //Logging.Log(LogLevel.Info, $"Window size changed to {windowSize.X}x{windowSize.Y} because of {(maximized ? "maximization" : "minimization")}");
            OnToggleFullscreen?.Invoke(null, maximized);
        });

        Glfw.SetFramebufferSizeCallback(WindowHandle, (Window, w, h) =>
        {
            OnFramebufferResize?.Invoke(null, new Vector2(w, h));
            //Logging.Log(LogLevel.Info, $"Framebuffer size changed to {w}x{h}");
        });

        GL.glEnable(GL_DEBUG_OUTPUT);

        // GL.glDebugMessageCallback((source, type, id, severity, length, message, param) =>
        // {
        //     Console.WriteLine($"OpenGL: {message}");
        // }, (void*)0);
    }

    public static void CloseWindow()
    {
        Glfw.Terminate();
    }

    public static Vector2 GetWindowSizeInPixels()
    {
        Glfw.GetFramebufferSize(WindowHandle,
                                out int width,
                                out int height);

        return new Vector2(width, height);
    }

    public static void SetWindowSizeInPixels(Vector2 size)
    {
        Glfw.SetWindowSize(WindowHandle, (int)size.X, (int)size.Y);
    }

    public static bool IsWindowFocused()
    {
        return Glfw.GetWindowAttribute(WindowHandle, WindowAttribute.Focused);
    }

    public unsafe static void SetWindowIcon(Texture2D texture)
    {
        Glfw.SetWindowIcon(WindowHandle, 1, new Image[] { texture.GetAsGLFWImage() });
    }

    public unsafe static void SetCursorToPlatformStandard(CursorType cursorType)
    {
        var cursor = Glfw.CreateStandardCursor(cursorType);
        Glfw.SetCursor(WindowHandle, cursor);
    }

    public unsafe static void SetCursorToTexture(Texture2D texture, int xHotspot, int yHotspot)
    {
        Glfw.SetCursor(WindowHandle, Glfw.CreateCursor(texture.GetAsGLFWImage(), xHotspot, yHotspot));
    }

    public static void SetWindowShouldClose(bool value)
    {
        manuallySetClose = value;
    }

    public static bool GetWindowShouldClose()
    {
        return Glfw.WindowShouldClose(WindowHandle) || manuallySetClose;
    }

    public static void SwapBuffers(int swapInterval = 0)
    {
        Glfw.SwapInterval(swapInterval);
        Glfw.SwapBuffers(WindowHandle);
    }

    public static void PollEvents()
    {
        Glfw.PollEvents();
    }

    public static void SetWindowTitle(string title)
    {
        Glfw.SetWindowTitle(WindowHandle, title);
    }

    public static Vector2 GetViewSize(Camera2D camera)
    {
        Vector2 windowSize = GetWindowSizeInPixels();
        return new Vector2(windowSize.X / camera.Zoom, windowSize.Y / camera.Zoom);
    }
}
