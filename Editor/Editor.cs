using LogiX.Components;
using LogiX.SaveSystem;
using Newtonsoft.Json;

namespace LogiX.Editor;

public class Editor : Application
{
    // Editor states
    Camera2D editorCamera;
    EditorState editorState;
    Simulator simulator;

    // GATES
    IGateLogic[] availableGateLogics;

    // KEY COMBINATIONS
    KeyboardKey primaryKeyMod;

    // MAIN MENU BAR ACTIONS
    List<Tuple<string, List<Tuple<string, Func<bool>, KeyboardKey?, KeyboardKey?, Action>>>> mainMenuButtons;

    // VARIABLES FOR TEMPORARY STUFF
    ComponentInput? hoveredInput;
    ComponentOutput? hoveredOutput;
    Component? hoveredComponent;
    Vector2 recSelectFirstCorner;

    ComponentOutput? connectFrom;

    CircuitDescription? copiedCircuit;
    List<SLDescription> icSwitches;
    Dictionary<SLDescription, int> icSwitchGroup;
    Dictionary<SLDescription, int> icLampGroup;
    List<SLDescription> icLamps;
    string icName;
    string error;
    bool encounteredError;

    bool showDemo;

    // UI VARIABLES
    int newComponentBits;
    bool newComponentMultibit;

    public override void Initialize()
    {
        mainMenuButtons = new List<Tuple<string, List<Tuple<string, Func<bool>, KeyboardKey?, KeyboardKey?, Action>>>>();
#if OSX
        this.primaryKeyMod = KeyboardKey.KEY_LEFT_SUPER;
#else
        this.primaryKeyMod = KeyboardKey.KEY_LEFT_CONTROL;
#endif

        this.OnWindowResized += (width, height) =>
        {
            Vector2 windowSize = new Vector2(width, height);
            this.editorCamera = new Camera2D(windowSize / 2.0f, Vector2.Zero, 0f, 1f);
            Settings.SetSetting<int>("windowWidth", width);
            Settings.SetSetting<int>("windowHeight", height);
            Settings.SaveSettings();
        };

        availableGateLogics = new IGateLogic[] {
            new ANDLogic(),
            new NANDLogic(),
            new ORLogic(),
            new NORLogic(),
            new XORLogic(),
            new NOTLogic()
        };
        encounteredError = false;
        showDemo = false;
    }

    public override void LoadContent()
    {
        Vector2 windowSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        this.editorCamera = new Camera2D(windowSize / 2.0f, Vector2.Zero, 0f, 1f);
        this.editorState = EditorState.None;

        this.simulator = new Simulator();

        Util.OpenSans = Raylib.LoadFontEx($"{Directory.GetCurrentDirectory()}/assets/opensans-bold.ttf", 100, Enumerable.Range(0, 1000).ToArray(), 1000);
        Raylib.SetTextureFilter(Util.OpenSans.texture, TextureFilter.TEXTURE_FILTER_TRILINEAR);

        Settings.LoadSettings();

        // ASSIGNING KEYCOMBO ACTIONS
        AddNewMainMenuItem("File", "Save", () => true, this.primaryKeyMod, KeyboardKey.KEY_S, null);
        AddNewMainMenuItem("Edit", "Copy", () => this.simulator.SelectedComponents.Count > 0, this.primaryKeyMod, KeyboardKey.KEY_C, EditCopy);
        AddNewMainMenuItem("Edit", "Paste", () => this.copiedCircuit != null, this.primaryKeyMod, KeyboardKey.KEY_V, EditPaste);
        AddNewMainMenuItem("Edit", "Create IC from Clipboard", () => this.copiedCircuit != null, this.primaryKeyMod, KeyboardKey.KEY_I, EditCreateIC);
    }

    public void AddNewMainMenuItem(string mainButton, string actionButtonName, Func<bool> enabled, KeyboardKey? hold, KeyboardKey? press, Action action)
    {
        if (!this.mainMenuButtons.Exists(x => x.Item1 == mainButton))
        {
            this.mainMenuButtons.Add(new Tuple<string, List<Tuple<string, Func<bool>, KeyboardKey?, KeyboardKey?, Action>>>(mainButton, new List<Tuple<string, Func<bool>, KeyboardKey?, KeyboardKey?, Action>>()));
        }

        Tuple<string, List<Tuple<string, Func<bool>, KeyboardKey?, KeyboardKey?, Action>>> mainMenuButton = this.mainMenuButtons.Find(x => x.Item1 == mainButton);

        mainMenuButton.Item2.Add(new Tuple<string, Func<bool>, KeyboardKey?, KeyboardKey?, Action>(actionButtonName, enabled, hold, press, action));
    }

