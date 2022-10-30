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
    public float CurrentTicksPerSecond { get; set; }
    public int[] AvailableTickRates { get; } = new int[] { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384 };
    public int CurrentlySelectedTickRate { get; set; } = 8;
    public bool SimulationRunning { get; set; } = true;
    public EditorFSM FSM { get; private set; }

    public bool RequestedPopupModal { get; set; }
    public string RequestedPopupModalTitle { get; set; }
    public Action RequestedPopupModalSubmit { get; set; }

    public bool RequestedPopupContext { get; set; }
    public Action RequestedPopupContextSubmit { get; set; }
    public Vector2 MouseStartPosition { get; set; }

    public Editor()
    {
        this.GUIFramebuffer = new(true);
        this.WorkspaceFramebuffer = new(true);

        DisplayManager.LockedGLContext(() =>
        {
            this.ImGuiController = new((int)DisplayManager.GetWindowSizeInPixels().X, (int)DisplayManager.GetWindowSizeInPixels().Y, LogiX.ContentManager.GetContentItem<Font>("content_1.font.opensans"));
        });

        Input.OnScroll += (sender, e) =>
        {
            if (!DisplayManager.IsWindowFocused() || ImGui.GetIO().WantCaptureMouse)
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

        // INITIALIZE PROJECT AND STUFF
        var lastOpenProject = Settings.GetSetting<string>(Settings.LAST_OPEN_PROJECT);

        if (lastOpenProject is null || lastOpenProject.Length == 0)
        {
            // No project was open last time, so we assume an empty one.
            this.SetProject(LogiXProject.New("Untitled"));
        }
        else
        {
            // Load the last open project
            this.SetProject(LogiXProject.FromFile(lastOpenProject));
        }

        var thread = new Thread(async () =>
        {
            Stopwatch sw = new();
            sw.Start();

            while (true)
            {
                long start = sw.Elapsed.Ticks;
                if (this.Sim is null || !this.SimulationRunning)
                {
                    this.CurrentTicksPerSecond = 0;
                    await Task.Delay(100);
                    continue;
                }

                this.Sim.LockedAction(s =>
                {
                    try
                    {
                        s.Tick();
                    }
                    catch (System.Exception ex)
                    {
                        this.OpenErrorPopup("Simulation Error", true, () =>
                        {
                            ImGui.Text(ex.Message);
                            if (ImGui.Button("OK"))
                            {
                                ImGui.CloseCurrentPopup();
                            }
                        });
                    }
                });

                int targetTps = this.AvailableTickRates[this.CurrentlySelectedTickRate];
                long targetDiff = TimeSpan.TicksPerSecond / targetTps;
                while (sw.Elapsed.Ticks < start + targetDiff)
                {
                    await Task.Delay(TimeSpan.FromTicks(targetDiff / 10));
                }

                long diff = sw.Elapsed.Ticks - start;
                double seconds = diff / (double)TimeSpan.TicksPerSecond;
                this.CurrentTicksPerSecond = this.CurrentTicksPerSecond + (1f / (float)seconds - this.CurrentTicksPerSecond) * (0.8f / MathF.Sqrt(targetTps));
            }
        });

        thread.IsBackground = true;
        thread.Priority = ThreadPriority.Highest;
        thread.Start();
    }

    public void OpenPopup(string title, Action submit)
    {
        this.RequestedPopupModal = true;
        this.RequestedPopupModalTitle = title;
        this.RequestedPopupModalSubmit = submit;
    }

    public void OpenContextMenu(Action submit)
    {
        this.RequestedPopupContext = true;
        this.RequestedPopupContextSubmit = submit;
        this.MouseStartPosition = Input.GetMousePositionInWindow();
    }

    public void OpenErrorPopup(string title, bool stopSim, Action submit)
    {
        this.RequestedPopupModal = true;
        this.RequestedPopupModalTitle = title;
        this.RequestedPopupModalSubmit = submit;

        this.SimulationRunning = !stopSim;
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

    public void OpenCircuit(Guid id, bool save = true)
    {
        if (this.CurrentlyOpenCircuit is not null && save)
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

            PrimitiveRenderer.RenderLine(new Vector2(lineX, lineYstart), new Vector2(lineX, lineYend), 1, color);
        }

        // Draw horizontal lines
        for (int i = (int)((camPos.Y - viewSize.Y / 2.0F) / pixelsInBetweenLines); i < ((camPos.Y + viewSize.Y / 2.0F) / pixelsInBetweenLines); i++)
        {
            int lineY = i * pixelsInBetweenLines;
            int lineXstart = (int)(camPos.X - viewSize.X / 2.0F);
            int lineXend = (int)(camPos.X + viewSize.X / 2.0F);

            PrimitiveRenderer.RenderLine(new Vector2(lineXstart, lineY - 0.5f), new Vector2(lineXend, lineY - 0.5f), 1, color);
        }
    }

    public void Render()
    {
        this.ImGuiController.Update(GameTime.DeltaTime);
        var fShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.fb_default");
        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");

        if (this.CurrentlyOpenCircuit is not null)
        {
            this.WorkspaceFramebuffer.Bind(() =>
            {
                Framebuffer.Clear(ColorF.Darken(ColorF.LightGray, 0.9f));
                this.DrawGrid();
                this.Sim.LockedAction(s => s.Render(this.Camera));
                this.FSM.Render(this);
                PrimitiveRenderer.FinalizeRender(pShader, this.Camera);
                TextRenderer.FinalizeRender();
            });

            this.GUIFramebuffer.Bind(() =>
            {
                Framebuffer.Clear(ColorF.Transparent);
                ImGui.PushFont(ImGui.GetIO().Fonts.Fonts[1]);
                try
                {
                    this.SubmitGUI();
                }
                catch (Exception e)
                {
                    this.OpenErrorPopup("Error", true, () =>
                    {
                        ImGui.Text(e.Message);
                        if (ImGui.Button("OK"))
                        {
                            ImGui.CloseCurrentPopup();
                        }
                    });
                }
                this.FSM?.SubmitUI(this);
                ImGui.PopFont();
                this.ImGuiController.Render();
            });

            glEnable(GL_BLEND);
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
            Framebuffer.RenderFrameBufferToScreen(fShader, this.WorkspaceFramebuffer);
            Framebuffer.RenderFrameBufferToScreen(fShader, this.GUIFramebuffer);
        }
        else
        {
            this.GUIFramebuffer.Bind(() =>
            {
                Framebuffer.Clear(ColorF.Transparent);
                ImGui.PushFont(ImGui.GetIO().Fonts.Fonts[1]);
                try
                {
                    this.SubmitGUI();
                }
                catch (Exception e)
                {
                    this.OpenErrorPopup("Error", true, () =>
                    {
                        ImGui.Text(e.Message);
                        if (ImGui.Button("OK"))
                        {
                            ImGui.CloseCurrentPopup();
                        }
                    });
                }
                ImGui.PopFont();
                this.ImGuiController.Render();
            });

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
                Settings.SetSetting(Settings.LAST_OPEN_PROJECT, project.LoadedFromPath);
            }
            if (ImGui.MenuItem("Intentional Exception", "", false, true))
            {
                throw new Exception("Intentional Exception");
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
            if (ImGui.MenuItem("Delete Selection", "", false, this.Sim.LockedAction(s => s.SelectedComponents.Count > 0)))
            {
                var commands = this.Sim.LockedAction(s => s.SelectedComponents.Select(c => new CDeleteComponent(c)));
                this.Execute(new CMulti(commands.ToArray()), this);
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
        ImGui.Text($"State: {this.FSM.CurrentState.GetType().Name}");

        var mainMenuBarSize = ImGui.GetWindowSize();
        ImGui.EndMainMenuBar();

        ImGui.SetNextWindowPos(new Vector2(0, mainMenuBarSize.Y));
        ImGui.SetNextWindowSize(new Vector2(180, DisplayManager.GetWindowSizeInPixels().Y - mainMenuBarSize.Y));
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
                                    this.OpenCircuit(this.CurrentlyOpenCircuit.ID, false);
                                    ImGui.CloseCurrentPopup();
                                }
                                ImGui.SameLine();
                                if (ImGui.Button("No"))
                                {
                                    ImGui.CloseCurrentPopup();
                                }
                            });
                        }
                        if (ImGui.MenuItem("Rename"))
                        {
                            this.NewCircuitName = "";
                            this.OpenPopup("Rename Circuit", () =>
                            {
                                ImGui.Text($"Rename circuit '{circuit.Name}' to:");
                                var circName = this.NewCircuitName;
                                ImGui.SetKeyboardFocusHere();
                                ImGui.InputText("##RenameCircuit", ref circName, 16);
                                this.NewCircuitName = circName;
                                if (ImGui.Button("Rename") || Input.IsKeyPressed(Keys.Enter))
                                {
                                    circuit.Name = circName;
                                    ImGui.CloseCurrentPopup();
                                }
                                ImGui.SameLine();
                                if (ImGui.Button("Cancel"))
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

                var open = true;
                if (ImGui.BeginPopupModal(this.RequestedPopupModalTitle, ref open, ImGuiWindowFlags.AlwaysAutoResize))
                {
                    this.RequestedPopupModalSubmit.Invoke();
                    ImGui.EndPopup();
                }

                if (!ImGui.IsPopupOpen(this.RequestedPopupModalTitle))
                {
                    this.RequestedPopupModal = false;
                }
            }

            if (this.RequestedPopupContext)
            {
                ImGui.OpenPopup("MAINCONTEXT");

                if (ImGui.BeginPopupContextWindow("MAINCONTEXT"))
                {
                    this.RequestedPopupContextSubmit.Invoke();

                    var size = ImGui.GetWindowSize();
                    var position = ImGui.GetWindowPos();

                    if (!position.CreateRect(size).Inflate(20).Contains(Input.GetMousePositionInWindow()))
                    {
                        ImGui.CloseCurrentPopup();
                        this.RequestedPopupContext = false;
                    }

                    ImGui.EndPopup();
                }

                if (!ImGui.IsPopupOpen("MAINCONTEXT"))
                {
                    this.RequestedPopupContext = false;
                }
            }
        }
    }
}