using System;
using System.Numerics;
using DotGLFW;
using LogiX.Graphics.Textures;

namespace LogiX.UserInterfaceContext;

public interface IUserInterfaceContext<TInputState, TKeys, TModifierKeys, TMouseButtons>
{
    event EventHandler<(int, int)> WindowSizeChanged;
    event EventHandler<(TMouseButtons, TInputState, TModifierKeys)> MouseButtonChanged;
    event EventHandler<int> MouseWheelScrolled;
    event EventHandler<char> CharacterTyped;
    event EventHandler<(TKeys, int, TInputState, TModifierKeys)> KeyChanged;

    void Destroy();

    void SetWindowTitle(string title);
    void SetWindowIcon(ITexture2D iconTexture);

    int GetWindowWidth();
    int GetWindowHeight();
    Vector2 GetWindowSizeAsVector2() => new(GetWindowWidth(), GetWindowHeight());

    InputState GetMouseButtonState(TMouseButtons button);
    int GetMousePositionInWindowX();
    int GetMousePositionInWindowY();

    InputState GetKeyState(TKeys key);

    bool IsWindowFocused();
    bool ContextRequestsClose();
    double GetTimeSinceLaunch();

    void PerformFrame();
}
