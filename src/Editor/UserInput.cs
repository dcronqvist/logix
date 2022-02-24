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

    public static Vector2 GetMouseDelta(Camera2D cam)
    {
        return (currentMousePos - previousMousePos) / cam.zoom;
    }

    public static Vector2 GetViewSize(Camera2D cam)
    {
        int windowWidth = Raylib.GetScreenWidth();
        int windowHeight = Raylib.GetScreenHeight();
        return GetViewSize(new Vector2(windowWidth, windowHeight), cam);
    }

    public static Vector2 GetViewSize(Vector2 windowSize, Camera2D cam)
    {
        Vector2 viewSize = new Vector2(windowSize.X / cam.zoom, windowSize.Y / cam.zoom);
        return viewSize;
    }

    public static Vector2 GetMousePositionInWindow()
    {
        return currentMousePos;
    }

    public static Vector2 GetMousePositionInWorld(Camera2D cam, Vector2? worldSize = null, Vector2? topLeftOffset = null)
    {
        Vector2 viewSize = GetViewSize(cam);
        Vector2 offset = Vector2.Zero;
        if (topLeftOffset != null)
        {
            offset = topLeftOffset.Value;
        }
        Vector2 topLeft = new Vector2(cam.target.X - viewSize.X / 2.0F, cam.target.Y - viewSize.Y / 2.0F);
        return new Vector2(topLeft.X + (currentMousePos.X - offset.X) / cam.zoom, topLeft.Y + (currentMousePos.Y - offset.Y) / cam.zoom);
    }

    public static bool KeyComboPressed(KeyboardKey hold, KeyboardKey press)
    {
        return Raylib.IsKeyDown(hold) && Raylib.IsKeyPressed(press);
    }

    public static string KeyComboString(KeyboardKey hold, KeyboardKey press)
    {
        return hold.Pretty() + " + " + press.Pretty();
    }
}