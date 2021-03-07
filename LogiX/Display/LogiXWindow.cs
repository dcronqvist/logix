using ImGuiNET;
using LogiX.Assets;
using LogiX.Circuits.Drawables;
using LogiX.Circuits.Integrated;
using LogiX.Circuits.Logic;
using LogiX.Logging;
using LogiX.Settings;
using LogiX.Simulation;
using LogiX.UI;
using LogiX.Utils;
using Raylib_cs;
using System;
using System.Collections.Generic;
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
        OutputToInput
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

        // The logic simulation
        Simulator sim;

        // Controller for ImGui.NET
        ImGUIController controller;

        bool _simulationOn;
        bool _makingIc;
        string newIcName = "";
        bool yes = true;
        string[] arr = { "dani", "saga", "carl", "benji" };

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
            sim = new Simulator();
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
            AssetManager.LoadAllAssets();
        }

        public Vector2 GetMousePositionInWorld()
        {
            return TopLeftCorner + currentMousePos / cam.zoom;
        }

        public void RenderGrid()
        {
            // TODO: Move this to more suitable spot, also try to shorten this down.
            int windowWidth = int.Parse(SettingManager.GetSetting("window-width", "1440"));
            int windowHeight = int.Parse(SettingManager.GetSetting("window-height", "768"));
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
            sim.Render(GetMousePositionInWorld());
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
                sim.UpdateLogic(GetMousePositionInWorld());
            // Should always update all updateable components though, like being able
            // to turn off and on switches during sim-off.
            sim.Update(GetMousePositionInWorld());

            // Get the hovered component, is null if nothing is hovered.
            DrawableComponent hovered = sim.GetComponentFromPosition(GetMousePositionInWorld());
            // Hovering inputs & outputs
            Tuple<int, DrawableComponent> hoveredInput = sim.GetComponentAndInputFromPos(GetMousePositionInWorld());
            Tuple<int, DrawableComponent> hoveredOutput = sim.GetComponentAndOutputFromPos(GetMousePositionInWorld());

            // Allow for camera zooming with the mouse wheel.
            #region Mouse Wheel Camera Zoom
            if (Raylib.GetMouseWheelMove() > 0)
            {
                cam.zoom *= 1.05f;
            }
            if (Raylib.GetMouseWheelMove() < 0)
            {
                cam.zoom *= 1/ 1.05f;
            }
            #endregion

            #region Decide Which Editor State
            if (!ImGui.GetIO().WantCaptureMouse)
            {
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
                    else if (sim.SelectedComponents.Contains(hovered))
                    {
                        state = EditorState.MovingSelection;
                    }
                }
                if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON) && (state == EditorState.MovingSelection || state == EditorState.RectangleSelecting))
                {
                    state = EditorState.None;
                }

                if (state == EditorState.None)
                {
                    if (hoveredInput != null)
                        state = EditorState.HoveringInput;
                    else if (hoveredOutput != null)
                        state = EditorState.HoveringOutput;
                }
                else if(state.HasFlag(EditorState.HoveringInput) || state.HasFlag(EditorState.HoveringOutput))
                {
                    if(hoveredInput == null && hoveredOutput == null && state != EditorState.OutputToInput)
                        state = EditorState.None;
                }

                if (state == EditorState.HoveringUI)
                    state = EditorState.None;
            }
            else
            {
                state = EditorState.HoveringUI;
            }
            #endregion

            #region Perform Editor State 

            if(state == EditorState.MovingCamera)
            {
                cam.target -= (currentMousePos - previousMousePos) * 1f / cam.zoom;
            }

            if(state == EditorState.MovingSelection)
            {
                foreach (DrawableComponent dc in sim.SelectedComponents)
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

                sim.SelectComponentsInRectangle(selectionRec);
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
                        sim.RemoveWire(dw);
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
                        sim.AddWire(dw);
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
                sim.DeleteSelectedComponents();
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
                        sim.AddSelectedComponent(hovered);
                    }
                    else
                    {
                        if(!sim.SelectedComponents.Contains(hovered))
                            sim.ClearSelectedComponents();

                        sim.AddSelectedComponent(hovered);
                        // Allow for moving instantly!
                        state = EditorState.MovingSelection;
                    }
                }
                else
                {
                    sim.ClearSelectedComponents();
                }
            }

            #endregion

            /*
            Tuple<int, DrawableComponent> tempintup = sim.GetComponentAndInputFromPos(mousePos);
            Tuple<int, DrawableComponent> tempouttup = sim.GetComponentAndOutputFromPos(mousePos);

            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
            {
                if(hovered != null)
                {
                    if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT))
                    {
                        sim.AddSelectedComponent(hovered);
                    }
                    else 
                    {
                        sim.ClearSelectedComponents();
                        sim.AddSelectedComponent(hovered);
                    }
                    lastSelectedComponent = hovered;
                }

                // Pressing output first and then input
                if (outtup == null && tempintup == null)
                {
                    if(tempouttup != null)
                    {
                        outtup = tempouttup;
                    }
                }
                     
                if(tempintup != null && outtup != null)
                {
                    if(tempintup.Item2.Inputs[tempintup.Item1].Signal == null)
                    {
                        DrawableWire dw = new DrawableWire(outtup.Item2, tempintup.Item2, outtup.Item1, tempintup.Item1);
                        sim.AddWire(dw);
                        outtup.Item2.AddOutputWire(outtup.Item1, dw);
                        tempintup.Item2.SetInputWire(tempintup.Item1, dw);
                        outtup = null;
                        return;
                    }
                }
            }
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON))
            {
                if(tempintup != null)
                {
                    DrawableWire dw = (DrawableWire)tempintup.Item2.Inputs[tempintup.Item1].Signal;
                    tempintup.Item2.Inputs[tempintup.Item1].RemoveSignal();
                    dw.From.RemoveOutputWire(dw.FromIndex, dw);
                    sim.RemoveWire(dw);
                }
            }

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_ESCAPE))
            {
                sim.ClearSelectedComponents();
                outtup = null;
            }
            */

            previousMousePos = currentMousePos;
        }

        public override void Unload()

        {
            LogManager.DumpToFile();
        }

        public void SubmitUI()
        {
            #region MAIN MENU BAR
            ImGui.BeginMainMenuBar();

            // File thing
            if (ImGui.BeginMenu("File"))
            {
                ImGui.MenuItem("Empty for now");
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
                    sim.UpdateLogic(GetMousePositionInWorld());
                }
                ImGui.EndMenu();
            }

            if(ImGui.BeginMenu("Integrated Circuits"))
            {
                if (ImGui.MenuItem("New..."))
                {
                    _makingIc = true;
                }

                List<ICDescription> ics = AssetManager.GetAllAssetsOfType<ICDescription>();
                foreach(ICDescription ic in ics)
                {
                    if (ImGui.MenuItem(ic.Name))
                    {
                        sim.AddComponent(new DrawableIC(new Vector2(100, 100), ic.Name, ic));
                    }
                }

                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();

            #endregion

            #region COMPONENTS WINDOW
            // Components window
            {
                ImGui.SetNextWindowPos(new Vector2(25, 50), ImGuiCond.Appearing);
                ImGui.SetNextWindowCollapsed(false, ImGuiCond.Appearing);
                ImGui.Begin("Components", ImGuiWindowFlags.AlwaysAutoResize); 

                if (ImGui.Button("Switch"))
                {
                    sim.AddComponent(new DrawableCircuitSwitch(GetMousePositionInWorld() + new Vector2(200, 0)));
                }

                if (ImGui.Button("Lamp"))
                {
                    sim.AddComponent(new DrawableCircuitLamp(GetMousePositionInWorld() + new Vector2(200, 0)));
                }

                if (ImGui.Button("AND"))
                {
                    sim.AddComponent(new DrawableLogicGate(GetMousePositionInWorld() + new Vector2(200, 0), "AND", new ANDGateLogic()));
                }

                if (ImGui.Button("XOR"))
                {
                    sim.AddComponent(new DrawableLogicGate(GetMousePositionInWorld() + new Vector2(200, 0), "XOR", new XORGateLogic()));
                }

                if (ImGui.Button("OR"))
                {
                    sim.AddComponent(new DrawableLogicGate(GetMousePositionInWorld() + new Vector2(200, 0), "OR", new ORGateLogic()));
                }

                if (ImGui.Button("NOR"))
                {
                    sim.AddComponent(new DrawableLogicGate(GetMousePositionInWorld() + new Vector2(200, 0), "NOR", new NORGateLogic()));
                }

                ImGui.End();
            }

            #endregion

            #region SET ID TO CIRCUIT IO WINDOW       
            if(sim.SelectedComponents.Count == 1)
            {
                if(sim.SelectedComponents[0] is DrawableCircuitSwitch || sim.SelectedComponents[0] is DrawableCircuitLamp)
                {
                    if(ImGui.Begin("Setting IO ID", ImGuiWindowFlags.AlwaysAutoResize))
                    {
                        if (sim.SelectedComponents[0] is DrawableCircuitSwitch)
                        {
                            DrawableCircuitSwitch dcs = (DrawableCircuitSwitch)sim.SelectedComponents[0];
                            ImGui.InputText("ID", ref dcs.ID, 10);
                        }
                        if (sim.SelectedComponents[0] is DrawableCircuitLamp)
                        {
                            DrawableCircuitLamp dcs = (DrawableCircuitLamp)sim.SelectedComponents[0];
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
                ImGui.OpenPopup("Create Integrated Circuit");

                if (icInputs == null)
                {
                    icInputs = sim.SelectedComponents.Where(x => x.GetType() == typeof(DrawableCircuitSwitch)).ToArray();
                    icOutputs = sim.SelectedComponents.Where(x => x.GetType() == typeof(DrawableCircuitLamp)).ToArray();
                    theRest = sim.SelectedComponents.Where(x => x.GetType() != typeof(DrawableCircuitLamp) && x.GetType() != typeof(DrawableCircuitSwitch)).ToArray();
                }
            }

            //ImGui.SetNextWindowPos(WindowSize / 2f, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
            if (ImGui.BeginPopupModal("Create Integrated Circuit", ref yes, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoTitleBar))
            {
                ImGui.InputText("Name", ref newIcName, 18);
                ImGui.Columns(2);
                ImGui.Text("Inputs");
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
                        int n_next = n + (ImGui.GetMouseDragDelta(0).Y < 0 ? -1 : 1);
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

                if (ImGui.Button("Close", new Vector2(120, 0)))
                {
                    _makingIc = false;
                    icInputs = null;
                    ImGui.CloseCurrentPopup();
                }

                ImGui.SameLine();

                if (ImGui.Button("Create", new Vector2(120, 0)))
                {
                    if (ICDescription.ValidateComponents(sim.SelectedComponents))
                    {
                        List<DrawableComponent> comps = icInputs.ToList();
                        comps.AddRange(icOutputs);
                        comps.AddRange(theRest);

                        ICDescription icd = new ICDescription(comps);
                        icd.SaveToFile(newIcName);
                        sim.AddComponent(new DrawableIC(new Vector2(100, 100), newIcName, icd));
                        newIcName = "";
                        _makingIc = false;
                        icInputs = null;
                        ImGui.CloseCurrentPopup();
                    }
                    else
                    {
                        ImGui.OpenPopup("Error", ImGuiPopupFlags.AnyPopup);
                    }
                }
                Vector2 middleOfWindow = WindowSize / 2f;
                ImGui.SetNextWindowPos(middleOfWindow, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
                if (ImGui.BeginPopupModal("Error", ref yes, ImGuiWindowFlags.AlwaysAutoResize))
                {
                    ImGui.Text("Unable to create IC from selection.");
                    ImGui.TextWrapped("Make sure you've named all IOs and selected all components!");
                    ImGui.Separator();

                    if (ImGui.Button("Close"))
                    {
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.EndPopup();
                }

                ImGui.EndPopup();
            }
            #endregion

            #region DEBUG WINDOW

            {
                ImGui.Begin("Debug", ImGuiWindowFlags.AlwaysAutoResize);
                ImGui.Text($"Editor State: {state.ToString()}");
                ImGui.Separator();

                int switches = sim.SelectedComponents.Where(x => x.GetType() == typeof(DrawableCircuitSwitch)).Count();
                int lamps = sim.SelectedComponents.Where(x => x.GetType() == typeof(DrawableCircuitLamp)).Count();
                int gates = sim.SelectedComponents.Where(x => x.GetType() == typeof(DrawableLogicGate)).Count();
                int ics = sim.SelectedComponents.Where(x => x.GetType() == typeof(DrawableIC)).Count();

                if (ImGui.CollapsingHeader($"Selected Components", ImGuiTreeNodeFlags.CollapsingHeader))
                {
                    ImGui.Text($"Switches: {switches}");
                    ImGui.Text($"Lamps: {lamps}");
                    ImGui.Text($"Gates: {gates}");
                    ImGui.Text($"ICs: {ics}");
                    ImGui.Separator();
                    ImGui.Text($"Total: {sim.SelectedComponents.Count}");
                }
                ImGui.End();
            }

            #endregion
        }
    }
}
