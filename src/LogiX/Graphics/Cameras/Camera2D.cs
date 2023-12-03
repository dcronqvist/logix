using System.Drawing;
using System.Numerics;

namespace LogiX.Graphics.Cameras;

public class Camera2D : ICamera2D
{
    private readonly IProvider<Vector2> _viewSizeProvider;
    private readonly IProvider<Vector2> _focusPositionProvider;
    private readonly IProvider<float> _zoomProvider;

    public RectangleF VisibleArea
    {
        get
        {
            Vector2 viewSize = _viewSizeProvider.Get();
            Vector2 focus = _focusPositionProvider.Get();
            float zoom = _zoomProvider.Get();

            float left = focus.X - viewSize.X / 2f / zoom;
            float top = focus.Y - viewSize.Y / 2f / zoom;
            float height = viewSize.Y / zoom;
            float width = viewSize.X / zoom;

            return new RectangleF(left, top, width, height);
        }
    }

    public Vector2 TopLeft
    {
        get
        {
            Vector2 viewSize = _viewSizeProvider.Get();
            float zoom = _zoomProvider.Get();

            float left = _focusPositionProvider.Get().X - viewSize.X / 2f / zoom;
            float top = _focusPositionProvider.Get().Y - viewSize.Y / 2f / zoom;

            return new Vector2(left, top);
        }
    }

    public Vector2 FocusPosition => _focusPositionProvider.Get();

    public Camera2D(
        IProvider<Vector2> viewSizeProvider,
        IProvider<Vector2> focusPositionProvider,
        IProvider<float> zoomProvider)
    {
        _viewSizeProvider = viewSizeProvider;
        _focusPositionProvider = focusPositionProvider;
        _zoomProvider = zoomProvider;
    }

    public Matrix4x4 GetProjectionMatrix()
    {
        Vector2 viewSize = _viewSizeProvider.Get();
        Vector2 focus = _focusPositionProvider.Get();
        float zoom = _zoomProvider.Get();

        float left = focus.X - viewSize.X / 2f;
        float right = focus.X + viewSize.X / 2f;
        float bottom = focus.Y + viewSize.Y / 2f;
        float top = focus.Y - viewSize.Y / 2f;

        Matrix4x4 orthoMatrix = Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, 0.01f, 100);
        Matrix4x4 zoomMatrix = Matrix4x4.CreateScale(zoom);

        return orthoMatrix * zoomMatrix;
    }
}
