using System.Numerics;
using LogiX.Architecture.Commands;
using LogiX.Architecture.Serialization;
using LogiX.Architecture.StateMachine;
using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Graphics.UI;
using LogiX.Rendering;

namespace LogiX.Architecture;

public class EditorTab : Invoker<EditorTab>
{
    public Camera2D Camera { get; set; }
    public ThreadSafe<Simulation> Sim { get; private set; }
    public Circuit SavedCircuit { get; private set; }
    public Framebuffer WorkspaceFramebuffer { get; private set; }
    public Framebuffer GUIFramebuffer { get; private set; }
    public TabFSM FSM { get; private set; }
    public Action ContextMenuAction { get; set; }
    public bool ContextMenuOpen { get; set; }

    public EditorTab(string tabName, Circuit initialCircuit)
    {
        this.Camera = new Camera2D(Vector2.Zero, 1f);
        this.SavedCircuit = initialCircuit;
        this.Sim = new(Simulation.FromCircuit(initialCircuit));
        this.WorkspaceFramebuffer = new Framebuffer();
        this.GUIFramebuffer = new Framebuffer();
        this.FSM = new TabFSM();

        Input.OnScroll += (sender, e) =>
        {
            // TODO: Only perform if the tab is focused.

            if (e > 0)
                Camera.Zoom *= 1.1f;
            else if (e < 0)
                Camera.Zoom /= 1.1f;

            Camera.Zoom = Math.Clamp(Camera.Zoom, 0.3f, 10f);
        };
    }

    public EditorTab(string tabName)
    {
        this.Camera = new Camera2D(Vector2.Zero, 1f);
        this.SavedCircuit = new Circuit();
        this.Sim = new(new());
        this.WorkspaceFramebuffer = new Framebuffer();
        this.GUIFramebuffer = new Framebuffer();
        this.FSM = new TabFSM();

        Input.OnScroll += (sender, e) =>
        {
            // TODO: Only perform if the tab is focused.

            if (e > 0)
                Camera.Zoom *= 1.1f;
            else if (e < 0)
                Camera.Zoom /= 1.1f;

            Camera.Zoom = Math.Clamp(Camera.Zoom, 0.3f, 10f);
        };

        NewGUI.Init(LogiX.ContentManager.GetContentItem<Font>("content_1.font.default"));
    }

    public void DrawGrid()
    {
        // Get camera's position and the size of the current view
        // This depends on the zoom of the camera. Dividing by the
        // zoom gives a correct representation of the actual visible view.
        Vector2 viewSize = DisplayManager.GetViewSize(this.Camera);
        Vector2 camPos = this.Camera.FocusPosition;

        int pixelsInBetweenLines = 16;

        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");
        var color = ColorF.Darken(ColorF.LightGray, 0.8f);

        // Draw vertical lines
        for (int i = (int)((camPos.X - viewSize.X / 2.0F) / pixelsInBetweenLines); i < ((camPos.X + viewSize.X / 2.0F) / pixelsInBetweenLines); i++)
        {
            int lineX = i * pixelsInBetweenLines;
            int lineYstart = (int)(camPos.Y - viewSize.Y / 2.0F);
            int lineYend = (int)(camPos.Y + viewSize.Y / 2.0F);

            PrimitiveRenderer.RenderLine(pShader, new Vector2(lineX, lineYstart), new Vector2(lineX, lineYend), 1, color, Camera);
        }

        // Draw horizontal lines
        for (int i = (int)((camPos.Y - viewSize.Y / 2.0F) / pixelsInBetweenLines); i < ((camPos.Y + viewSize.Y / 2.0F) / pixelsInBetweenLines); i++)
        {
            int lineY = i * pixelsInBetweenLines;
            int lineXstart = (int)(camPos.X - viewSize.X / 2.0F);
            int lineXend = (int)(camPos.X + viewSize.X / 2.0F);

            PrimitiveRenderer.RenderLine(pShader, new Vector2(lineXstart, lineY), new Vector2(lineXend, lineY), 1, color, Camera);
        }
    }

    public void Update()
    {
        this.FSM.Update(this);
        this.Sim.LockedAction(s => s.Tick());
    }

