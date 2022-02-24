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