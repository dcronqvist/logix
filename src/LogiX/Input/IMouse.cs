using System;
using System.Numerics;
using DotGLFW;
using LogiX.Eventing;

namespace LogiX.Input;

public interface IMouse<TMouseButton>
{
    IEventProvider<MouseButton> MouseButtonPressedEventProvider { get; }

    event EventHandler<int> MouseWheelScrolled;

    void Begin();
    void End();

    bool IsMouseButtonDown(TMouseButton button);
    bool IsMouseButtonPressed(TMouseButton button);
    bool IsMouseButtonReleased(TMouseButton button);

    int GetMouseXInWindow();
    int GetMouseYInWindow();

    Vector2 GetMouseDeltaInWindowAsVector2();
    Vector2 GetMousePositionInWindowAsVector2() => new(GetMouseXInWindow(), GetMouseYInWindow());
}
