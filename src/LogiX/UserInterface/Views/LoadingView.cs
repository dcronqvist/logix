using System.Numerics;
using DotGL;
using DotGLFW;
using ImGuiNET;
using LogiX.UserInterfaceContext;

namespace LogiX.UserInterface.Views;

public class LoadingView : IView
{
    private readonly IUserInterfaceContext<InputState, Keys, ModifierKeys, MouseButton> _userInterfaceContext;

    public LoadingView(
        IUserInterfaceContext<InputState, Keys, ModifierKeys, MouseButton> userInterfaceContext)
    {
        _userInterfaceContext = userInterfaceContext;
    }

    public void Render(float deltaTime, float totalTime)
    {
        GL.glClearColor(0f, 0f, 0f, 1f);
        GL.glClear(GL.GL_COLOR_BUFFER_BIT);
    }

    public void SubmitGUI(float deltaTime, float totalTime)
    {
        var displaySize = _userInterfaceContext.GetWindowSizeAsVector2();
        var loadingWindowSize = new Vector2(350, 300);

        ImGui.SetNextWindowSize(loadingWindowSize, ImGuiCond.Always);
        ImGui.SetNextWindowPos(displaySize / 2 - loadingWindowSize / 2, ImGuiCond.Always);
        ImGui.Begin("Loading LogiX...", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove);

        ImGui.Text($"Loading... {totalTime:F0}s");

        ImGui.End();
    }

    public void Update(float deltaTime, float totalTime)
    {
    }
}
