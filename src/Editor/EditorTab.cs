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
    public Circuit LiveCircuit { get; set; }

    public IntegratedComponent? ic;

    public EditorTab(Circuit circuit)
    {
        this.Name = circuit.Name;
        this.FSM = new EditorFSM();
        this.CameraTarget = Vector2.Zero;
        this.CameraZoom = 1f;
        this.LastSavedCommandIndex = this.CurrentCommandIndex;
        this.LiveCircuit = circuit.Clone();
        this.Circuit = circuit;
        this.Simulator = circuit.GetSimulatorForCircuit();
    }

    public void OnEnter(Editor editor)
    {
        List<CircuitDependency> dependencies = this.LiveCircuit.GetDependencyCircuits();

        foreach (EditorTab tab in editor.EditorTabs.Values)
        {
            // Check if tab's circuit is in our dependencies
            CircuitDependency? dependency = dependencies.FirstOrDefault(d => d.CircuitID == tab.Circuit.UniqueID, null);

            if (dependency is not null)
            {
                // IF so, check if that circuit is up to date
                if (!tab.Circuit.MatchesDependency(dependency))
                {
                    // IF not, update the circuit
                    List<IntegratedComponent> comps = this.Simulator.GetComponents<IntegratedComponent>(ic => ic.Circuit.UniqueID == dependency.CircuitID);
                    comps.ForEach(ic => ic.UpdateCircuit(tab.Circuit));

                    this.LiveCircuit.UpdateDependency(dependency, tab.Circuit);
                }
            }
        }
    }

    public void Save()
    {
        this.LastSavedCommandIndex = this.CurrentCommandIndex;
        this.Circuit.Update(this.Simulator.AllComponents, this.Simulator.AllWires);
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

        this.LiveCircuit.Update(this.Simulator.AllComponents, this.Simulator.AllWires);
    }

    public void SubmitUI(Editor editor)
    {
        this.FSM.SubmitUI(editor);

        if (this.Simulator.Selection.Count == 1 && this.Simulator.Selection[0] is Component comp)
        {
            comp.SubmitUIPropertyWindow(editor);
        }

        ImGui.Begin("Dependencies");

        ImGui.Text("This circuit has ID: " + this.Circuit.UniqueID + "\nUpdate: " + this.Circuit.UpdateID);
        ImGui.Separator();

        ImGui.Text("Deps:");

        List<CircuitDependency> dependencies = this.Circuit.GetDependencyCircuits();
        foreach (CircuitDependency dep in dependencies)
        {
            ImGui.Text(dep.CircuitID);
            ImGui.Text(dep.CircuitUpdateID);
            ImGui.Separator();
        }

        ImGui.Text("Live Deps:");

        dependencies = this.LiveCircuit.GetDependencyCircuits();
        foreach (CircuitDependency dep in dependencies)
        {
            ImGui.Text(dep.CircuitID);
            ImGui.Text(dep.CircuitUpdateID);
            ImGui.Separator();
        }

        ImGui.End();
    }

    public void Render(Editor editor)
    {
        this.Simulator.Render();
        this.FSM.Render(editor);
    }
}