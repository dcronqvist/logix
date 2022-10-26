using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using LogiX.Architecture.BuiltinComponents;
using LogiX.Architecture.Commands;
using LogiX.Architecture.Serialization;
using LogiX.Architecture.StateMachine;
using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Graphics.UI;
using LogiX.Rendering;
using static LogiX.OpenGL.GL;

namespace LogiX.Architecture;

public class Editor : Invoker<Editor>
{
    public LogiXProject Project { get; private set; }
    public Circuit CurrentlyOpenCircuit { get; private set; }

    public ImGuiController ImGuiController { get; private set; }
    public Framebuffer GUIFramebuffer { get; private set; }
    public Framebuffer WorkspaceFramebuffer { get; private set; }
    public bool WritingNewCircuitName { get; set; }
    public string NewCircuitName { get; set; }

    public Camera2D Camera { get; private set; }
    public ThreadSafe<Simulation> Sim { get; private set; }
    public Task SimTickTask { get; set; }
    private Mutex SimTickMutex { get; set; }
    public float CurrentTicksPerSecond { get; set; }
    public int[] AvailableTickRates { get; } = new int[] { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384 };
    public int CurrentlySelectedTickRate { get; set; } = 8;
    public bool SimulationRunning { get; set; } = true;
    public EditorFSM FSM { get; private set; }

    public bool RequestedPopupModal { get; set; }
    public string RequestedPopupModalTitle { get; set; }
    public Action RequestedPopupModalSubmit { get; set; }

    public Editor()
    {
        this.GUIFramebuffer = new(true);
        this.SimTickMutex = new();
        this.WorkspaceFramebuffer = new(true);
        NewGUI.Init(LogiX.ContentManager.GetContentItem<Font>("content_1.font.default"));

        DisplayManager.LockedGLContext(() =>
        {
            this.ImGuiController = new((int)DisplayManager.GetWindowSizeInPixels().X, (int)DisplayManager.GetWindowSizeInPixels().Y);
        });

        Input.OnScroll += (sender, e) =>
        {
            if (!DisplayManager.IsWindowFocused())
            {
                return; // Early return if the window is not focused.
            }

            if (e > 0)
                Camera.Zoom *= 1.1f;
            else if (e < 0)
                Camera.Zoom /= 1.1f;

            Camera.Zoom = Math.Clamp(Camera.Zoom, 0.3f, 10f);

            this.ImGuiController.MouseScroll(new Vector2(0, e));
        };

        Input.OnChar += (sender, e) =>
        {
            this.ImGuiController.PressChar(e);
        };

        DisplayManager.OnFramebufferResize += (sender, e) =>
        {
            this.ImGuiController.WindowResized((int)e.X, (int)e.Y);
        };

        this.SetProject(LogiXProject.New("Untitled"));

        var thread = new Thread(async () =>
        {
            Stopwatch sw = new();
            sw.Start();

            while (true)
            {
                if (this.Sim is null || !this.SimulationRunning)
                {
                    this.CurrentTicksPerSecond = 0;
                    await Task.Delay(100);
                    continue;
                }

                long start = sw.Elapsed.Ticks;
                this.SimTickMutex.WaitOne();
                this.Sim.LockedAction(s =>
                {
                    s.Tick();
                });

                int targetTps = this.AvailableTickRates[this.CurrentlySelectedTickRate];
                long targetDiff = TimeSpan.TicksPerSecond / targetTps;

                this.SimTickMutex.ReleaseMutex();

                while (sw.Elapsed.Ticks < start + targetDiff)
                {
                    await Task.Delay(TimeSpan.FromTicks(targetDiff / 10));
                }

                long diff = sw.Elapsed.Ticks - start;
                double seconds = diff / (double)TimeSpan.TicksPerSecond;
                this.CurrentTicksPerSecond = this.CurrentTicksPerSecond + (1f / (float)seconds - this.CurrentTicksPerSecond) * (0.8f / MathF.Sqrt(targetTps));
            }
        });

        thread.Start();
    }

    public void OpenPopup(string title, Action submit)
    {
        this.RequestedPopupModal = true;
        this.RequestedPopupModalTitle = title;
        this.RequestedPopupModalSubmit = submit;
    }

    public void SetProject(LogiXProject project)
    {
        ComponentDescription.CurrentProject = project;
        this.Project = project;
        DisplayManager.SetWindowTitle($"LogiX - {this.Project.Name}");
        this.Camera = new Camera2D(Vector2.Zero, 1f);
        DisplayManager.SetWindowTitle($"LogiX - {this.Project.Name}");
        this.FSM = new EditorFSM();
        this.Commands.Clear();
        this.CurrentCommandIndex = -1;

        if (project.LastOpenedCircuit != Guid.Empty)
        {
            this.OpenCircuit(project.LastOpenedCircuit);
        }

    }

