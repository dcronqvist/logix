using System.Collections;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using ImGuiNET;
using LogiX.Architecture.BuiltinComponents;
using LogiX.Architecture.Commands;
using LogiX.Architecture.Plugins;
using LogiX.Architecture.Serialization;
using LogiX.Architecture.StateMachine;
using LogiX.Content;
using LogiX.Content.Scripting;
using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Graphics.UI;
using LogiX.Rendering;
using static LogiX.OpenGL.GL;

namespace LogiX.Architecture;

public class Editor : Invoker<Circuit, Editor>
{
    // Currently loaded project and the currently open circuit from that project
    public LogiXProject Project { get; private set; }
    public Circuit CurrentlyOpenCircuit { get; private set; }
    public NodeInfoAttribute CurrentlyOpenCircuitInfoDocumentation { get; private set; }

    // GUI stuff, ImGui controller and framebuffers for both the circuit and the GUI
    public ImGuiController ImGuiController { get; private set; }
    public Framebuffer GUIFramebuffer { get; private set; }
    public Framebuffer WorkspaceFramebuffer { get; private set; }
    public Framebuffer NewComponentFramebuffer { get; private set; }

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
    public int[] AvailableTickRates { get; } = new int[] { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768 };
    // The currently selected "target" ticks per second for the simulation, defaults to 64 ticks per second
    public int CurrentlySelectedTickRate { get; set; } = 6;
    // The currently inputted custom tick rate
    public int CustomTickRate { get; set; } = 64;
    // Get the currently effective tick rate
    public int CurrentEffectiveTickRate => this.CurrentlySelectedTickRate == -1 ? this.CustomTickRate : this.AvailableTickRates[this.CurrentlySelectedTickRate];
    // Whether or not the simulation is supposed to be running, basically pauses the simulation threads while loop
    public bool SimulationRunning { get; set; } = true;
    // The finite state machine that controls the editor and all the different "tools", moving components, etc.
    public EditorFSM FSM { get; private set; }
    // If the simulation rendering should render wires
    public bool RenderWires { get; set; } = true;
    // If the editor should show what position the mouse is on in the workspace
    public bool ShowMousePosition { get; set; } = false;

    public float CurrentActivity { get; set; } = 0f;

    // Some variables for popups, like the FileDialog modal and stuff
    public bool RequestedPopupModal { get; set; }
    public Modal PopupModal { get; set; }

    // Top right current message 
    public string CurrentMessage { get; set; }

    // Some variables for a context menu that pops up when someone calls the "OpenContextMenu" method
    public bool RequestedPopupContext { get; set; }
    public Action RequestedPopupContextSubmit { get; set; }
    public Vector2 MouseStartPosition { get; set; }

    // Some variables for the AddNewComponent coroutine
    public NodeDescription NewComponent { get; set; }

    // All editor actions that should be visible in the main menu bar, and have shortcuts etc.
    public List<(string, List<(string, EditorAction)>)> EditorActions { get; private set; }

    public override Circuit GetCurrentInvokerState()
    {
        return this.GetCurrentCircuit();
    }

    private List<Guid> _currentComponentClipboard = new();
    private List<(Vector2i, Vector2i)> _currentSegmentClipboard = new();

