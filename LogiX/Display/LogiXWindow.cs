using ImGuiNET;
using LogiX.Assets;
using LogiX.Circuits.Drawables;
using LogiX.Circuits.Integrated;
using LogiX.Circuits.Logic;
using LogiX.Logging;
using LogiX.Projects;
using LogiX.Settings;
using LogiX.Simulation;
using LogiX.UI;
using LogiX.Utils;
using Newtonsoft.Json;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;


namespace LogiX.Display
{
    enum EditorState
    {
        None,
        MovingCamera,
        MovingSelection, 
        RectangleSelecting, 
        HoveringUI,
        HoveringInput,
        HoveringOutput,
        OutputToInput,
        SelectingProjectFile,
        IncludeCollection,
        IncludeDescription,
        SavingProjectFile
    }

    class LogiXWindow : BaseWindow
    {
        // Keeps track of what we're doing right now
        EditorState state;

        // State variables

        // RectangleSelecting
        Vector2 startPos;
        Rectangle selectionRec;

        // OutputToInput
        Tuple<int, DrawableComponent> tempOutput;

        // Creating new IC
        DrawableComponent[] icInputs;
        DrawableComponent[] icOutputs;
        DrawableComponent[] theRest;

        // World camera
        Camera2D cam;

        // Keeps track of mouse positions and allows for
        // viewport -> world coordinates.
        Vector2 currentMousePos;
        Vector2 previousMousePos;
        public Vector2 TopLeftCorner
        {
            get
            {
                return new Vector2(cam.target.X - ((WindowSize.X / 2f) / cam.zoom), cam.target.Y - ((WindowSize.Y / 2f) / cam.zoom));
            }
        }

        // Creating & dragging new component
        DrawableComponent newComponent;
        FileDialog fd;

        // Controller for ImGui.NET
        ImGUIController controller;

        bool _simulationOn;
        bool _makingIc;
        string newIcName = "";
        bool yes = true;
        bool editingSettings = false;
        bool icWindowOpen = false;
        private bool creatingCollection;
        List<ICCollection> collections;
        List<ICDescription> nonCollectionDescriptions;
        List<ICDescription> selectedIcs;
        string newCollectionName;

        LogiXProject currentProject;

        public LogiXWindow() : base(new Vector2(1280, 720), "LogiX")
        {
            state = EditorState.None;
        }

        public override void Initialize()
        {
            // Should initialize some basic settings and stuff
            // Load settings files etc.

            // Set window icon and stuff can also be done with this.
            SettingManager.LoadSettings();

            // Apply window size settings to window
            int windowWidth = int.Parse(SettingManager.GetSetting("window-width", "1440"));
            int windowHeight = int.Parse(SettingManager.GetSetting("window-height", "768"));
            Vector2 size = new Vector2(windowWidth, windowHeight);
            SetWindowSize(size, false);
            LogManager.AddEntry($"Creating LogiX window with width = {(int)size.X} and height = {(int)size.Y}");

            // Create new camera that's focused on (0,0).
            // This should maybe be done somewhere else. But is good for testing atm.
            cam = new Camera2D(size / 2f, Vector2.Zero, 0f, 1f);
            _simulationOn = true;
            _makingIc = false;
        }

        public override void LoadContent()
        {
            // Load files for use in the program.
            Raylib.SetTargetFPS(500);
            controller = new ImGUIController();
            controller.Load((int)base.WindowSize.X, (int)base.WindowSize.Y);

            // Load all ics and stuff
            //AssetManager.LoadAllAssets();
            SetWindowSize(WindowSize, true);

            string latestProject = SettingManager.GetSetting("latest-project", "");
            if(latestProject != "")
            {
                LogiXProject lp = LogiXProject.LoadFromFile(latestProject);
                if(lp != null)
                    SetProject(lp);
                else
                    SetProject(new LogiXProject("new-project"));
            }
            else
            {
                SetProject(new LogiXProject("new-project"));
            }
        }

        public void SetProject(LogiXProject project)
        {
            currentProject = project;
            collections = currentProject.GetAllCollections();
            nonCollectionDescriptions = currentProject.GetAllNonCollectionDescriptions();
        }

        public Vector2 GetMousePositionInWorld()
        {
            return TopLeftCorner + currentMousePos / cam.zoom;
        }

