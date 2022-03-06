using LogiX.Editor.StateMachine;

namespace LogiX.Editor;

public class EditorTab
{
    public Vector2 CameraTarget { get; set; }
    public float CameraZoom { get; set; }
    public string Name { get; set; }
    public Simulator Simulator { get; set; }
    public EditorFSM FSM { get; set; }

    public EditorTab(string name)
    {
        this.Name = name;
        this.Simulator = new Simulator();
        this.FSM = new EditorFSM();
        this.CameraTarget = Vector2.Zero;
        this.CameraZoom = 1f;
    }

    public void MoveCamera(Vector2 delta)
    {
        CameraTarget += delta;
    }

    public void ZoomCamera(float zoomFactor)
    {
        CameraZoom = this.CameraZoom * zoomFactor;
    }

    public void Update(Editor editor)
    {
        if (Raylib.IsKeyDown(KeyboardKey.KEY_SPACE) || Raylib.IsMouseButtonDown(MouseButton.MOUSE_MIDDLE_BUTTON))
        {
            this.MoveCamera(-UserInput.GetMouseDelta(editor.camera));
        }

        float zoomSpeed = 1.15f;
        if (Raylib.GetMouseWheelMove() > 0)
        {
            this.ZoomCamera(zoomSpeed);
        }
        if (Raylib.GetMouseWheelMove() < 0)
        {
            this.ZoomCamera(1f / zoomSpeed);
        }
        editor.camera.zoom = MathF.Min(MathF.Max(editor.camera.zoom, 0.1f), 4f);


        this.Simulator.Interact(editor);
        this.Simulator.PerformLogic();
        this.FSM.Update(editor);
    }

    public void SubmitUI(Editor editor)
    {
        this.FSM.SubmitUI(editor);
    }

    public void Render(Editor editor)
    {
        this.Simulator.Render();
        this.FSM.Render(editor);
    }
}