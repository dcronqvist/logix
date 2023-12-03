using System.Collections;
using System.Numerics;
using DotGLFW;
using LogiX.Extensions;
using LogiX.Graphics;
using LogiX.UserInterface.Coroutines;

namespace LogiX.UserInterface.Views.EditorView;

public partial class EditorView
{
    private IEnumerator RectangleSelection()
    {
        var mousePosStart = (_mouse.GetMousePositionInWindowAsVector2() / _cameraZoom) + new Vector2(_camera.VisibleArea.Left, _camera.VisibleArea.Top);

        yield return CoroutineHelpers.Render((renderer, _, _) =>
        {
            var mousePosEnd = (_mouse.GetMousePositionInWindowAsVector2() / _cameraZoom) + new Vector2(_camera.VisibleArea.Left, _camera.VisibleArea.Top);
            var rectangle = RectangleFExtensions.CreateRectangleFromCornerPoints(mousePosStart, mousePosEnd);

            renderer.Primitives.RenderRectangle(rectangle, Vector2.Zero, 0f, ColorF.LightBlue * 0.3f);
        });

        yield return CoroutineHelpers.WaitUntil(() => _mouse.IsMouseButtonReleased(MouseButton.Left), () =>
        {
            var mousePosEnd = (_mouse.GetMousePositionInWindowAsVector2() / _cameraZoom) + new Vector2(_camera.VisibleArea.Left, _camera.VisibleArea.Top);
            var rectangle = RectangleFExtensions.CreateRectangleFromCornerPoints(mousePosStart, mousePosEnd);

            _currentlySimulatedCircuitViewModel.SelectAllNodesThatIntersectRectangle(rectangle);
            _currentlySimulatedCircuitViewModel.SelectAllSignalSegmentsThatIntersectRectangle(rectangle);
        });

        yield break;
    }
}