        public void SetNewComponent(DrawableComponent dc)
        {
            newComponent = dc;
            currentProject.Simulation.ClearSelectedComponents();
            currentProject.Simulation.AddComponent(dc);
            currentProject.Simulation.AddSelectedComponent(dc);
            state = EditorState.MovingSelection;
        }

        public void RenderGrid()
        {
            // TODO: Move this to more suitable spot, also try to shorten this down.
            int windowWidth = (int)base.WindowSize.X;
            int windowHeight = (int)base.WindowSize.Y;
            Vector2 size = new Vector2(windowWidth, windowHeight);
            Vector2 camPos = cam.target;
            Vector2 viewSize = size / cam.zoom;

            int pixelsBetweenLines = int.Parse(SettingManager.GetSetting("grid-size", "250"));

            // Render vertical lines
            for (int i = (int)((camPos.X - viewSize.X / 2f) / pixelsBetweenLines); i <= (int)((camPos.X + viewSize.X / 2f) / pixelsBetweenLines); i++)
            {
                float lineX = i * pixelsBetweenLines;
                float lineYstart = (camPos.Y - viewSize.Y / 2f);
                float lineYend = (camPos.Y + viewSize.Y / 2f);

                Raylib.DrawLine((int)lineX, (int)lineYstart, (int)lineX, (int)lineYend, Color.DARKGRAY.Opacity(0.3f));
            }

            // Render vertical lines
            for (int i = (int)((camPos.Y - viewSize.Y / 2f) / pixelsBetweenLines); i <= (int)((camPos.Y + viewSize.Y / 2f) / pixelsBetweenLines); i++)
            {
                float lineY = i * pixelsBetweenLines;
                float lineXstart = (camPos.X - viewSize.X / 2f);
                float lineXend = (camPos.X + viewSize.X / 2f);
                Raylib.DrawLine((int)lineXstart, (int)lineY, (int)lineXend, (int)lineY, Color.DARKGRAY.Opacity(0.3f));
            }
        }

        public override void Render()
        {
            Raylib.BeginDrawing();
            Raylib.BeginMode2D(cam);
            Raylib.ClearBackground(Color.LIGHTGRAY);

            RenderGrid();
            currentProject.Simulation.Render(GetMousePositionInWorld());
            if(state == EditorState.OutputToInput)
            {
                Raylib.DrawLineBezier(tempOutput.Item2.GetOutputPosition(tempOutput.Item1), GetMousePositionInWorld(), 2f, Color.WHITE);
            }

            if(state == EditorState.RectangleSelecting)
            {
                Raylib.DrawRectangleRec(selectionRec, Color.BLUE.Opacity(0.3f));
                Raylib.DrawRectangleLinesEx(selectionRec, 1, Color.BLUE.Opacity(0.8f));
            }
            Raylib.EndMode2D();
            
            controller.Draw();
            Raylib.EndDrawing();
        }