    public void EditCopy()
    {
        copiedCircuit = new CircuitDescription(this.simulator.SelectedComponents);
        icSwitchGroup = new Dictionary<SLDescription, int>();
        icSwitches = copiedCircuit.GetSwitches();
        for (int i = 0; i < icSwitches.Count; i++)
        {
            icSwitchGroup.Add(icSwitches[i], i);
        }
        icLampGroup = new Dictionary<SLDescription, int>();
        icLamps = copiedCircuit.GetLamps();
        for (int i = 0; i < icLamps.Count; i++)
        {
            icLampGroup.Add(icLamps[i], i);
        }
        icName = "";
    }

    public void EditPaste()
    {
        if (copiedCircuit != null)
        {
            this.simulator.ClearSelection();
            Tuple<List<Component>, List<Wire>> newStuff = copiedCircuit.CreateComponentsAndWires(this.editorCamera.target);
            this.simulator.AddComponents(newStuff.Item1);
            this.simulator.AddWires(newStuff.Item2);

            foreach (Component c in newStuff.Item1)
            {
                this.simulator.SelectComponent(c);
            }
        }
    }

    public void EditCreateIC()
    {
        if (this.copiedCircuit.ValidForIC())
        {
            this.editorState = EditorState.MakingIC;
        }
        else
        {
            this.EditorError("Cannot create integrated circuit from selected components, \nsince some switches or lamps have no identifier.");
        }
    }