    public void Render()
    {
        var fShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.fb_default");
        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");
        var mousePos = Input.GetMousePosition(this.Camera).ToVector2i(16);

        // Render workspace to its own framebuffer, then render that framebuffer to the screen
        this.WorkspaceFramebuffer.Bind(() =>
        {
            Framebuffer.Clear(ColorF.Darken(ColorF.LightGray, 0.9f));

            this.DrawGrid();
            this.Sim.LockedAction(s => s.Render(this.Camera));
            this.FSM.Render(this);

            if (_currentlySelectedComponent is not null)
            {
                _currentlySelectedComponent.Position = mousePos;
                _currentlySelectedComponent.Render(this.Camera);
            }
        });

        this.GUIFramebuffer.Bind(() =>
        {
            NewGUI.Begin();
            SubmitGUI();

            this.Sim.LockedAction(s =>
            {
                if (s.SelectedComponents.Count == 1)
                {
                    var c = s.SelectedComponents.First();

                    if (c.ShowPropertyWindow)
                    {
                        if (NewGUI.BeginWindow("Component Properties", new Vector2(100, 100)))
                        {
                            c.SubmitUISelected();
                        }
                        NewGUI.EndWindow();
                    }
                }
            });

            NewGUI.End();

            string s = $"ANY_WIN_HOV: {NewGUI.AnyWindowHovered()}, HOVERED: {(NewGUI.HoveredEnvironment == null ? "null" : NewGUI.HoveredEnvironment.GetID())}, HOT: {NewGUI.HotID ?? "null"}, ACTIVE: {NewGUI.ActiveID ?? "null"}";
            DisplayManager.SetWindowTitle(s);
        });

        Framebuffer.RenderFrameBufferToScreen(fShader, this.WorkspaceFramebuffer);
        Framebuffer.RenderFrameBufferToScreen(fShader, this.GUIFramebuffer);
    }

    public void OpenContextMenu(Action action)
    {
        this.ContextMenuOpen = true;
        this.ContextMenuAction = action;
    }

    public void AddNewComponent(ComponentDescription comp, Vector2i position)
    {
        var command = new CAddComponent(comp.CreateComponent(), position);
        this.Execute(command, this);
        this.FSM.SetState<StateMovingSelection>(this, 0);
    }

    private Component _currentlySelectedComponent = null;

    bool open = false;
    string s = "";

    public void SubmitGUI()
    {
        NewGUI.BeginMainMenuBar();

        if (NewGUI.BeginMenu("File"))
        {
            if (NewGUI.MenuItem("New..."))
            {

            }
        }
        NewGUI.EndMenu();

        NewGUI.Spacer(5);

        if (NewGUI.BeginMenu("Edit"))
        {
            if (NewGUI.MenuItem("Undo"))
            {
                this.Undo(this);
            }
            NewGUI.Spacer(5);
            if (NewGUI.MenuItem("Redo"))
            {
                this.Redo(this);
            }
        }
        NewGUI.EndMenu();

        var stateName = this.FSM.CurrentState.GetType().Name;
        NewGUI.Label($"STATE: {stateName}");

        var amountOfWires = this.Sim.LockedAction(s => s.Wires.Count);
        NewGUI.Label($"WIRES: {amountOfWires}");

        if (NewGUI.Button("Save"))
        {
            this.Sim.LockedAction(s =>
            {
                s.GetCircuitInSimulation().SaveToFile("test_circuit.json");
            });
        }

        NewGUI.Spacer(5);

        if (NewGUI.Button("Load"))
        {
            this.Sim.LockedAction(s =>
            {
                s.SetCircuitInSimulation(Circuit.LoadFromFile("test_circuit.json"));
            });
        }

        NewGUI.EndMainMenuBar();

        bool open = this.ContextMenuOpen;
        if (NewGUI.BeginContextMenu(ref open))
        {
            this.ContextMenuAction();
            NewGUI.EndContextMenu();
        }
        this.ContextMenuOpen = open;

        if (NewGUI.BeginWindow("Components", new Vector2(5, 28)))
        {
            NewGUI.Spacer(5);

            NewGUI.InputText("Test", ref s);
            NewGUI.Spacer(5);

            var types = ComponentDescription.GetRegisteredComponentTypes();
            foreach (var type in types)
            {
                NewGUI.PushNextItemSize(new Vector2(100, 20));
                if (NewGUI.Button(type.Split(".").Last()))
                {
                    // TODO: Add component to simulation
                    //_currentlySelectedComponent = ComponentDescription.CreateDefaultComponent(type);
                }
                if (NewGUI.IsPreviousItemActive() && this.FSM.CurrentState is StateIdle)
                {
                    this.AddNewComponent(ComponentDescription.CreateDefaultComponentDescription(type), Input.GetMousePosition(this.Camera).ToVector2i(16));
                }
                NewGUI.Spacer(5);
            }

            // NewGUI.Spacer(-5);

            if (NewGUI.Expandable("expand me", ref open))
            {
                NewGUI.Spacer(5);
                NewGUI.Label("");
                NewGUI.SameLine();
                NewGUI.Button("In expanded");
                NewGUI.Spacer(5);
                NewGUI.Label("");
                NewGUI.SameLine();
                NewGUI.Button("In expanded 2");
            }
        }
        NewGUI.EndWindow();

    }

    public void PerformSimulationTick()
    {
        this.Sim.LockedAction(s =>
        {
            s.Tick();
        });
    }
}