        public override void Update()
        {
            // Update ImGui, and submit UI.
            controller.Update(Raylib.GetFrameTime());
            SubmitUI();

            // TODO: Move this to more suitable place
            currentMousePos = Raylib.GetMousePosition();

            // This comes from being able to turn off simulation in the main menu
            if(_simulationOn)
                currentProject.Simulation.UpdateLogic(GetMousePositionInWorld());
            // Should always update all updateable components though, like being able
            // to turn off and on switches during sim-off.
            currentProject.Simulation.Update(GetMousePositionInWorld());

            // Get the hovered component, is null if nothing is hovered.
            DrawableComponent hovered = currentProject.Simulation.GetComponentFromPosition(GetMousePositionInWorld());
            // Hovering inputs & outputs
            Tuple<int, DrawableComponent> hoveredInput = currentProject.Simulation.GetComponentAndInputFromPos(GetMousePositionInWorld());
            Tuple<int, DrawableComponent> hoveredOutput = currentProject.Simulation.GetComponentAndOutputFromPos(GetMousePositionInWorld());

            // Allow for camera zooming with the mouse wheel.
            #region Mouse Wheel Camera Zoom

            if (!ImGui.GetIO().WantCaptureMouse)
            {
                if (Raylib.GetMouseWheelMove() > 0)
                {
                    cam.zoom *= 1.05f;
                }
                if (Raylib.GetMouseWheelMove() < 0)
                {
                    cam.zoom *= 1 / 1.05f;
                }
            }
            #endregion

            #region Decide Which Editor State
            if (state != EditorState.SelectingProjectFile && state != EditorState.IncludeCollection && state != EditorState.IncludeDescription && state != EditorState.SavingProjectFile)
            {
                if (!ImGui.GetIO().WantCaptureMouse || newComponent != null)
                {
                    if(Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT) && Raylib.IsKeyPressed(KeyboardKey.KEY_D))
                    {

                        CopySelectedComponentsNewOffsetPos(new Vector2(100, 100));
                    }

                    if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_MIDDLE_BUTTON) && state == EditorState.None)
                    {
                        state = EditorState.MovingCamera;
                    }
                    if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_MIDDLE_BUTTON) && state == EditorState.MovingCamera)
                    {
                        state = EditorState.None;
                    }

                    if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON) && state == EditorState.None)
                    {
                        if (hovered == null)
                        {
                            state = EditorState.RectangleSelecting;
                            startPos = GetMousePositionInWorld();
                        }
                        else if (currentProject.Simulation.SelectedComponents.Contains(hovered))
                        {
                            state = EditorState.MovingSelection;
                        }
                    }
                    if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON) && (state == EditorState.MovingSelection || state == EditorState.RectangleSelecting))
                    {
                        state = EditorState.None;
                        newComponent = null;
                    }

                    if (state == EditorState.None)
                    {
                        if (hoveredInput != null)
                            state = EditorState.HoveringInput;
                        else if (hoveredOutput != null)
                            state = EditorState.HoveringOutput;
                    }
                    else if (state.HasFlag(EditorState.HoveringInput) || state.HasFlag(EditorState.HoveringOutput))
                    {
                        if (hoveredInput == null && hoveredOutput == null && state != EditorState.OutputToInput)
                            state = EditorState.None;
                    }

                    if (state == EditorState.HoveringUI)
                        state = EditorState.None;
                }
                else
                {
                    if (state != EditorState.MovingSelection)
                        state = EditorState.HoveringUI;
                }
            }
            #endregion

            #region Perform Editor State 

            if(state == EditorState.MovingCamera)
            {
                cam.target -= (currentMousePos - previousMousePos) * 1f / cam.zoom;
            }

            if(state == EditorState.MovingSelection)
            {
                foreach (DrawableComponent dc in currentProject.Simulation.SelectedComponents)
                {
                    dc.Position += (currentMousePos - previousMousePos) / cam.zoom;
                }
            }

            if(state == EditorState.RectangleSelecting)
            {
                Vector2 current = GetMousePositionInWorld();

                if(current.Y < startPos.Y && current.X > startPos.X)
                {
                    // First quadrant
                    selectionRec = new Rectangle(startPos.X, current.Y, current.X - startPos.X, startPos.Y - current.Y);
                }
                else if(current.Y < startPos.Y && current.X < startPos.X)
                {
                    // Second quadrant
                    selectionRec = new Rectangle(current.X, current.Y, startPos.X - current.X, startPos.Y - current.Y);
                }
                else if(current.Y > startPos.Y && current.X < startPos.X)
                {
                    // Third quadrant
                    selectionRec = new Rectangle(current.X, startPos.Y, startPos.X - current.X, current.Y - startPos.Y);
                }
                else
                {
                    // Fourth quadrant
                    selectionRec = new Rectangle(startPos.X, startPos.Y, current.X - startPos.X, current.Y - startPos.Y);
                }

                currentProject.Simulation.SelectComponentsInRectangle(selectionRec);
            }

            if(state == EditorState.HoveringOutput)
            {
                if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
                {
                    tempOutput = hoveredOutput;
                    state = EditorState.OutputToInput;
                }
            }

            if(state == EditorState.HoveringInput)
            {
                if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON))
                {
                    if (hoveredInput.Item2.Inputs[hoveredInput.Item1].Signal != null)
                    {
                        DrawableWire dw = (DrawableWire)hoveredInput.Item2.Inputs[hoveredInput.Item1].Signal;
                        hoveredInput.Item2.Inputs[hoveredInput.Item1].RemoveSignal();
                        dw.From.RemoveOutputWire(dw.FromIndex, dw);
                        currentProject.Simulation.RemoveWire(dw);
                    }
                }
            }

            if(state == EditorState.OutputToInput)
            {
                if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON) && hoveredInput != null)
                {
                    if (hoveredInput.Item2.Inputs[hoveredInput.Item1].Signal == null)
                    {
                        DrawableWire dw = new DrawableWire(tempOutput.Item2, hoveredInput.Item2, tempOutput.Item1, hoveredInput.Item1);
                        currentProject.Simulation.AddWire(dw);
                        tempOutput.Item2.AddOutputWire(tempOutput.Item1, dw);
                        hoveredInput.Item2.SetInputWire(hoveredInput.Item1, dw);
                        tempOutput = null;
                        state = EditorState.None;
                    }
                }
            }

            #endregion

            #region Delete Components
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_DELETE))
            {
                currentProject.Simulation.DeleteSelectedComponents();
            }
            #endregion

            #region Select Components With Clicking

            // If we press LMB, then we have the following scenario:
            // 
            // If we aren't hovering anything -> Clear Selected Components
            // If we are hovering something
            //      If we're holding shift -> Add Selected Component
            //      If not                 -> Clear Selected Components, Add Hovered Component to Selected
            //                                + Bonus: By setting state to MovingSelection, you can move any component you simply click.
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON) && state == EditorState.None)
            {
                
                if(hovered != null)
                {
                    if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT))
                    {
                        currentProject.Simulation.AddSelectedComponent(hovered);
                    }
                    else
                    {
                        if(!currentProject.Simulation.SelectedComponents.Contains(hovered))
                            currentProject.Simulation.ClearSelectedComponents();

                        currentProject.Simulation.AddSelectedComponent(hovered);
                        // Allow for moving instantly!
                        state = EditorState.MovingSelection;
                    }
                }
                else
                {
                    currentProject.Simulation.ClearSelectedComponents();
                }
            }

            #endregion

            previousMousePos = currentMousePos;
        }

        public override void Unload()
        {
            LogManager.DumpToFile();
        }

        public void CopySelectedComponentsNewOffsetPos(Vector2 offset)
        {
            WorkspaceContainer wc = new WorkspaceContainer(currentProject.Simulation.SelectedComponents);

            Tuple<List<DrawableComponent>, List<DrawableWire>> tup = wc.GenerateDrawables(currentProject.GetAllDescriptions(), offset);

            currentProject.Simulation.AllComponents.AddRange(tup.Item1);
            currentProject.Simulation.AllWires.AddRange(tup.Item2);

            currentProject.Simulation.ClearSelectedComponents();
            currentProject.Simulation.SelectedComponents.AddRange(tup.Item1);
        }

        public void SubmitUI()
        {
            yes = true;
            #region MAIN MENU BAR
            ImGui.BeginMainMenuBar();

            // File thing
            if (ImGui.BeginMenu("File"))
            {
                if(ImGui.MenuItem("New Project..."))
                {
                    // TODO: Modal for new project
                }

                if(ImGui.MenuItem("Open Project"))
                {
                    state = EditorState.SelectingProjectFile;
                    fd = new FileDialog(Utility.QUICKLINK_DIRS["Documents"], "Open Project", FileDialogType.SelectFile, new string[] { Utility.EXT_PROJ });
                }

                if (ImGui.BeginMenu("Include"))
                {
                    if(ImGui.MenuItem("IC Collection"))
                    {
                        state = EditorState.IncludeCollection;
                        fd = new FileDialog(Utility.QUICKLINK_DIRS["Documents"], "Include Collection", FileDialogType.SelectMultipleFiles, new string[] { Utility.EXT_ICCOLLECTION });
                    }
                    if(ImGui.MenuItem("IC Description"))
                    {
                        state = EditorState.IncludeDescription;
                        fd = new FileDialog(Utility.QUICKLINK_DIRS["Documents"], "Include Description", FileDialogType.SelectMultipleFiles, new string[] { Utility.EXT_IC });
                    }
                    ImGui.EndMenu();
                }

                ImGui.Separator();

                if (ImGui.MenuItem("Save"))
                {
                    string path = Utility.CreateProjectFilePath(currentProject.ProjectName);
                    currentProject.SaveProjectToFile(path);
                    SettingManager.SetSetting("latest-project", path);
                    SettingManager.SaveSettings();
                }

                if(ImGui.MenuItem("Save as..."))
                {
                    state = EditorState.SavingProjectFile;
                    fd = new FileDialog(Utility.PROJECTS_DIR, "Save Project As", FileDialogType.SaveFile, new string[] { Utility.EXT_PROJ });
                }

                if (ImGui.MenuItem("Settings"))
                {
                    editingSettings = true;
                }

                ImGui.EndMenu();
            }
            
            // Edit thing
            if(ImGui.BeginMenu("Edit"))
            {
                ImGui.MenuItem("Empty for now");
                ImGui.EndMenu();
            }

            // Simulation
            if (ImGui.BeginMenu("Simulation"))
            {
                ImGui.Checkbox("Simulating", ref _simulationOn);
                if (ImGui.Button("Update simulation"))
                {
                    currentProject.Simulation.UpdateLogic(GetMousePositionInWorld());
                }
                ImGui.EndMenu();
            }

            if(ImGui.BeginMenu("Integrated Circuits"))
            {
                if (ImGui.MenuItem("New..."))
                {
                    _makingIc = true;
                }

                if (ImGui.MenuItem("Open Folder..."))
                {
                    Utility.OpenPath(Utility.ASSETS_DIR);
                }

                if(ImGui.MenuItem("Show Window", "", icWindowOpen))
                {
                    icWindowOpen = !icWindowOpen;
                }

                if(ImGui.MenuItem("Create Collection"))
                {
                    creatingCollection = true;
                }

                ImGui.EndMenu();
            }

            ImGui.Separator();

            ImGui.TextDisabled(currentProject.ProjectName);

            ImGui.EndMainMenuBar();

            #endregion

            #region COMPONENTS WINDOW
            // Components window
            {
                ImGui.SetNextWindowPos(new Vector2(25, 50), ImGuiCond.Appearing);
                ImGui.SetNextWindowCollapsed(false, ImGuiCond.Appearing);
                ImGui.Begin("Components", ImGuiWindowFlags.AlwaysAutoResize);

                ImGui.Text("Input/Output");
                Vector2 buttonSize = Utility.BUTTON_DEFAULT_SIZE;

                ImGui.Button("SWITCH", buttonSize);
                if (ImGui.IsItemClicked())
                {
                    SetNewComponent(new DrawableCircuitSwitch(GetMousePositionInWorld(), true));
                }

                ImGui.Button("LAMP", buttonSize);
                if (ImGui.IsItemClicked())
                {
                    SetNewComponent(new DrawableCircuitLamp(GetMousePositionInWorld(), true));
                }

                ImGui.Separator();
                ImGui.Text("Logic Gates");

                // TODO: Do the rest of the buttons like this
                ImGui.Button("AND", buttonSize);
                if (ImGui.IsItemClicked())
                {
                    SetNewComponent(new DrawableLogicGate(GetMousePositionInWorld(), "AND", new ANDGateLogic(), true));       
                }
                ImGui.Button("NAND", buttonSize);
                if (ImGui.IsItemClicked())
                {
                    SetNewComponent(new DrawableLogicGate(GetMousePositionInWorld(), "NAND", new NANDGateLogic(), true));
                }

                ImGui.Button("OR", buttonSize);
                if (ImGui.IsItemClicked())
                {
                    SetNewComponent(new DrawableLogicGate(GetMousePositionInWorld(), "OR", new ORGateLogic(), true));
                }
                ImGui.Button("NOR", buttonSize);
                if (ImGui.IsItemClicked())
                {
                    SetNewComponent(new DrawableLogicGate(GetMousePositionInWorld(), "NOR", new NORGateLogic(), true));
                }

                ImGui.Button("XOR", buttonSize);
                if (ImGui.IsItemClicked())
                {
                    SetNewComponent(new DrawableLogicGate(GetMousePositionInWorld(), "XOR", new XORGateLogic(), true));
                }
                ImGui.Button("XNOR", buttonSize);
                if (ImGui.IsItemClicked())
                {
                    SetNewComponent(new DrawableLogicGate(GetMousePositionInWorld(), "XNOR", new XNORGateLogic(), true));
                }

                ImGui.Button("NOT", buttonSize);
                if (ImGui.IsItemClicked())
                {
                    SetNewComponent(new DrawableLogicGate(GetMousePositionInWorld(), "NOT", new NOTGateLogic(), true));
                }


                ImGui.End();
            }

            #endregion

            #region IC WINDOW

            if (icWindowOpen)
            {
                if (ImGui.Begin("ICs", ImGuiWindowFlags.AlwaysAutoResize))
                {
                    foreach(ICCollection collection in collections)
                    {
                        ImGui.Text(collection.Name);

                        foreach(ICDescription descr in collection.Descriptions)
                        {
                            ImGui.Button(descr.Name, Utility.BUTTON_DEFAULT_SIZE);
                            if (ImGui.IsItemClicked())
                            {
                                SetNewComponent(new DrawableIC(GetMousePositionInWorld(), descr.Name, descr, true));
                            }
                        }
                        ImGui.Separator();
                    }

                    ImGui.Text("In Project");
                    foreach(ICDescription desc in nonCollectionDescriptions)
                    {
                        ImGui.Button(desc.Name, Utility.BUTTON_DEFAULT_SIZE);
                        if (ImGui.IsItemClicked())
                        {
                            SetNewComponent(new DrawableIC(GetMousePositionInWorld(), desc.Name, desc, true));
                        }
                    }

                    ImGui.End();
                }
            }

            #endregion

            #region SET ID TO CIRCUIT IO WINDOW       
            if (currentProject.Simulation.SelectedComponents.Count == 1)
            {
                if(currentProject.Simulation.SelectedComponents[0] is DrawableCircuitSwitch || currentProject.Simulation.SelectedComponents[0] is DrawableCircuitLamp)
                {
                    if(ImGui.Begin("Setting IO ID", ImGuiWindowFlags.AlwaysAutoResize))
                    {
                        if (currentProject.Simulation.SelectedComponents[0] is DrawableCircuitSwitch)
                        {
                            DrawableCircuitSwitch dcs = (DrawableCircuitSwitch)currentProject.Simulation.SelectedComponents[0];
                            ImGui.InputText("ID", ref dcs.ID, 10);
                        }
                        if (currentProject.Simulation.SelectedComponents[0] is DrawableCircuitLamp)
                        {
                            DrawableCircuitLamp dcs = (DrawableCircuitLamp)currentProject.Simulation.SelectedComponents[0];
                            ImGui.InputText("ID", ref dcs.ID, 10);
                        }

                        ImGui.End();
                    }
                }
            }    
            #endregion

            #region CREATE NEW IC WINDOW
            if (_makingIc)
            {
                if (ICDescription.ValidateComponents(currentProject.Simulation.SelectedComponents))
                {
                    ImGui.OpenPopup("Create Integrated Circuit");

                    if (icInputs == null)
                    {
                        icInputs = currentProject.Simulation.SelectedComponents.Where(x => x.GetType() == typeof(DrawableCircuitSwitch)).ToArray();
                        icOutputs = currentProject.Simulation.SelectedComponents.Where(x => x.GetType() == typeof(DrawableCircuitLamp)).ToArray();
                        theRest = currentProject.Simulation.SelectedComponents.Where(x => x.GetType() != typeof(DrawableCircuitLamp) && x.GetType() != typeof(DrawableCircuitSwitch)).ToArray();
                    }
                }
                else
                {
                    _makingIc = false;
                    ImGui.OpenPopup("Error");
                }
            }

            Vector2 middleOfWindow = WindowSize / 2f;
            ImGui.SetNextWindowPos(middleOfWindow, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
            if (ImGui.BeginPopupModal("Error", ref yes, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text("Unable to create IC from selection.");
                ImGui.TextWrapped("Make sure you've named all IOs and selected all components!");
                ImGui.Separator();

                if (ImGui.Button("Close", Utility.BUTTON_DEFAULT_SIZE))
                {
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }

            //ImGui.SetNextWindowPos(WindowSize / 2f, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
            if (ImGui.BeginPopupModal("Create Integrated Circuit", ref yes, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize))
            {
                ImGui.InputText("Name", ref newIcName, 18);
                ImGui.Columns(2);
                ImGui.Text("Inputs");
                ImGui.SameLine();
                Utility.GuiHelpMarker("Here you can name your IC, as well as reorder your inputs and outputs.");
                ImGui.NextColumn();
                ImGui.Text("Outputs");
                ImGui.Columns(1);
                ImGui.Columns(2, "IOs");
                ImGui.Separator();

                for (int n = 0; n < icInputs.Length; n++)
                {
                    DrawableCircuitSwitch item = (DrawableCircuitSwitch)icInputs[n];
                    ImGui.Selectable(item.ID);

                    if (ImGui.IsItemActive() && !ImGui.IsItemHovered())
                    {
                        int n_next = n + (ImGui.GetMouseDragDelta(ImGuiMouseButton.Left).Y < 0 ? -1 : 1);
                        if (n_next >= 0 && n_next < icInputs.Length)
                        {
                            icInputs[n] = icInputs[n_next];
                            icInputs[n_next] = item;
                            ImGui.ResetMouseDragDelta();
                        }
                    }
                }

                ImGui.NextColumn();

                for (int n = 0; n < icOutputs.Length; n++)
                {
                    DrawableCircuitLamp item = (DrawableCircuitLamp)icOutputs[n];
                    ImGui.Selectable(item.ID);

                    if (ImGui.IsItemActive() && !ImGui.IsItemHovered())
                    {
                        int n_next = n + (ImGui.GetMouseDragDelta(0).Y < 0 ? -1 : 1);
                        if (n_next >= 0 && n_next < icOutputs.Length)
                        {
                            icOutputs[n] = icOutputs[n_next];
                            icOutputs[n_next] = item;
                            ImGui.ResetMouseDragDelta();
                        }
                    }
                }

                ImGui.NextColumn();
                ImGui.Columns(1);
                ImGui.Separator();

                if (ImGui.Button("Close", Utility.BUTTON_DEFAULT_SIZE))
                {
                    _makingIc = false;
                    icInputs = null;
                    ImGui.CloseCurrentPopup();
                }

                ImGui.SameLine();

                if (ImGui.Button("Create", Utility.BUTTON_DEFAULT_SIZE))
                {
                    List<DrawableComponent> comps = icInputs.ToList();
                    comps.AddRange(icOutputs);
                    comps.AddRange(theRest);

                    ICDescription icd = new ICDescription(comps);
                    string path = icd.SaveToFile(newIcName);
                    //currentProject.Simulation.AddComponent(new DrawableIC(new Vector2(100, 100), newIcName, icd, true));
                    IncludeDescriptions(path);
                    newIcName = "";
                    _makingIc = false;
                    icInputs = null;
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }
            #endregion

            #region DEBUG WINDOW

            {
                ImGui.Begin("Debug", ImGuiWindowFlags.AlwaysAutoResize);
                ImGui.Text($"Editor State: {state.ToString()}");
                ImGui.Separator();

                int switches = currentProject.Simulation.SelectedComponents.Where(x => x.GetType() == typeof(DrawableCircuitSwitch)).Count();
                int lamps = currentProject.Simulation.SelectedComponents.Where(x => x.GetType() == typeof(DrawableCircuitLamp)).Count();
                int gates = currentProject.Simulation.SelectedComponents.Where(x => x.GetType() == typeof(DrawableLogicGate)).Count();
                int ics = currentProject.Simulation.SelectedComponents.Where(x => x.GetType() == typeof(DrawableIC)).Count();

                if (ImGui.CollapsingHeader($"Selected Components", ImGuiTreeNodeFlags.CollapsingHeader))
                {
                    ImGui.Text($"Switches: {switches}");
                    ImGui.Text($"Lamps: {lamps}");
                    ImGui.Text($"Gates: {gates}");
                    ImGui.Text($"ICs: {ics}");
                    ImGui.Separator();
                    ImGui.Text($"Total: {currentProject.Simulation.SelectedComponents.Count}");
                }
                ImGui.End();
            }

            #endregion

            #region SETTINGS WINDOW

            if (editingSettings)
            {
                if (ImGui.Begin("Settings", ImGuiWindowFlags.AlwaysAutoResize))
                {
                    if (ImGui.BeginTabBar("Settings"))
                    {
                        if (ImGui.BeginTabItem("Appearance"))
                        {
                            // IO Vertical distance
                            Utility.GuiSettingFloatSlider("IO Vertical Distance", "io-v-dist", 14f, 1, 50);

                            // IO Horizontal distance
                            Utility.GuiSettingFloatSlider("IO Horizontal Distance", "io-h-dist", 10f, 1, 50);

                            // IO Size
                            Utility.GuiSettingFloatSlider("IO Size", "io-size", 6, 1, 20);
                        }

                        ImGui.EndTabBar();
                    }

                    if (ImGui.Button("Save", Utility.BUTTON_DEFAULT_SIZE))
                    {
                        SettingManager.SaveSettings();
                        editingSettings = false;
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Close", Utility.BUTTON_DEFAULT_SIZE))
                    {
                        editingSettings = false;
                    }
                    ImGui.End();
                }
            }

            #endregion

            #region CREATE COLLECTION WINDOW
            
            if (creatingCollection)
            {
                ImGui.OpenPopup("Create IC Collection");
                creatingCollection = false;
                selectedIcs = new List<ICDescription>();
                newCollectionName = "";
            }

            ImGui.SetNextWindowPos(WindowSize / 2f, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
            if (ImGui.BeginPopupModal("Create IC Collection", ref yes, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text("Select integrated components.");

                Vector2 size = new Vector2(300, 200);

                if(ImGui.BeginChild("ics", size, true))
                {
                    foreach(ICCollection coll in collections)
                    {
                        ImGui.TextDisabled(coll.Name);

                        foreach(ICDescription icd in coll.Descriptions)
                        {
                            if(ImGui.Selectable(icd.Name, selectedIcs.Contains(icd)))
                            {
                                if (!selectedIcs.Contains(icd))
                                {
                                    selectedIcs.Add(icd);
                                }
                                else
                                {
                                    selectedIcs.Remove(icd);
                                }
                            }
                        }
                    }

                    ImGui.TextDisabled("In Project");

                    foreach (ICDescription icd in nonCollectionDescriptions)
                    {
                        if (ImGui.Selectable(icd.Name, selectedIcs.Contains(icd)))
                        {
                            if (!selectedIcs.Contains(icd))
                            {
                                selectedIcs.Add(icd);
                            }
                            else
                            {
                                selectedIcs.Remove(icd);
                            }
                        }
                    }

                    ImGui.EndChild();
                }

                ImGui.InputText("Collection name", ref newCollectionName, 20);
                ImGui.SameLine();
                if (ImGui.Button("Close"))
                {
                    ImGui.CloseCurrentPopup();
                }
                ImGui.SameLine();
                if (ImGui.Button("Create"))
                {
                    ICCollection icc = new ICCollection(newCollectionName, selectedIcs);

                    using(StreamWriter sw = new StreamWriter(Utility.ASSETS_DIR + @"/" + icc.Name + Utility.EXT_ICCOLLECTION))
                    {
                        sw.Write(JsonConvert.SerializeObject(icc));
                    }
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }

            #endregion

            if (fd?.Done() == true)
            {
                if (fd.SelectedFiles.Count > 0)
                {
                    switch (state)
                    {
                        case EditorState.SelectingProjectFile:
                            SetProject(LogiXProject.LoadFromFile(fd.SelectedFiles[0]));
                            break;

                        case EditorState.IncludeCollection:
                            foreach(string collection in fd.SelectedFiles)
                            {
                                IncludeCollection(collection);
                            }
                            break;

                        case EditorState.IncludeDescription:
                            foreach (string description in fd.SelectedFiles)
                            {
                                IncludeDescriptions(description);
                            }
                            break;

                        case EditorState.SavingProjectFile:
                            currentProject.SaveProjectToFile(fd.SelectedFiles[0]);
                            SettingManager.SetSetting("latest-project", fd.SelectedFiles[0]);
                            SettingManager.SaveSettings();
                            break;
                    }
                    state = EditorState.None;
                }
                fd = null;
            }
        }

        public void IncludeCollection(string coll)
        {
            currentProject.IncludeCollection(coll);

            SetProject(currentProject);
        }

        public void IncludeDescriptions(string description)
        {
            currentProject.IncludeDescription(description);

            SetProject(currentProject);
        }
    }
}