    public override void SubmitUI()
    {
        // MAIN MENU BAR
        ImGui.BeginMainMenuBar();

        foreach (Tuple<string, List<Tuple<string, Func<bool>, KeyboardKey?, KeyboardKey?, Action>>> tup in this.mainMenuButtons)
        {
            if (ImGui.BeginMenu(tup.Item1))
            {
                foreach (Tuple<string, Func<bool>, KeyboardKey?, KeyboardKey?, Action> inner in tup.Item2)
                {
                    if (ImGui.MenuItem(inner.Item1, (inner.Item3 != null && inner.Item4 != null) ? UserInput.KeyComboString(inner.Item3.Value, inner.Item4.Value) : null, false, inner.Item2()))
                    {
                        if (inner.Item5 != null)
                        {
                            inner.Item5();
                        }
                    }
                }

                ImGui.EndMenu();
            }
        }

        ImGui.EndMainMenuBar();

        // DEMO WINDOW

        ImGui.ShowDemoWindow(ref this.showDemo);

        // SIDEBAR

        ImGui.SetNextWindowPos(new Vector2(0, 22), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(120, Raylib.GetScreenHeight() - 19), ImGuiCond.Always);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.3f, 0.3f, 0.3f, 0.8f));
        ImGui.Begin("Sidebar", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoNavInputs);
        ImGui.End();
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();

        // TESTING WINDOW

        ImGui.Begin("Components", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysVerticalScrollbar | ImGuiWindowFlags.NoNavInputs);
        ImGui.Text("Gates");
        for (int i = 0; i < this.availableGateLogics.Length; i++)
        {
            CreateNewGateButton(this.availableGateLogics[i]);
        }
        ImGui.Separator();
        ImGui.Text("I/O");

        CreateNewComponentButton("Switch", true, (bits, multibit, worldPos) =>
        {
            return new Switch(bits, worldPos);
        }, 1, false);
        CreateNewComponentButton("Button", false, (bits, multibit, worldPos) =>
        {
            return new Button(bits, worldPos);
        }, 1, false);
        CreateNewComponentButton("Lamp", true, (bits, multibit, worldPos) =>
        {
            return new Lamp(bits, worldPos);
        }, 1, false);
        CreateNewComponentButton("Hex Viewer", true, (bits, multibit, worldPos) =>
        {
            return new HexViewer(bits, multibit, worldPos);
        }, 4, false);

        ImGui.End();

        if (this.editorState == EditorState.MakingIC)
        {
            ImGui.OpenPopup("Create Integrated Circuit");
        }

        // MAKING IC WINDOW
        Vector2 windowSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        ImGui.SetNextWindowPos(windowSize / 2, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
        Vector2 popupSize = new Vector2(80) * 4f;
        ImGui.SetNextWindowSizeConstraints(popupSize, popupSize);
        ImGui.SetNextWindowSize(popupSize);
        if (ImGui.BeginPopupModal("Create Integrated Circuit"))
        {
            ImGui.InputText("Circuit name", ref this.icName, 25);
            ImGui.Separator();

            ImGui.Columns(2);
            ImGui.Text("Inputs");
            ImGui.NextColumn();
            ImGui.Text("Outputs");
            ImGui.Separator();
            ImGui.NextColumn();

            for (int i = 0; i < icSwitches.Count; i++)
            {
                SLDescription sw = icSwitches[i];
                ImGui.PushID(sw.ID);
                ImGui.SetNextItemWidth(80);
                int gr = this.icSwitchGroup[sw];
                ImGui.InputInt("", ref gr, 1, 1);
                this.icSwitchGroup[sw] = gr;
                ImGui.PopID();
                ImGui.SameLine();
                int group = this.icSwitchGroup[sw];
                ImGui.Selectable(sw.Name, false, ImGuiSelectableFlags.DontClosePopups);

                if (ImGui.IsItemActive() && !ImGui.IsItemHovered())
                {
                    int nNext = i + (ImGui.GetMouseDragDelta(ImGuiMouseButton.Left).Y < 0f ? -1 : 1);
                    if (nNext >= 0 && nNext < icSwitches.Count)
                    {
                        icSwitches[i] = icSwitches[nNext];
                        icSwitches[nNext] = sw;
                    }
                }
            }

            ImGui.NextColumn();

            for (int i = 0; i < icLamps.Count; i++)
            {
                SLDescription sw = icLamps[i];
                ImGui.PushID(sw.ID);
                ImGui.SetNextItemWidth(80);
                int gr = this.icLampGroup[sw];
                ImGui.InputInt("", ref gr, 1, 1);
                this.icLampGroup[sw] = gr;
                ImGui.PopID();
                ImGui.SameLine();
                int group = this.icLampGroup[sw];
                ImGui.Selectable(sw.Name, false, ImGuiSelectableFlags.DontClosePopups);

                if (ImGui.IsItemActive() && !ImGui.IsItemHovered())
                {
                    int nNext = i + (ImGui.GetMouseDragDelta(ImGuiMouseButton.Left).Y < 0f ? -1 : 1);
                    if (nNext >= 0 && nNext < icLamps.Count)
                    {
                        icLamps[i] = icLamps[nNext];
                        icLamps[nNext] = sw;
                    }
                }
            }

            ImGui.Columns(1);
            ImGui.Separator();

            if (ImGui.Button("Create"))
            {
                if (copiedCircuit != null)
                {
                    if (copiedCircuit.ValidForIC())
                    {
                        List<List<string>> inputOrder = new List<List<string>>();
                        List<List<string>> outputOrder = new List<List<string>>();

                        int max = 0;
                        foreach (KeyValuePair<SLDescription, int> kvp in this.icSwitchGroup)
                        {
                            max = Math.Max(max, kvp.Value);
                        }

                        for (int i = 0; i <= max; i++)
                        {
                            if (this.icSwitchGroup.ContainsValue(i))
                            {
                                List<SLDescription> inGroup = this.icSwitchGroup.Where(x => x.Value == i).Select(x => x.Key).ToList();
                                inputOrder.Add(inGroup.Select(x => x.ID).ToList());
                            }
                        }

                        foreach (KeyValuePair<SLDescription, int> kvp in this.icLampGroup)
                        {
                            max = Math.Max(max, kvp.Value);
                        }

                        for (int i = 0; i <= max; i++)
                        {
                            if (this.icLampGroup.ContainsValue(i))
                            {
                                List<SLDescription> inGroup = this.icLampGroup.Where(x => x.Value == i).Select(x => x.Key).ToList();
                                outputOrder.Add(inGroup.Select(x => x.ID).ToList());
                            }
                        }

                        ICDescription icd = new ICDescription(this.icName, copiedCircuit, inputOrder, outputOrder);
                        Console.WriteLine(JsonConvert.SerializeObject(icd, Formatting.Indented));

                        this.simulator.AddComponent(icd.ToComponent());
                    }
                    else
                    {
                        Console.WriteLine("circuit contains unnamed switches or lamps.");
                    }
                }

                ImGui.CloseCurrentPopup();
                this.editorState = EditorState.None;
            }
            ImGui.EndPopup();
        }

        // DEBUG WINDOW

        ImGui.Begin("Debug stuff");
        ImGui.Text("Mouse Position in Window:");
        ImGui.Text(UserInput.GetMousePositionInWindow().ToString());
        ImGui.Text("Camera Position:");
        ImGui.Text(this.editorCamera.target.ToString());
        ImGui.Text("Mouse Position in World:");
        ImGui.Text(UserInput.GetMousePositionInWorld(this.editorCamera).ToString());
        ImGui.Text("Camera View Size:");
        ImGui.Text(UserInput.GetViewSize(this.editorCamera).ToString());
        ImGui.Text("Current editor state:");
        ImGui.Text(this.editorState.ToString());
        ImGui.Text("IO Want Keyboard:");
        ImGui.Text(ImGui.GetIO().WantCaptureKeyboard.ToString());
        ImGui.Text("IO Want Mouse:");
        ImGui.Text(ImGui.GetIO().WantCaptureMouse.ToString());

        ImGui.Separator();
        ImGui.Text("Settings");

        Dictionary<string, Setting> settings = Settings.GetAllSettings();
        foreach (KeyValuePair<string, Setting> setting in settings)
        {
            ImGui.Text(setting.Key + ": " + setting.Value.Value.ToString());
        }

        ImGui.End();

        // If single selecting a component
        if (this.simulator.SelectedComponents.Count == 1)
        {
            Component c = this.simulator.SelectedComponents[0];
            c.OnSingleSelectedSubmitUI();
        }

        if (this.encounteredError)
        {
            ImGui.OpenPopup("Error");
        }

        // Error popup
        ImGui.SetNextWindowPos(windowSize / 2, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
        if (ImGui.BeginPopupModal("Error", ref this.encounteredError, ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text(this.error);

            if (ImGui.Button("OK"))
            {
                this.encounteredError = false;
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
    }

    public void CreateNewComponentButton(string text, bool multibitPop, Func<int, bool, Vector2, Component> createComponent, int defaultBits, bool defaultMultibit)
    {
        Vector2 buttonSize = new Vector2(94, 25);
        ImGui.Button(text, buttonSize);
        if (ImGui.IsItemClicked())
        {
            // Create new component
            Component comp = createComponent(defaultBits, defaultMultibit, UserInput.GetMousePositionInWorld(this.editorCamera));
            this.NewComponent(comp);
        }

        if (multibitPop)
        {
            if (ImGui.BeginPopupContextItem(text))
            {
                ImGui.SetNextItemWidth(80);
                ImGui.InputInt("Bits", ref newComponentBits);
                ImGui.Checkbox("Multibit", ref newComponentMultibit);
                ImGui.Separator();
                ImGui.Button("Create");

                if (ImGui.IsItemClicked())
                {
                    // Create new component
                    Component comp = createComponent(newComponentBits, newComponentMultibit, UserInput.GetMousePositionInWorld(this.editorCamera));
                    this.NewComponent(comp);
                }

                ImGui.EndPopup();
            }
        }
    }

    public void CreateNewGateButton(IGateLogic logic)
    {
        CreateNewComponentButton(logic.GetLogicText(), logic.AllowMultibit(), (bits, multibit, worldPos) =>
        {
            return new LogicGate(bits, multibit, logic, worldPos);
        }, logic.DefaultBits(), false);
    }

    public void DrawGrid()
    {
        // Get camera's position and the size of the current view
        // This depends on the zoom of the camera. Dividing by the
        // zoom gives a correct representation of the actual visible view.
        Vector2 camPos = this.editorCamera.target;
        Vector2 viewSize = UserInput.GetViewSize(this.editorCamera);

        int pixelsInBetweenLines = 250;

        // Draw vertical lines
        for (int i = (int)((camPos.X - viewSize.X / 2.0F) / pixelsInBetweenLines); i < ((camPos.X + viewSize.X / 2.0F) / pixelsInBetweenLines); i++)
        {
            int lineX = i * pixelsInBetweenLines;
            int lineYstart = (int)(camPos.Y - viewSize.Y / 2.0F);
            int lineYend = (int)(camPos.Y + viewSize.Y / 2.0F);

            Raylib.DrawLine(lineX, lineYstart, lineX, lineYend, Color.DARKGRAY);
        }

        // Draw horizontal lines
        for (int i = (int)((camPos.Y - viewSize.Y / 2.0F) / pixelsInBetweenLines); i < ((camPos.Y + viewSize.Y / 2.0F) / pixelsInBetweenLines); i++)
        {
            int lineY = i * pixelsInBetweenLines;
            int lineXstart = (int)(camPos.X - viewSize.X / 2.0F);
            int lineXend = (int)(camPos.X + viewSize.X / 2.0F);
            Raylib.DrawLine(lineXstart, lineY, lineXend, lineY, Color.DARKGRAY);
        }
    }

    public void FindEditorState()
    {
        if (!ImGui.GetIO().WantCaptureMouse)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE) && this.editorState == EditorState.None)
            {
                this.editorState = EditorState.MovingCamera;
            }
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_SPACE) && this.editorState == EditorState.MovingCamera)
            {
                this.editorState = EditorState.None;
            }

            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_MIDDLE_BUTTON) && this.editorState == EditorState.None)
            {
                this.editorState = EditorState.MovingCamera; // Pressing Middle mouse button and doing nothing -> move camera
            }
            if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_MIDDLE_BUTTON) && this.editorState == EditorState.MovingCamera)
            {
                this.editorState = EditorState.None; // Releasing Middle mouse button and moving camera -> doing nothing
            }

            if (this.hoveredOutput != null && this.editorState == EditorState.None)
            {
                this.editorState = EditorState.HoveringOutput;
            }
            else if (this.hoveredOutput == null && this.editorState == EditorState.HoveringOutput)
            {
                this.editorState = EditorState.None;
            }

            if (this.hoveredInput != null && this.editorState == EditorState.None)
            {
                this.editorState = EditorState.HoveringInput;
            }
            else if (this.hoveredInput == null && this.editorState == EditorState.HoveringInput)
            {
                this.editorState = EditorState.None;
            }

            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON) && this.editorState == EditorState.None)
            {
                if (this.hoveredComponent != null)
                {
                    if (this.simulator.IsComponentSelected(this.hoveredComponent))
                    {
                        this.editorState = EditorState.MovingSelection;
                    }
                }
                else
                {
                    this.editorState = EditorState.RectangleSelecting;
                    this.recSelectFirstCorner = UserInput.GetMousePositionInWorld(this.editorCamera);
                }
            }
        }

        if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON) && this.editorState == EditorState.MovingSelection)
        {
            this.editorState = EditorState.None;
        }

        if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON) && this.editorState == EditorState.HoveringOutput)
        {
            this.connectFrom = this.hoveredOutput;
            this.editorState = EditorState.OutputToInput;
        }

        if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON) && this.editorState == EditorState.OutputToInput)
        {
            if (this.hoveredInput != null)
            {
                if (!this.hoveredInput.HasSignal())
                {
                    Wire wire = new Wire(this.connectFrom.Bits, this.hoveredInput.OnComponent, this.hoveredInput.OnComponentIndex, this.connectFrom!.OnComponent, this.connectFrom!.OnComponentIndex);
                    this.hoveredInput.SetSignal(wire);
                    this.connectFrom.AddOutputWire(wire);
                    this.simulator.AddWire(wire);
                    this.editorState = EditorState.None;
                }
            }
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.KEY_ESCAPE) && this.editorState == EditorState.OutputToInput)
        {
            this.editorState = EditorState.None;
            this.connectFrom = null;
        }
    }

    public void PerformEditorState()
    {
        Vector2 mouseDelta = UserInput.GetMouseDelta(this.editorCamera);

        // ALLOW USER TO ZOOM IN AND OUT
        // TODO: Should probably fix some cap on
        // how far in/out u can zoom.
        if (!ImGui.GetIO().WantCaptureMouse)
        {
            if (Raylib.GetMouseWheelMove() > 0)
            {
                this.editorCamera.zoom = 1.05F * this.editorCamera.zoom;
            }
            if (Raylib.GetMouseWheelMove() < 0)
            {
                this.editorCamera.zoom = (1f / 1.05f) * this.editorCamera.zoom;
            }

            // SELECTING/DESELECTING
            if (this.editorState == EditorState.None)
            {
                if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
                {
                    if (hoveredComponent != null)
                    {
                        this.simulator.ClearSelection();
                        this.simulator.SelectComponent(this.hoveredComponent);
                        this.editorState = EditorState.MovingSelection;
                    }
                    else
                    {
                        this.simulator.ClearSelection();
                    }
                }
            }
        }

        // CHECKS TO SEE WHICH EDITOR STATE TO PERFORM
        if (this.editorState == EditorState.MovingCamera)
        {
            this.editorCamera.target = this.editorCamera.target - mouseDelta;
        }

        if (this.editorState == EditorState.MovingSelection)
        {
            this.simulator.MoveSelection(this.editorCamera);
        }

        if (this.editorState == EditorState.HoveringInput && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON))
        {
            this.simulator.DeleteWire(this.hoveredInput.Signal);
        }

        if (this.editorState == EditorState.RectangleSelecting)
        {
            Rectangle rec = Util.CreateRecFromTwoCorners(this.recSelectFirstCorner, UserInput.GetMousePositionInWorld(this.editorCamera));
            this.simulator.ClearSelection();
            this.simulator.SelectComponentsInRectangle(rec);
        }

        if (this.editorState == EditorState.RectangleSelecting && Raylib.IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON))
        {
            this.editorState = EditorState.None;
        }

        if (!ImGui.GetIO().WantCaptureKeyboard)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_BACKSPACE))
            {
                this.simulator.DeleteSelection();
            }

            foreach (Tuple<string, List<Tuple<string, Func<bool>, KeyboardKey?, KeyboardKey?, Action>>> tup in this.mainMenuButtons)
            {
                foreach (Tuple<string, Func<bool>, KeyboardKey?, KeyboardKey?, Action> inner in tup.Item2)
                {
                    if (inner.Item3.HasValue && inner.Item4.HasValue)
                    {
                        if (UserInput.KeyComboPressed(inner.Item3.Value, inner.Item4.Value))
                        {
                            if (inner.Item5 != null)
                            {
                                inner.Item5();
                            }
                        }
                    }
                }
            }
        }
    }

    public void NewComponent(Component comp)
    {
        this.simulator.AddComponent(comp);
        this.simulator.ClearSelection();
        this.simulator.SelectComponent(comp);
        this.editorState = EditorState.MovingSelection;
    }

    public void EditorError(string error)
    {
        this.encounteredError = true;
        this.error = error;
        ImGui.OpenPopup("Error");
    }

    public override void Update()
    {
        Vector2 mousePosInWorld = UserInput.GetMousePositionInWorld(this.editorCamera);

        this.hoveredInput = this.simulator.GetInputFromWorldPos(mousePosInWorld);
        this.hoveredOutput = this.simulator.GetOutputFromWorldPos(mousePosInWorld);
        this.hoveredComponent = this.simulator.GetComponentFromWorldPos(mousePosInWorld);

        FindEditorState();
        PerformEditorState();
        this.simulator.Update(mousePosInWorld);
    }

    public override void Render()
    {
        Raylib.BeginMode2D(this.editorCamera);
        Raylib.ClearBackground(Color.LIGHTGRAY);
        DrawGrid();

        Vector2 mousePosInWorld = UserInput.GetMousePositionInWorld(this.editorCamera);

        this.simulator.Render(mousePosInWorld);

        if (this.editorState == EditorState.OutputToInput)
        {
            Raylib.DrawLineBezier(this.connectFrom.Position, mousePosInWorld, 4, Color.BLACK);
            Raylib.DrawLineBezier(this.connectFrom.Position, mousePosInWorld, 2, Color.WHITE);
        }

        if (this.editorState == EditorState.RectangleSelecting)
        {
            Raylib.DrawRectangleLinesEx(Util.CreateRecFromTwoCorners(this.recSelectFirstCorner, mousePosInWorld), 2, Color.BLUE.Opacity(0.3f));
            Raylib.DrawRectangleRec(Util.CreateRecFromTwoCorners(this.recSelectFirstCorner, mousePosInWorld), Color.BLUE.Opacity(0.3f));
        }

        Raylib.EndMode2D();
    }
}