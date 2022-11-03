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
    // Currently loaded project and the currently open circuit from that project
    public LogiXProject Project { get; private set; }
    public Circuit CurrentlyOpenCircuit { get; private set; }

    // GUI stuff, ImGui controller and framebuffers for both the circuit and the GUI
    public ImGuiController ImGuiController { get; private set; }
    public Framebuffer GUIFramebuffer { get; private set; }
    public Framebuffer WorkspaceFramebuffer { get; private set; }

    // A few temporary variables for creating a new circuit
    public bool WritingNewCircuitName { get; set; }
    public string NewCircuitName { get; set; }

    // Some variables and stuff for the actual editor
    // Camera used to pan around the workspace
    public Camera2D Camera { get; private set; }
    // The currently running simulation of the CurrentlyOpenCircuit
    public ThreadSafe<Simulation> Sim { get; private set; }
    // How many ticks per second the simulation is currently running at
    public float CurrentTicksPerSecond { get; set; }
    // The available "target" ticks per second for the simulation
    public int[] AvailableTickRates { get; } = new int[] { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384 };
    // The currently selected "target" ticks per second for the simulation, defaults to 64 ticks per second
    public int CurrentlySelectedTickRate { get; set; } = 6;
    // Whether or not the simulation is supposed to be running, basically pauses the simulation threads while loop
    public bool SimulationRunning { get; set; } = true;
    // The finite state machine that controls the editor and all the different "tools", moving components, etc.
    public EditorFSM FSM { get; private set; }

    // Some variables for popups, like the FileDialog modal and stuff
    public bool RequestedPopupModal { get; set; }
    public Modal PopupModal { get; set; }

    // Some variables for a context menu that pops up when someone calls the "OpenContextMenu" method
    public bool RequestedPopupContext { get; set; }
    public Action RequestedPopupContextSubmit { get; set; }
    public Vector2 MouseStartPosition { get; set; }

    public Editor()
    {
        #region INITIALIZE FRAMEBUFFER AND INPUT
        // Define the GUI framebuffer and the workspace framebuffer
        this.GUIFramebuffer = new(true);
        this.WorkspaceFramebuffer = new(true);

        // Upon creating this editor, make sure that we have acquired the OpenGL context
        DisplayManager.LockedGLContext(() =>
        {
            // Create the ImGui controller
            // Loads with a custom font "opensans"
            this.ImGuiController = new((int)DisplayManager.GetWindowSizeInPixels().X, (int)DisplayManager.GetWindowSizeInPixels().Y, LogiX.ContentManager.GetContentItem<Font>("content_1.font.opensans"));
        });

        // This event is called every time the mouse wheel is scrolled
        Input.OnScroll += (sender, e) =>
        {
            // Tell the ImGui backend that we performed a scroll
            this.ImGuiController.MouseScroll(new Vector2(0, e));

            // If the window is not focused, or if the ImGui backend is using the mouse, return
            if (!DisplayManager.IsWindowFocused() || ImGui.GetIO().WantCaptureMouse)
            {
                return; // Early return if the window is not focused.
            }

            // If the ImGui backend is not using the mouse, then we can zoom in and out
            if (e > 0)
                Camera.Zoom *= 1.1f;
            else if (e < 0)
                Camera.Zoom /= 1.1f;

            // Clamp the zoom to a minimum of 0.3f and a maximum of 10f, so we don't zoom in too far or out too far
            Camera.Zoom = Math.Clamp(Camera.Zoom, 0.3f, 10f);
        };

        // This event is called every time a key that represents a character is pressed.
        Input.OnChar += (sender, e) =>
        {
            // Let the ImGui backend know that we pressed a character
            this.ImGuiController.PressChar(e);
        };

        // Called everytime the window changes size
        DisplayManager.OnFramebufferResize += (sender, e) =>
        {
            // Let the ImGui backend know that the window changed size so it can update its internal framebuffer
            this.ImGuiController.WindowResized((int)e.X, (int)e.Y);
        };

        #endregion

        #region INITIALIZE PROJECT
        // Get the previously opened project from the settings file
        var lastOpenProject = Settings.GetSetting<string>(Settings.LAST_OPEN_PROJECT);

        // If the last open project is either null or empty, then we must create a new project
        if (lastOpenProject is null || lastOpenProject.Length == 0)
        {
            this.SetProject(LogiXProject.New("Untitled"));
        }
        else
        {
            // Make sure that the file exists, if it doesn't also make a new project
            if (!File.Exists(lastOpenProject))
            {
                this.SetProject(LogiXProject.New("Untitled"));
            }
            else
            {
                // If it exists, load the project from file.
                // TODO: If an error occurs here, we should instead make a new project and show a popup saying that the project failed to load
                this.SetProject(LogiXProject.FromFile(lastOpenProject));
            }
        }

        #endregion

        #region CREATE SIMULATION THREAD
        var thread = new Thread(async () =>
        {
            // Create a stopwatch to measure a tick's time
            Stopwatch sw = new();
            sw.Start();

            while (true)
            {
                // Get start time
                long start = sw.Elapsed.Ticks;

                // If we are not supposed to run or there is no current sim, just sleep for 100 ms and continue.
                if (this.Sim is null || !this.SimulationRunning)
                {
                    this.CurrentTicksPerSecond = 0;
                    await Task.Delay(100);
                    continue;
                }

                // Run a single tick in the simulation
                this.Sim.LockedAction(s =>
                {
                    try
                    {
                        s.Tick();
                    }
                    catch (System.Exception ex)
                    {
                        // If an exception occurs, show a popup of it and stop the simulation
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

                // Get target tick rate
                int targetTps = this.AvailableTickRates[this.CurrentlySelectedTickRate];

                // Get how much time that the target tick rate should take
                long targetDiff = TimeSpan.TicksPerSecond / targetTps;

                // While the time that has passed is less than the target time, sleep for some time.
                while (sw.Elapsed.Ticks < start + targetDiff)
                {
                    await Task.Delay(TimeSpan.FromTicks(targetDiff / 10));
                }

                // Once we are done, set the current ticks per second to the target ticks per second
                long diff = sw.Elapsed.Ticks - start;
                double seconds = diff / (double)TimeSpan.TicksPerSecond;
                this.CurrentTicksPerSecond = this.CurrentTicksPerSecond + (1f / (float)seconds - this.CurrentTicksPerSecond) * (0.8f / MathF.Sqrt(targetTps));
            }
        });

        thread.IsBackground = true;
        thread.Priority = ThreadPriority.Highest;
        thread.Start();
        #endregion
    }

    #region POPUP METHODS

    /// <summary>
    /// Opens a popup from the given <see cref="Modal"/>.
    /// </summary>
    public void OpenPopup(Modal modal)
    {
        this.RequestedPopupModal = true;
        this.PopupModal = modal;
    }

    /// <summary>
    /// Opens a dynamic popup with a given title and content.
    /// </summary>
    public void OpenPopup(string title, Action<Editor> submit)
    {
        this.OpenPopup(new DynamicModal(title, ImGuiWindowFlags.AlwaysAutoResize, ImGuiPopupFlags.None, submit));
    }

    /// <summary>
    /// Opens a dynamic context menu with the given content.
    /// </summary>
    public void OpenContextMenu(Action submit)
    {
        this.RequestedPopupContext = true;
        this.RequestedPopupContextSubmit = submit;
        this.MouseStartPosition = Input.GetMousePositionInWindow();
    }

    /// <summary>
    /// Opens an error popup with a given title and content, also allows you to set if the error should cause the simulation to stop.
    /// </summary>
    public void OpenErrorPopup(string title, bool stopSim, Action submit)
    {
        this.OpenPopup(new DynamicModal(title, ImGuiWindowFlags.AlwaysAutoResize, ImGuiPopupFlags.None, (e) => submit()));
        this.SimulationRunning = !stopSim;
    }

    #endregion

    #region PROJECT METHODS

    /// <summary>
    /// Sets the current project to the given <see cref="LogiXProject"/>.
    /// Makes sure to also save the currently open circuit if there is one.
    /// </summary>
    public void SetProject(LogiXProject project)
    {
        if (this.CurrentlyOpenCircuit is not null)
        {
            var newCircuit = this.Sim.LockedAction(s => s.GetCircuitInSimulation(this.CurrentlyOpenCircuit.Name));
            newCircuit.ID = this.CurrentlyOpenCircuit.ID;
            this.Project.UpdateCircuit(newCircuit);
            this.CurrentlyOpenCircuit = null;
        }

        ComponentDescription.CurrentProject = project;
        this.Project = project;
        DisplayManager.SetWindowTitle($"LogiX - {this.Project.Name}");
        this.Camera = new Camera2D(Vector2.Zero, 1f);
        this.FSM = new EditorFSM();
        this.Commands.Clear();
        this.CurrentCommandIndex = -1;

        if (project.LastOpenedCircuit != Guid.Empty)
        {
            this.OpenCircuit(project.LastOpenedCircuit);
        }
    }

    /// <summary>
    /// Saves the current project to it's given path.
    /// TODO: If the path is null or empty, we should instead open a save file dialog.
    /// </summary>
    public void QuickSaveProject()
    {
        var newCircuit = this.Sim.LockedAction(s => s.GetCircuitInSimulation(this.CurrentlyOpenCircuit.Name));
        newCircuit.ID = this.CurrentlyOpenCircuit.ID;
        this.Project.UpdateCircuit(newCircuit);
        this.Project.Quicksave();
    }

    /// <summary>
    /// Opens the specified circuit in the current project. Saves the currently open circuit if there is one.
    /// </summary>
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
        this.Camera = new Camera2D(circuit.GetMiddleOfCircuit().ToVector2(Constants.GRIDSIZE), 1f);

        this.Sim = new(Simulation.FromCircuit(circuit));
        this.Project.SetLastOpenedCircuit(id);
    }

    #endregion

    #region CURRENTLY OPEN CIRCUIT METHODS

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

    #endregion

    #region RENDERING METHODS

    /// <summary>
    /// Renders a grid across the editor
    /// </summary>
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

    #endregion

    #region GUI METHODS

    public void SubmitMainMenuBar()
    {
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
                this.Project.Quicksave();
            }
            if (ImGui.MenuItem("Open Project...", "Ctrl+O", false, true))
            {
                var fileDialog = new FileDialog(".", FileDialogType.SelectFile, (s) =>
                {
                    // On select
                    if (this.Project is not null)
                    {
                        this.QuickSaveProject();
                    }

                    var project = LogiXProject.FromFile(s);
                    this.SetProject(project);
                    Settings.SetSetting(Settings.LAST_OPEN_PROJECT, project.LoadedFromPath);

                }, ".lxprojj");

                this.OpenPopup(fileDialog);
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
            if (ImGui.MenuItem("Style Editor", "", ref styleEditorOpen))
            {

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
    }

    public void SubmitComponentsWindow(Vector2 mainMenuBarSize)
    {
        ImGui.SetNextWindowPos(new Vector2(0, mainMenuBarSize.Y));
        ImGui.SetNextWindowSize(new Vector2(180, DisplayManager.GetWindowSizeInPixels().Y - mainMenuBarSize.Y));
        if (ImGui.Begin("Components", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.AlwaysVerticalScrollbar))
        {
            if (ImGui.CollapsingHeader("Project"))
            {
                ImGui.TreePush();
                // First your own circuits in the project you have loaded.
                var circuits = this.Project.Circuits;

                foreach (var circuit in circuits)
                {
                    ImGui.Selectable(circuit.Name, this.CurrentlyOpenCircuit is not null ? circuit.ID == CurrentlyOpenCircuit.ID : false);
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
                            this.OpenPopup("Delete Circuit", (e) =>
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
                            this.OpenPopup("Rename Circuit", (e) =>
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
        }
        ImGui.End();
    }

    public void SubmitSingleSelectedPropertyWindow()
    {
        if (this.Sim?.LockedAction(s => s.SelectedComponents.Count == 1) == true)
        {
            var selected = this.Sim.LockedAction(s => s.SelectedComponents.First());
            var index = this.Sim.LockedAction(s => s.Components.IndexOf(selected));
            selected.CompleteSubmitUISelected(this, index);
        }
    }

    public void SubmitPopupModals()
    {
        if (this.RequestedPopupModal)
        {
            ImGui.OpenPopup(this.PopupModal.Title, this.PopupModal.PopupFlags);

            var open = true;
            if (ImGui.BeginPopupModal(this.PopupModal.Title, ref open, this.PopupModal.WindowFlags))
            {
                this.PopupModal.SubmitUI(this);
                ImGui.EndPopup();
            }

            if (!ImGui.IsPopupOpen(this.PopupModal.Title))
            {
                this.RequestedPopupModal = false;
            }
        }
    }

    public void SubmitContextMenu()
    {
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

    private bool styleEditorOpen = false;
    public void SubmitGUI()
    {
        // Show the style editor 
        if (styleEditorOpen)
        {
            ImGui.Begin("Style Editor", ref styleEditorOpen);
            ImGui.ShowStyleEditor();
            ImGui.End();
        }

        // Show the main menu bar
        ImGui.BeginMainMenuBar();
        this.SubmitMainMenuBar();

        // Need main menu bar height to determine where to place components window
        var mainMenuBarSize = ImGui.GetWindowSize();
        ImGui.EndMainMenuBar();

        // Show the components window which you can drag new components from
        this.SubmitComponentsWindow(mainMenuBarSize);

        // Show the properties window for the selected component
        this.SubmitSingleSelectedPropertyWindow();

        // Show the currently open popup modal
        this.SubmitPopupModals();

        // Show the currently open popup context
        this.SubmitContextMenu();
    }

    #endregion
}