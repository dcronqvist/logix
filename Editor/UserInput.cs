namespace LogiX.Editor;

public static class UserInput
{
    private static Vector2 currentMousePos;
    private static Vector2 previousMousePos;

    public static void Begin()
    {
        currentMousePos = Raylib.GetMousePosition();
    }

    public static void End()
    {
        previousMousePos = currentMousePos;
    }

    public static Vector2 GetMouseDelta(Camera2D camera)
    {
        return (currentMousePos - previousMousePos) / camera.zoom;
    }
}