    public void OpenCircuit(Guid id)
    {
        if (this.CurrentlyOpenCircuit is not null)
        {
            var newCircuit = this.Sim.LockedAction(s => s.GetCircuitInSimulation(this.CurrentlyOpenCircuit.Name));
            newCircuit.ID = this.CurrentlyOpenCircuit.ID;
            this.Project.UpdateCircuit(newCircuit);
        }

        var circuit = this.Project.GetCircuit(id);
        this.CurrentlyOpenCircuit = circuit;
        this.Camera = new Camera2D(Vector2.Zero, 1f);

        this.Sim = new(Simulation.FromCircuit(circuit));
        this.Project.SetLastOpenedCircuit(id);
    }

    public void AddNewComponent(ComponentDescription comp, Vector2i position)
    {
        var command = new CAddComponent(comp.CreateComponent(), position);
        this.Execute(command, this);
        this.FSM.SetState<StateMovingSelection>(this, 0);
    }

    public void Update()
    {
        //this.Sim?.LockedAction(s => s.Tick());

        if (this.Sim is not null)
        {
            this.FSM?.Update(this);
        }
    }

    public void DrawGrid()
    {
        // Get camera's position and the size of the current view
        // This depends on the zoom of the camera. Dividing by the
        // zoom gives a correct representation of the actual visible view.
        Vector2 viewSize = DisplayManager.GetViewSize(this.Camera);
        Vector2 camPos = this.Camera.FocusPosition;

        int pixelsInBetweenLines = Constants.GRIDSIZE;

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

            PrimitiveRenderer.RenderLine(pShader, new Vector2(lineXstart, lineY - 0.5f), new Vector2(lineXend, lineY - 0.5f), 1, color, Camera);
        }
    }

    public void Render()
    {
        this.ImGuiController.Update(GameTime.DeltaTime);
        var fShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.fb_default");

        if (this.CurrentlyOpenCircuit is not null)
        {
            this.WorkspaceFramebuffer.Bind(() =>
            {
                Framebuffer.Clear(ColorF.Darken(ColorF.LightGray, 0.9f));
                this.DrawGrid();
                this.Sim.LockedAction(s => s.Render(this.Camera));
                this.FSM.Render(this);
            });

            this.GUIFramebuffer.Bind(() =>
            {
                Framebuffer.Clear(ColorF.Transparent);
                this.SubmitGUI();
                this.FSM?.SubmitUI(this);
                this.ImGuiController.Render();
            });

            glEnable(GL_BLEND);
            glBlendFunc(GL_ONE, GL_ONE_MINUS_SRC_ALPHA);
            Framebuffer.RenderFrameBufferToScreen(fShader, this.WorkspaceFramebuffer);
            Framebuffer.RenderFrameBufferToScreen(fShader, this.GUIFramebuffer);
        }
        else
        {
            this.GUIFramebuffer.Bind(() =>
            {
                Framebuffer.Clear(ColorF.Transparent);
                this.SubmitGUI();
                this.ImGuiController.Render();
            });

            Framebuffer.BindDefaultFramebuffer();
            var tShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.text");
            var font = LogiX.ContentManager.GetContentItem<Font>("content_1.font.default");
            var windowSize = DisplayManager.GetWindowSizeInPixels();
            string text = "No circuit open";
            float scale = 3f;
            var textMeasure = font.MeasureString(text, scale);

            TextRenderer.RenderText(tShader, font, text, windowSize / 2f - textMeasure / 2f, scale, ColorF.White, Framebuffer.GetDefaultCamera());
            Framebuffer.RenderFrameBufferToScreen(fShader, this.GUIFramebuffer);
        }
    }

    public void SubmitGUI()
    {
        ImGui.BeginMainMenuBar();

        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem("New Project...", "Ctrl+N", false, true))
            {

            }
            if (ImGui.MenuItem("Save Project", "Ctrl+S", false, true))
            {
                var newCircuit = this.Sim.LockedAction(s => s.GetCircuitInSimulation(this.CurrentlyOpenCircuit.Name));
                newCircuit.ID = this.CurrentlyOpenCircuit.ID;
                this.Project.UpdateCircuit(newCircuit);
                this.Project.SaveProjectToFile("test.lxprojj");
            }
            if (ImGui.MenuItem("Open Project...", "Ctrl+O", false, true))
            {
                var project = LogiXProject.FromFile("test.lxprojj");
                this.SetProject(project);
            }

            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Edit"))
        {
            if (ImGui.MenuItem("Undo", "Ctrl+Z", false, this.CurrentCommandIndex >= 0))
            {
                this.Undo(this);
            }
            if (ImGui.MenuItem("Redo", "Ctrl+Y", false, this.CurrentCommandIndex < this.Commands.Count - 1))
            {
                this.Redo(this);
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Circuits"))
        {
            if (ImGui.MenuItem("New Circuit...", "Ctrl+Shift+N", false, true))
            {
                this.WritingNewCircuitName = true;
                this.NewCircuitName = "";
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Simulation"))
        {
            var running = this.SimulationRunning;
            ImGui.MenuItem("Simulating", "", ref running);
            this.SimulationRunning = running;

            if (ImGui.BeginMenu("Tick Rate"))
            {
                Utilities.ImGuiHelp("High tick rates can cause severe performance issues. Adjust with caution.");
                for (int i = 0; i < this.AvailableTickRates.Length; i++)
                {
                    if (ImGui.Selectable(this.AvailableTickRates[i].GetAsHertzString(), this.CurrentlySelectedTickRate == i))
                    {
                        this.CurrentlySelectedTickRate = i;
                    }
                }

                ImGui.EndMenu();
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Tick Once", "", false, !this.SimulationRunning))
            {
                this.Sim.LockedAction(s => s.Tick());
            }

            ImGui.EndMenu();
        }

        ImGui.Text($"TPS: {this.CurrentTicksPerSecond.GetAsHertzString()}");

        ImGui.EndMainMenuBar();

        ImGui.SetNextWindowPos(new Vector2(0, 18));
        ImGui.SetNextWindowSize(new Vector2(140, DisplayManager.GetWindowSizeInPixels().Y - 18));
        if (ImGui.Begin("Components", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove))
        {
            if (ImGui.CollapsingHeader("Project"))
            {
                ImGui.TreePush();
                // First your own circuits in the project you have loaded.
                var circuits = this.Project.Circuits;

                foreach (var circuit in circuits)
                {
                    ImGui.Selectable(circuit.Name, circuit.ID == CurrentlyOpenCircuit.ID);
                    if (ImGui.IsItemClicked() && circuit.ID != CurrentlyOpenCircuit.ID)
                    {
                        // Cannot put a circuit inside itself
                        this.AddNewComponent(Integrated.CreateDescriptionFromCircuit(circuit.Name, circuit), Input.GetMousePosition(this.Camera).ToVector2i(Constants.GRIDSIZE));
                    }
                    if (ImGui.BeginPopupContextItem())
                    {
                        if (ImGui.MenuItem("Edit", this.CurrentlyOpenCircuit is null ? true : this.CurrentlyOpenCircuit.ID != circuit.ID))
                        {
                            this.OpenCircuit(circuit.ID);
                        }
                        if (ImGui.MenuItem("Delete", this.CurrentlyOpenCircuit is null ? true : this.CurrentlyOpenCircuit.ID != circuit.ID))
                        {
                            this.OpenPopup("Delete Circuit", () =>
                            {
                                ImGui.Text($"Are you sure you want to delete circuit '{circuit.Name}'?");
                                if (ImGui.Button("Yes"))
                                {
                                    this.Project.RemoveCircuit(circuit.ID);
                                    this.OpenCircuit(this.CurrentlyOpenCircuit.ID);
                                    ImGui.CloseCurrentPopup();
                                }
                                ImGui.SameLine();
                                if (ImGui.Button("No"))
                                {
                                    ImGui.CloseCurrentPopup();
                                }
                            });
                        }

                        ImGui.EndPopup();
                    }
                }

                if (this.WritingNewCircuitName)
                {
                    if (Input.IsKeyPressed(Keys.Escape))
                    {
                        this.WritingNewCircuitName = false;
                    }

                    ImGui.SetKeyboardFocusHere();
                    string s = this.NewCircuitName;
                    if (ImGui.InputTextWithHint("##CircuitNAME", "name...", ref s, 15, ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        this.WritingNewCircuitName = false;
                        this.Project.AddCircuit(new Circuit(s));
                    }
                    this.NewCircuitName = s;
                }

                ImGui.TreePop();
            }

            var componentCategories = ComponentDescription.GetAllComponentCategories();

            foreach (var category in componentCategories)
            {
                if (ImGui.CollapsingHeader(category.Key.ToString()))
                {
                    ImGui.TreePush(category.Key.ToString());
                    foreach (var c in category.Value)
                    {
                        var cInfo = ComponentDescription.GetComponentInfo(c);
                        ImGui.MenuItem(cInfo.DisplayName);

                        if (ImGui.IsItemClicked())
                        {
                            this.AddNewComponent(ComponentDescription.CreateDefaultComponentDescription(c), Input.GetMousePosition(this.Camera).ToVector2i(Constants.GRIDSIZE));
                        }
                    }
                    ImGui.TreePop();
                }
            }

            ImGui.End();

            if (this.Sim?.LockedAction(s => s.SelectedComponents.Count == 1) == true)
            {
                var selected = this.Sim.LockedAction(s => s.SelectedComponents.First());
                if (selected.ShowPropertyWindow && ImGui.Begin($"Component Properties", ImGuiWindowFlags.AlwaysAutoResize))
                {
                    selected.SubmitUISelected(this.Sim.LockedAction(s => s.Components.IndexOf(selected)));
                }
                ImGui.End();
            }


            if (this.RequestedPopupModal)
            {
                ImGui.OpenPopup(this.RequestedPopupModalTitle);

                if (ImGui.BeginPopupModal(this.RequestedPopupModalTitle))
                {
                    this.RequestedPopupModalSubmit.Invoke();
                    ImGui.EndPopup();
                }

                if (!ImGui.IsPopupOpen(this.RequestedPopupModalTitle))
                {
                    this.RequestedPopupModal = false;
                }
            }
        }
    }
}