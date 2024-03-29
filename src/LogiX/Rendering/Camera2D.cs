using System;
using System.Drawing;
using System.Numerics;
using LogiX.Graphics;

namespace LogiX.Rendering;

public class Camera2D
{
    public Vector2 FocusPosition { get; set; }
    public RectangleF VisibleArea
    {
        get
        {
            Vector2 windowSize = this.GetViewSize();

            float left = FocusPosition.X - windowSize.X / 2f / Zoom;
            float top = FocusPosition.Y - windowSize.Y / 2f / Zoom;
            float height = windowSize.Y / Zoom;
            float width = windowSize.X / Zoom;

            return new RectangleF(left, top, width, height);
        }
    }
    public Vector2 TopLeft
    {
        get
        {
            Vector2 windowSize = DisplayManager.GetWindowSizeInPixels();

            float left = FocusPosition.X - windowSize.X / 2f / Zoom;
            float top = FocusPosition.Y - windowSize.Y / 2f / Zoom;

            return new Vector2(left, top);
        }
    }

    public float Zoom { get; set; }
    public Vector2 FixedViewSize { get; set; }

    public Camera2D(Vector2 focusPosition, float zoom, Vector2 fixedViewSize = default)
    {
        FocusPosition = focusPosition;
        Zoom = zoom;
        FixedViewSize = fixedViewSize;
    }

    public Vector2 GetViewSize()
    {
        if (FixedViewSize != default)
        {
            return FixedViewSize;
        }
        else
        {
            return DisplayManager.GetWindowSizeInPixels();
        }
    }

    public Matrix4x4 GetProjectionMatrix()
    {
        return GetProjectionMatrix(GetViewSize());
    }

    public Matrix4x4 GetProjectionMatrix(Vector2 viewSize)
    {
        Vector2 windowSize = viewSize;

        float left = FocusPosition.X - windowSize.X / 2f;
        float right = FocusPosition.X + windowSize.X / 2f;
        float bottom = FocusPosition.Y + windowSize.Y / 2f;
        float top = FocusPosition.Y - windowSize.Y / 2f;

        Matrix4x4 orthoMatrix = Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, 0.01f, 100);
        Matrix4x4 zoomMatrix = Matrix4x4.CreateScale(Zoom);

        return orthoMatrix * zoomMatrix;
    }
}
