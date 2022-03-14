using System.Diagnostics;
using LogiX.Components;
using LogiX.Editor.Commands;
using LogiX.Editor.StateMachine;
using LogiX.SaveSystem;

namespace LogiX.Editor;

public class EditorTab : Invoker<Editor>
{
    public Vector2 CameraTarget { get; set; }
    public float CameraZoom { get; set; }
    public string Name { get; set; }
    public Simulator Simulator { get; set; }
    public EditorFSM FSM { get; set; }
    public int LastSavedCommandIndex { get; set; }
    public Circuit Circuit { get; set; }

    public IntegratedComponent? ic;

    public EditorTab(Circuit circuit)
    {
        this.Name = circuit.Name;
        this.Simulator = new Simulator();
        this.FSM = new EditorFSM();
        this.CameraTarget = Vector2.Zero;
        this.CameraZoom = 1f;
        this.LastSavedCommandIndex = this.CurrentCommandIndex;
        this.Circuit = circuit;
    }

    public void OnEnter(Editor editor)
    {

    }

    public void Save()
    {
        this.LastSavedCommandIndex = this.CurrentCommandIndex;
        this.Circuit.Update(this.Simulator);
    }

    public bool HasChanges()
    {
        return this.LastSavedCommandIndex != this.CurrentCommandIndex;
    }

    public bool TryClose()
    {
        return !this.HasChanges();
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

        this.FSM.Update(editor);
        this.Simulator.Interact(editor);
        this.Simulator.PerformLogic();
    }

    public void SubmitUI(Editor editor)
    {
        this.FSM.SubmitUI(editor);

        if (this.Simulator.Selection.Count == 1 && this.Simulator.Selection[0] is Component comp)
        {
            comp.SubmitUIPropertyWindow();
        }

        ImGui.Begin("Dependencies");

        List<string> dependencies = this.Circuit.GetDependencyCircuits();
        foreach (string dep in dependencies)
        {
            ImGui.Text(dep);
        }

        ImGui.End();
    }

    public void Render(Editor editor)
    {
        this.Simulator.Render();
        this.FSM.Render(editor);
    }
}