    public Editor()
    {
        #region INITIALIZE FRAMEBUFFER AND INPUT
        // Define the GUI framebuffer and the workspace framebuffer
        this.GUIFramebuffer = new(true);
        this.WorkspaceFramebuffer = new(true);
        this.NewComponentFramebuffer = new(true);

        // Upon creating this editor, make sure that we have acquired the OpenGL context
        DisplayManager.LockedGLContext(() =>
        {
            // Create the ImGui controller
            // Loads with a custom font "opensans"
            this.ImGuiController = new((int)DisplayManager.GetWindowSizeInPixels().X, (int)DisplayManager.GetWindowSizeInPixels().Y, LogiXWindow.ContentManager.GetContentItems().Where(x => x is Font).Cast<Font>().ToArray());
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
            Camera.Zoom = Math.Clamp(Camera.Zoom, 0.5f, 10f);
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

        var projectHistory = Settings.GetSetting<List<string>>(Settings.RECENT_OPEN_PROJECTS);
        var keepHistory = new List<string>();
        foreach (var p in projectHistory)
        {
            if (File.Exists(p))
                keepHistory.Add(p);
        }
        Settings.SetSetting(Settings.RECENT_OPEN_PROJECTS, keepHistory);

        var uiScales = new (string, int)[] {
            ("Small", 16),
            ("Medium", 20),
            ("Large", 24)
        };
        var settingsUiScale = Settings.GetSetting<string>(Settings.UI_SCALE);

        if (settingsUiScale is null || !uiScales.Any(x => x.Item1 == settingsUiScale))
        {
            settingsUiScale = "Medium";
            Settings.SetSetting(Settings.UI_SCALE, settingsUiScale);
        }

        this._guiFontSize = uiScales.First(x => x.Item1 == settingsUiScale).Item2;

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

                this.Sim.LockedAction(s =>
                {
                    try
                    {
                        (int changed, int total) = s.Step();
                        float activity = (float)changed / total;

                        this.CurrentActivity = MathF.Max(this.CurrentActivity + (activity - this.CurrentActivity) * (0.8f / MathF.Sqrt(MathF.Max(this.CurrentTicksPerSecond, 1))), 0);
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
                long targetTps = this.CurrentEffectiveTickRate == -1 ? Stopwatch.Frequency : this.CurrentEffectiveTickRate;
                // Get how much time that the target tick rate should take
                long targetDiff = Stopwatch.Frequency / targetTps;

                // While the time that has passed is less than the target time, sleep for some time.
                while (sw.Elapsed.Ticks < start + targetDiff)
                {
                    await Task.Delay(TimeSpan.FromTicks(targetDiff / 10));
                }
                // Once we are done, set the current ticks per second to the target ticks per second
                long diff = sw.Elapsed.Ticks - start;
                double seconds = diff / (double)Stopwatch.Frequency;
                this.CurrentTicksPerSecond = this.CurrentTicksPerSecond + (1f / (float)seconds - this.CurrentTicksPerSecond) * (0.8f / MathF.Sqrt(targetTps));
            }
        });

        thread.IsBackground = true;
        thread.Priority = ThreadPriority.Normal;
        thread.Start();
        #endregion

        #region INITIALIZE EDITOR ACTIONS

        Input.OnKeyPressOrRepeat += (sender, e) =>
        {
            var key = e.Item1;
            var mods = e.Item2;

            if (ImGui.GetIO().WantCaptureKeyboard || ImGui.GetIO().WantCaptureMouse)
            {
                return;
            }

            foreach (var (category, actions) in this.EditorActions)
            {
                foreach (var (actionName, action) in actions)
                {
                    var actionMods = action.Modifiers;
                    var actionKey = action.Key;

                    if ((mods.HasFlag(actionMods) || actionMods == 0) && (key == actionKey) && action.Condition.Invoke(this))
                    {
                        try
                        {
                            action.Execute.Invoke(this);
                        }
                        catch (Exception ex)
                        {
                            this.OpenErrorPopup("Error", true, () => { ImGui.Text(ex.Message); ImGui.Button("OK"); });
                        }
                    }
                }
            }
        };

        // ALL FILE ACTIONS
        this.AddMainMenuItem("File", "New Project", new EditorAction((e) => true, (e) => false, (e) =>
        {
            // Verify that the user wants to create a new project
            // Currently very unforgiving, and should be changed later.

            string projectName = "";
            this.OpenPopup("New Project", (e) =>
            {
                ImGui.InputTextWithHint("Project Name", "project name...", ref projectName, 100);
                if (ImGui.Button("OK"))
                {
                    this.SetProject(LogiXProject.New(projectName));
                    ImGui.CloseCurrentPopup();
                }
            });

        }, ModifierKeys.Control, Keys.N));

        this.AddMainMenuItem("File", "Open Project", new EditorAction((e) => true, (e) => false, (e) =>
        {
            var fileDialog = new FileDialog(FileDialog.LastDirectory, "Open Project", FileDialogType.SelectFile, (s) =>
            {
                // On select
                if (this.Project is not null)
                {
                    this.SetMessage(this.QuickSaveProject());
                }

                var project = LogiXProject.FromFile(s);
                this.SetProject(project);
                Settings.SetSetting(Settings.LAST_OPEN_PROJECT, project.LoadedFromPath);

                var currProjHistory = Settings.GetSetting<List<string>>(Settings.RECENT_OPEN_PROJECTS);
                currProjHistory.Remove(s);
                currProjHistory.Insert(0, s);

                while (currProjHistory.Count > 10)
                {
                    currProjHistory.RemoveAt(10);
                }

                Settings.SetSetting(Settings.RECENT_OPEN_PROJECTS, currProjHistory);

            }, ".lxprojj");

            this.OpenPopup(fileDialog);
        }, ModifierKeys.Control, Keys.O));

        var recentProjects = Settings.GetSetting<List<string>>(Settings.RECENT_OPEN_PROJECTS);
        this.AddMainMenuItem("File", "Open Recent", new NestedEditorAction((e) => recentProjects.Count > 0, recentProjects.Select(rp =>
        {
            var rpCopy = rp;
            return (rp, new EditorAction((e) => true, (e) => false, (e) =>
            {
                if (this.Project is not null)
                {
                    this.SetMessage(this.QuickSaveProject());
                }

                var project = LogiXProject.FromFile(rpCopy);
                this.SetProject(project);
                Settings.SetSetting(Settings.LAST_OPEN_PROJECT, project.LoadedFromPath);

                var currProjHistory = Settings.GetSetting<List<string>>(Settings.RECENT_OPEN_PROJECTS);
                currProjHistory.Remove(rpCopy);
                currProjHistory.Insert(0, rpCopy);

                while (currProjHistory.Count > 10)
                {
                    currProjHistory.RemoveAt(10);
                }

                Settings.SetSetting(Settings.RECENT_OPEN_PROJECTS, currProjHistory);
            }));
        }).ToArray()));

        this.AddMainMenuItem("File", "Save Project", new EditorAction((e) => this.Project is not null, (e) => false, (e) =>
        {
            var newCircuit = this.Sim.LockedAction(s => s.GetCircuitInSimulation(this.CurrentlyOpenCircuit.Name));
            newCircuit.ID = this.CurrentlyOpenCircuit.ID;
            this.Project.UpdateCircuit(newCircuit);
            this.SetMessage(this.QuickSaveProject());
        }, ModifierKeys.Control, Keys.S));

        this.AddMainMenuItem("File", "Save Project As...", new EditorAction((e) => this.Project is not null, (e) => false, (e) =>
        {
            var fileDialog = new FileDialog(FileDialog.LastDirectory, "Save project as...", FileDialogType.SaveFile, (path) =>
            {
                this.Project.LoadedFromPath = Path.GetFullPath(path);
                Settings.SetSetting(Settings.LAST_OPEN_PROJECT, Path.GetFullPath(path));
                File.Create(path).Close();
                this.SetMessage(this.QuickSaveProject());
            });

            this.OpenPopup(fileDialog);
        }, 0, Keys.Unknown));

        // ALL EDIT ACTIONS
        this.AddMainMenuItem("Edit", "Undo", new EditorAction((e) => this.CurrentCommandIndex >= 0, (e) => false, (e) =>
        {
            var newState = this.Undo(this);
            this.Sim.LockedAction(s => s.SetCircuitInSimulation(newState));
        }, ModifierKeys.Control, Keys.Z));

        this.AddMainMenuItem("Edit", "Redo", new EditorAction((e) => this.CurrentCommandIndex < this.Commands.Count - 1, (e) => false, (e) =>
        {
            var newState = this.Redo(this);
            this.Sim.LockedAction(s => s.SetCircuitInSimulation(newState));
        }, ModifierKeys.Control, Keys.Y));

        this.AddMainMenuItem("Edit", "SEPARATOR", new SeparatorEditorAction());

        this.AddMainMenuItem("Edit", "Copy", new EditorAction((e) => this.Sim.LockedAction(s => s.HasSelection()), (e) => false, (e) =>
        {
            this._currentComponentClipboard = this.Sim.LockedAction(s => s.SelectedNodes.Select(c => c.ID)).ToList();
            // this._currentSegmentClipboard = this.Sim.LockedAction(s => s.SelectedWireSegments).ToList();

            this.SetMessage(TimedMessages(("Copied to clipboard!", 3000), ("", 0)));
        }, ModifierKeys.Control, Keys.C));

        this.AddMainMenuItem("Edit", "Paste", new EditorAction((e) => this._currentComponentClipboard.Count > 0, (e) => false, (e) =>
        {
            var comm = new CPaste(this._currentComponentClipboard, this._currentSegmentClipboard, Input.GetMousePosition(this.Camera).ToVector2i(Constants.GRIDSIZE));
            this.Execute(comm, this);
        }, ModifierKeys.Control, Keys.V));

        this.AddMainMenuItem("Edit", "SEPARATOR", new SeparatorEditorAction());

        this.AddMainMenuItem("Edit", "Delete Selection", new EditorAction((e) => this.Sim.LockedAction(s => s.HasSelection()), (e) => false, (e) =>
        {
            var commands = new List<Command<Editor>>();
            commands.AddRange(this.Sim.LockedAction(s => s.SelectedNodes.Select(c => new CDeleteNode(c.ID))));
            // commands.AddRange(this.Sim.LockedAction(s => s.SelectedWireSegments.Select(w => new CDeleteWireSegment(w))));
            this.Execute(new CMulti("Delete Selection", commands.ToArray()), this);
        }, 0, Keys.Delete));

        this.AddMainMenuItem("Edit", "Rotate Clockwise", new EditorAction((e) => this.Sim.LockedAction(s => s.SelectedNodes).Count > 0, (e) => false, (e) =>
        {
            var commands = this.Sim.LockedAction(s => s.SelectedNodes.Select(c => new CRotateComponent(c.ID, 1)));
            this.Execute(new CMulti("Rotate Selection CW", commands.ToArray()), this);
        }, ModifierKeys.Control, Keys.Right));

        this.AddMainMenuItem("Edit", "Rotate Counter Clockwise", new EditorAction((e) => this.Sim.LockedAction(s => s.SelectedNodes).Count > 0, (e) => false, (e) =>
        {
            var commands = this.Sim.LockedAction(s => s.SelectedNodes.Select(c => new CRotateComponent(c.ID, -1)));
            this.Execute(new CMulti("Rotate Selection CCW", commands.ToArray()), this);
        }, ModifierKeys.Control, Keys.Left));

        // ALL CIRCUIT ACTIONS
        this.AddMainMenuItem("Circuits", "New Circuit...", new EditorAction((e) => true, (e) => false, (e) =>
        {
            this.WritingNewCircuitName = true;
            this.NewCircuitName = "";
            this._projectsOpen = true;
        }, ModifierKeys.Control | ModifierKeys.Shift, Keys.N));

        // ALL SIMULATION ACTIONS
        this.AddMainMenuItem("Simulation", "Simulation Enabled", new EditorAction((e) => this.CurrentlyOpenCircuit is not null, (e) => this.SimulationRunning, (e) =>
        {
            this.SimulationRunning = !this.SimulationRunning;
        }, 0, Keys.F5));

        this.AddMainMenuItem("Simulation", $"Tick Rate", new NestedEditorAction((e) => true, this.AvailableTickRates.Select(tr => (tr.GetAsHertzString(), new EditorAction((e) => true, (e) => this.CurrentlySelectedTickRate == Array.IndexOf(this.AvailableTickRates, tr), (e) =>
        {
            this.CurrentlySelectedTickRate = Array.IndexOf(this.AvailableTickRates, tr);
        }, 0, Keys.Unknown))).ToList().Concat(new (string, EditorAction)[] { ("", new SeparatorEditorAction()), ("Custom", new EditorAction((e) => true, (e) => this.CurrentlySelectedTickRate == -1, (e) => {
            this.OpenPopup("Enter a custom tick rate", (s) => {
                var custom = this.CustomTickRate;
                if (ImGui.InputInt("Tick Rate", ref custom))
                {
                    this.CustomTickRate = Math.Clamp(custom, -1, this.AvailableTickRates.Max());
                }

                if (ImGui.Button("OK"))
                {
                    this.CurrentlySelectedTickRate = -1;
                    ImGui.CloseCurrentPopup();
                }

                ImGui.SameLine();

                if (ImGui.Button("Cancel"))
                {
                    ImGui.CloseCurrentPopup();
                }
            });
        }, 0, Keys.Unknown)) }).Concat(new (string, EditorAction)[] { ("Uncapped", new EditorAction((e) => true, (e) => this.CustomTickRate == -1 && this.CurrentlySelectedTickRate == -1, (e) => {
            this.CurrentlySelectedTickRate = -1;
            this.CustomTickRate = -1;
        }))}).ToArray()));

        this.AddMainMenuItem("Simulation", "Tick Once", new EditorAction((e) => this.CurrentlyOpenCircuit is not null, (e) => false, (e) =>
        {
            this.Sim.LockedAction(s => s.Step());
        }, 0, Keys.F6));

        // ALL HELP ACTIONS
        this.AddMainMenuItem("Help", "About", new EditorAction((e) => true, (e) => false, (e) =>
        {
            this._displayAboutWindow = true;
        }, 0, Keys.Unknown));

        this.AddMainMenuItem("Help", "Documentation", new EditorAction((e) => true, (e) => false, (e) =>
        {
            this.OpenPopup(new DynamicModal("Documentation", ImGuiWindowFlags.None, ImGuiPopupFlags.None, (e) =>
            {
                Utilities.RenderMarkdown(@"
# Docs

This is a work in progress, and documentation will be added later, when stuff stops changing so much.

For now, you can always right click a component in the left component window and click _**Show Help**_ to get help for that specific component.

Under *projects*, you can see your circuits, and right clicking them in the sidebar will allow you to edit them!
                ", (link) => { });
                if (ImGui.Button("OK"))
                {
                    ImGui.CloseCurrentPopup();
                }
            }));
        }, 0, Keys.Unknown));

        // ALL VIEW ACTIONS
        this.AddMainMenuItem("View", "Show Wires", new EditorAction((e) => true, (e) => this.RenderWires, (e) =>
        {
            this.RenderWires = !this.RenderWires;
        }, 0, Keys.Unknown));

        this.AddMainMenuItem("View", "Show Grid", new EditorAction((e) => true, (e) => this._drawGrid, (e) =>
        {
            this._drawGrid = !this._drawGrid;
        }, 0, Keys.Unknown));

        this.AddMainMenuItem("View", "Show Mouse Position", new EditorAction((e) => true, (e) => this.ShowMousePosition, (e) =>
        {
            this.ShowMousePosition = !this.ShowMousePosition;
        }, 0, Keys.Unknown));

        this.AddMainMenuItem("View", "UI Scale", new NestedEditorAction((e) => true, uiScales.Select(s => (s.Item1, new EditorAction((e) => true, (e) => this._guiFontSize == s.Item2, (e) =>
        {
            _ = Settings.SetSettingAsync(Settings.UI_SCALE, s.Item1);
            this._guiFontSize = s.Item2;
        }, 0, Keys.Unknown))).ToArray()));

        var plugins = PluginManager.GetPlugins();

        foreach (var plugin in plugins)
        {
            var meta = plugin.GetMeta();

            var allPluginActions = new NestedEditorAction((e) => plugin.GetActions().Count() > 0, plugin.GetActions().Select(a =>
            {
                return (a.Name, new EditorAction((e) => true, (e) => false, (e) =>
                {
                    a.Execute(this);
                }, 0, Keys.Unknown));
            }).ToArray());

            var pluginMenus = new List<(string, EditorAction)>();


            pluginMenus.Add(("About", new EditorAction((e) => true, (e) => false, (e) =>
            {
                this.OpenPopup($"About Plugin: {meta.Name}", (e) =>
                {
                    ImGui.Text($"{meta.Name} v{meta.Version}, by {meta.Author}");
                    ImGui.Text(meta.Description);
                });
            })));
            pluginMenus.Add(("", new SeparatorEditorAction()));
            pluginMenus.Add(("Actions", allPluginActions));


            this.AddMainMenuItem("Plugins", $"{meta.Name} (v{meta.Version})", new NestedEditorAction((e) => true, pluginMenus.ToArray()));
        }

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
    public void OpenPopup(string title, Action<Editor> submit, ImGuiWindowFlags windowFlags = ImGuiWindowFlags.AlwaysAutoResize)
    {
        this.OpenPopup(new DynamicModal(title, windowFlags, ImGuiPopupFlags.None, submit));
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

    public async IAsyncEnumerable<string> TimedMessages(params (string, int)[] messages)
    {
        foreach (var (message, time) in messages)
        {
            yield return message;
            await Task.Delay(time);
        }
    }

    public void SetMessage(IAsyncEnumerable<string> provider)
    {
        Task.Run(async () =>
        {
            await foreach (var message in provider)
            {
                this.CurrentMessage = message;
            }
        });
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

        NodeDescription.CurrentProject = project;
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
        else
        {
            this.OpenCircuit(project.Circuits.First().ID);
        }
    }

    /// <summary>
    /// Saves the current project to it's given path.
    /// TODO: If the path is null or empty, we should instead open a save file dialog.
    /// </summary>
    public async IAsyncEnumerable<string> QuickSaveProject()
    {
        if (this.Project.HasFileToSaveTo())
        {
            yield return "Saving project...";
            var newCircuit = this.Sim.LockedAction(s => s.GetCircuitInSimulation(this.CurrentlyOpenCircuit.Name));
            newCircuit.ID = this.CurrentlyOpenCircuit.ID;
            this.Project.UpdateCircuit(newCircuit);
            this.Project.Quicksave();
            yield return "Project saved!";
            await Task.Delay(5000);
            yield return "";
        }
        else
        {
            var fileDialog = new FileDialog(FileDialog.LastDirectory, "Save project as...", FileDialogType.SaveFile, (path) =>
            {
                this.Project.LoadedFromPath = Path.GetFullPath(path);
                Settings.SetSetting(Settings.LAST_OPEN_PROJECT, Path.GetFullPath(path));
                File.Create(path).Close();
                this.SetMessage(this.QuickSaveProject());
            }, this.Project.Name.Replace(" ", "_").ToLower() + ".lxprojj");

            this.OpenPopup(fileDialog);
        }
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
        this.Commands.Clear();
        this.CurrentCommandIndex = -1;
    }

    public Circuit GetCurrentCircuit()
    {
        var circ = this.Sim.LockedAction(s => s.GetCircuitInSimulation(this.CurrentlyOpenCircuit.Name));
        circ.ID = this.CurrentlyOpenCircuit.ID;

        return circ;
    }

    public override void Execute(Command<Editor> command, Editor arg, bool doExecute = true)
    {
        try
        {
            base.Execute(command, arg, doExecute);
        }
        catch (Exception ex)
        {
            // If an exception occurs, show a popup of it and stop the simulation
            this.OpenErrorPopup("Simulation Error", true, () =>
            {
                ImGui.Text(ex.Message);
                if (ImGui.Button("OK"))
                {
                    this.Undo(this);
                    this.FSM.SetState<StateIdle>(this, 0);
                    ImGui.CloseCurrentPopup();
                }
            });
        }
    }

    public override void Execute(Circuit stateBefore, Command<Editor> command, Editor arg, bool doExecute = true)
    {
        try
        {
            base.Execute(stateBefore, command, arg, doExecute);
        }
        catch (Exception ex)
        {
            // If an exception occurs, show a popup of it and stop the simulation
            this.OpenErrorPopup("Simulation Error", true, () =>
            {
                ImGui.Text(ex.Message);
                if (ImGui.Button("OK"))
                {
                    this.Undo(this);
                    this.FSM.SetState<StateIdle>(this, 0);
                    ImGui.CloseCurrentPopup();
                }
            });
        }
    }

    #endregion

    #region UPDATE METHODS

    public void AddNewComponent(NodeDescription desc)
    {
        this.NewComponent = desc;
        this.FSM.SetState<StateAddingNewComponent>(this, 0);
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

        var pShader = LogiXWindow.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.primitive");
        var color = ColorF.Darken(ColorF.LightGray, 0.63f);

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

    private int _guiFontSize = 20;
    private int _previousRenderMillis = 0;
    private bool _drawGrid = true;
    public void Render()
    {
        this.ImGuiController.Update(GameTime.DeltaTime);
        var fShader = LogiXWindow.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.fb_default");
        var pShader = LogiXWindow.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.primitive");
        var tShader = LogiXWindow.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.text");
        var font = Utilities.GetFont("core.font.default", 8);
        this._submittedInstances = 0;

        if (this.CurrentlyOpenCircuit is not null)
        {
            var watch = new Stopwatch();
            watch.Start();
            var watchStart = watch.ElapsedMilliseconds;

            this.WorkspaceFramebuffer.Bind(() =>
            {
                Framebuffer.Clear(ColorF.Darken(ColorF.LightGray, 0.7f));

                if (this._drawGrid)
                {
                    this.DrawGrid();
                }

                this.FSM.PreSimRender(this);

                this.Sim.LockedAction(s => s.Render(this.Camera, this.RenderWires), (e) => { });

                if (!this.FSM.CurrentState.RenderAboveGUI())
                {
                    this.FSM.PostSimRender(this);
                }

                this._submittedInstances += PrimitiveRenderer._submittedInstances;
                PrimitiveRenderer.FinalizeRender(pShader, this.Camera);
                TextRenderer.FinalizeRender(tShader, this.Camera);
            });

            this.NewComponentFramebuffer.Bind(() =>
            {
                Framebuffer.Clear(ColorF.Transparent);
                if (this.FSM.CurrentState.RenderAboveGUI())
                {
                    this.FSM.PostSimRender(this);
                    this._submittedInstances += PrimitiveRenderer._submittedInstances;
                    PrimitiveRenderer.FinalizeRender(pShader, this.Camera);
                    TextRenderer.FinalizeRender(tShader, this.Camera);
                }
            });

            this.GUIFramebuffer.Bind(() =>
            {

                var hasContext = DisplayManager.HasGLContext();
                var current = Framebuffer.GetCurrentBoundBuffer();

                Framebuffer.Clear(ColorF.Transparent);
                var font = Utilities.GetFont("core.font.opensans", _guiFontSize);
                Utilities.WithImGuiFont(font, () =>
                {
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
                });

                this.ImGuiController.Render();
            });

            glEnable(GL_BLEND);
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
            Framebuffer.RenderFrameBufferToScreen(fShader, this.WorkspaceFramebuffer);
            Framebuffer.RenderFrameBufferToScreen(fShader, this.GUIFramebuffer);
            Framebuffer.RenderFrameBufferToScreen(fShader, this.NewComponentFramebuffer);

            var watchEnd = watch.ElapsedMilliseconds;
            this._previousRenderMillis = (int)(watchEnd - watchStart);
        }
        else
        {
            this.GUIFramebuffer.Bind(() =>
            {
                Framebuffer.Clear(ColorF.Transparent);
                var font = Utilities.GetFont("core.font.opensans", 16);
                Utilities.WithImGuiFont(font, () =>
                {
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
                });
                this.ImGuiController.Render();
            });

            Framebuffer.RenderFrameBufferToScreen(fShader, this.GUIFramebuffer);
        }
    }

    #endregion

    #region GUI METHODS

    private bool _mouseOverComponentsWindow = false;
    public bool IsMouseOverComponentWindow()
    {
        return _mouseOverComponentsWindow;
    }

    private List<(string, EditorAction)> GetEditorActionCategory(string category)
    {
        if (this.EditorActions.Any(x => x.Item1 == category))
        {
            return this.EditorActions.First(x => x.Item1 == category).Item2;
        }

        var list = new List<(string, EditorAction)>();
        this.EditorActions.Add((category, list));
        return list;
    }

    public void AddMainMenuItem(string category, string name, EditorAction action)
    {
        if (this.EditorActions is null)
        {
            this.EditorActions = new();
        }

        var categoryList = this.GetEditorActionCategory(category);
        categoryList.Add((name, action));
    }

    private bool _displayAboutWindow = false;
    public void SubmitAboutLogiXWindow()
    {
        if (!_displayAboutWindow)
        {
            return;
        }

        ImGui.SetNextWindowSizeConstraints(new Vector2(200, 200), new Vector2(600, 600));
        ImGui.SetNextWindowSize(new Vector2(400, 200), ImGuiCond.FirstUseEver);
        var open = true;
        if (ImGui.Begin("About LogiX", ref open))
        {
            var about = LogiXWindow.ContentManager.GetContentItem<MarkdownFile>("core.markdown.about");
            Utilities.RenderMarkdown(about.Text, (url) =>
            {
                Utilities.OpenURL(url);
            });
        }

        if (!open)
        {
            _displayAboutWindow = false;
        }

        ImGui.End();
    }

    public void SubmitMainMenuBar()
    {
        foreach (var (category, actionList) in this.EditorActions)
        {
            if (ImGui.BeginMenu(category))
            {
                foreach (var (actionName, action) in actionList)
                {
                    action.SubmitGUI(this, actionName);
                }

                ImGui.EndMenu();
            }

            ImGui.Spacing();
        }

        if (this.CurrentMessage is not null && this.CurrentMessage != "")
        {
            var cursorX = ImGui.GetCursorPosX();
            var width = ImGui.GetWindowWidth();
            var text = this.CurrentMessage;
            var size = ImGui.CalcTextSize(text);
            ImGui.Dummy(new Vector2(width - size.X - cursorX - 10, 0));
            ImGui.Text(CurrentMessage);
        }

        // if (ImGui.BeginMenu("Edit"))
        // {
        //     if (ImGui.MenuItem("Rotate Selection Clockwise"))
        //     {
        //         // Testing rotations
        //         var selection = this.Sim.LockedAction(s => s.SelectedComponents);
        //         foreach (var c in selection)
        //         {
        //             c.RotateClockwise();
        //         }
        //     }
        //     if (ImGui.MenuItem("Rotate Selection Counter Clockwise"))
        //     {
        //         // Testing rotations
        //         var selection = this.Sim.LockedAction(s => s.SelectedComponents);
        //         foreach (var c in selection)
        //         {
        //             c.RotateCounterClockwise();
        //         }
        //     }

        //     ImGui.EndMenu();
        // }

        // if (ImGui.BeginMenu("Simulation"))
        // {
        //     if (ImGui.MenuItem("Tick Once", "", false, !this.SimulationRunning))
        //     {
        //         this.Sim.LockedAction(s => s.Tick());
        //     }

        //     ImGui.EndMenu();
        // }

        // if (ImGui.BeginMenu("View"))
        // {
        //     if (ImGui.MenuItem("About LogiX"))
        //     {
        //         this._displayAboutWindow = true;
        //     }

        //     ImGui.EndMenu();
        // }
    }

    private int _submittedInstances = 0;
    public void SubmitStatusMenuBar()
    {
        ImGui.Text($"{this.CurrentTicksPerSecond.GetAsHertzString()}");
        ImGui.TextUnformatted($"Activity: {MathF.Round(this.CurrentActivity * 100f, 1).ToString("0.0")}%");
        ImGui.Text($"{this._submittedInstances} @ {this._previousRenderMillis.ToString("00")} ms");
        ImGui.Separator();

        if (ImGui.BeginMenu($"Zoom: {(this.Camera.Zoom * 100f).ToString("0")}%###zoomMenu"))
        {
            if (ImGui.BeginMenu("Set Zoom to..."))
            {
                if (ImGui.MenuItem("25%"))
                {
                    this.Camera.Zoom = 0.25f;
                }
                if (ImGui.MenuItem("50%"))
                {
                    this.Camera.Zoom = 0.5f;
                }
                if (ImGui.MenuItem("75%"))
                {
                    this.Camera.Zoom = 0.75f;
                }
                if (ImGui.MenuItem("100%"))
                {
                    this.Camera.Zoom = 1f;
                }
                if (ImGui.MenuItem("125%"))
                {
                    this.Camera.Zoom = 1.25f;
                }
                if (ImGui.MenuItem("150%"))
                {
                    this.Camera.Zoom = 1.5f;
                }
                if (ImGui.MenuItem("175%"))
                {
                    this.Camera.Zoom = 1.75f;
                }
                if (ImGui.MenuItem("200%"))
                {
                    this.Camera.Zoom = 2f;
                }

                ImGui.EndMenu();
            }

            ImGui.Separator();
            if (ImGui.MenuItem("Reset Zoom"))
            {
                this.Camera.Zoom = 1f;
            }


            ImGui.EndMenu();
        }

        ImGui.Text($"{this.FSM.CurrentState.GetType().Name}");
        //ImGui.Text($"{this.Sim.LockedAction(s => s.Scheduler.ScheduledEvents.Count)}:{this.Sim.LockedAction(s => s.Scheduler.ScheduledEvents.Select(x => x.Count).Sum())} events");
    }

    private bool _projectsOpen = false;
    public void SubmitComponentsWindow(Vector2 mainMenuBarSize, Vector2 underMenuBarSize)
    {
        ImGui.SetNextWindowPos(new Vector2(0, mainMenuBarSize.Y));
        ImGui.SetNextWindowSize(new Vector2(12 * _guiFontSize, DisplayManager.GetWindowSizeInPixels().Y - mainMenuBarSize.Y - underMenuBarSize.Y));
        if (ImGui.Begin("Components", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.AlwaysVerticalScrollbar))
        {
            if (_projectsOpen)
            {
                ImGui.SetNextItemOpen(true, ImGuiCond.Once);
                this._projectsOpen = false;
            }
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
                        this.AddNewComponent(Integrated.CreateDescriptionFromCircuit(circuit.Name, circuit));
                    }
                    if (ImGui.BeginPopupContextItem())
                    {
                        if (ImGui.MenuItem("Edit Circuit", this.CurrentlyOpenCircuit is null ? true : this.CurrentlyOpenCircuit.ID != circuit.ID))
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

                                if (!ImGui.IsAnyItemHovered())
                                {
                                    ImGui.SetKeyboardFocusHere();
                                }

                                ImGui.InputText("##RenameCircuit", ref circName, 16);
                                this.NewCircuitName = circName;

                                if (this.NewCircuitName.Length == 0)
                                {
                                    ImGui.BeginDisabled();
                                }

                                if (ImGui.Button("Rename") || Input.IsKeyPressed(Keys.Enter))
                                {
                                    circuit.Name = circName;
                                    ImGui.CloseCurrentPopup();
                                }
                                if (this.NewCircuitName.Length == 0)
                                {
                                    ImGui.EndDisabled();
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

            var componentCategories = NodeDescription.GetAllNodeCategories();

            foreach (var category in componentCategories)
            {
                if (ImGui.CollapsingHeader(category.Key.ToString()))
                {
                    ImGui.TreePush(category.Key.ToString());
                    foreach (var c in category.Value)
                    {
                        var cInfo = NodeDescription.GetNodeInfo(c);
                        ImGui.MenuItem(cInfo.DisplayName);

                        if (ImGui.IsItemClicked())
                        {
                            this.AddNewComponent(NodeDescription.CreateDefaultNodeDescription(c));
                        }

                        if (ImGui.BeginPopupContextItem())
                        {
                            if (ImGui.MenuItem("Show Help"))
                            {
                                if (cInfo.DocumentationAsset is null)
                                {
                                    this.OpenErrorPopup("Missing documentation", false, () => ImGui.Text("No documentation available for this component."));
                                }
                                else
                                {
                                    this.CurrentlyOpenCircuitInfoDocumentation = cInfo;
                                }
                            }
                            ImGui.EndPopup();
                        }
                    }
                    ImGui.TreePop();
                }
            }
        }
        this._mouseOverComponentsWindow = ImGui.IsWindowHovered();
        ImGui.End();
    }

    public void SubmitSingleSelectedPropertyWindow()
    {
        if (this.Sim?.LockedAction(s => s.SelectedNodes.Count == 1) == true)
        {
            var selected = this.Sim.LockedAction(s => s.SelectedNodes.First());
            var index = this.Sim.LockedAction(s => s.Nodes.IndexOf(selected));
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

        if (this.CurrentlyOpenCircuitInfoDocumentation is not null)
        {
            var open = true;
            if (ImGui.Begin("Component Documentation", ref open, ImGuiWindowFlags.None))
            {
                var docAsset = LogiXWindow.ContentManager.GetContentItem<MarkdownFile>(this.CurrentlyOpenCircuitInfoDocumentation.DocumentationAsset);
                Utilities.RenderMarkdown(docAsset.Text, (url) =>
                {
                    if (url.StartsWith("http"))
                    {
                        Utilities.OpenURL(url);
                    }
                    else if (url.StartsWith("component://"))
                    {
                        var component = NodeDescription.GetNodeInfo(url.Substring("component://".Length));
                        this.CurrentlyOpenCircuitInfoDocumentation = component;
                    }
                });
            }

            if (!open)
            {
                this.CurrentlyOpenCircuitInfoDocumentation = null;
            }
            ImGui.End();
        }

        this.SubmitAboutLogiXWindow();

        // Show the main menu bar
        ImGui.BeginMainMenuBar();
        this.SubmitMainMenuBar();

        // Need main menu bar height to determine where to place components window
        var mainMenuBarSize = ImGui.GetWindowSize();
        ImGui.EndMainMenuBar();

        // Status menu bar
        ImGui.SetNextWindowPos(new Vector2(0, ImGui.GetIO().DisplaySize.Y - mainMenuBarSize.Y));
        ImGui.SetNextWindowSize(new Vector2(ImGui.GetIO().DisplaySize.X, mainMenuBarSize.Y), ImGuiCond.Always);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, Vector2.Zero);
        ImGui.Begin("##STATUSMENUBAR", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.MenuBar);
        ImGui.BeginMenuBar();
        this.SubmitStatusMenuBar();
        ImGui.EndMenuBar();
        var underMenuBarSize = ImGui.GetWindowSize();
        ImGui.End();
        ImGui.PopStyleVar(2);

        // Show the components window which you can drag new components from
        this.SubmitComponentsWindow(mainMenuBarSize, underMenuBarSize);

        // Show the properties window for the selected component
        this.SubmitSingleSelectedPropertyWindow();

        // Show the currently open popup context
        this.SubmitContextMenu();

        // Show the currently open popup modal
        this.SubmitPopupModals();

        if (this.ShowMousePosition)
        {
            var mousePos = Input.GetMousePosition(this.Camera);
            var aligned = mousePos.ToVector2i(Constants.GRIDSIZE);

            Utilities.MouseToolTip($"({aligned.X}, {aligned.Y})");
        }
    }

    #endregion
}