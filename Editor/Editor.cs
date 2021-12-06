using LogiX.Components;

namespace LogiX.Editor;

public class Editor : Application
{
    Camera2D editorCamera;
    EditorState editorState;
    Simulator simulator;
    IGateLogic[] availableGateLogics;

    // VARIABLES FOR TEMPORARY STUFF
    ComponentInput? hoveredInput;
    ComponentOutput? hoveredOutput;
    Component? hoveredComponent;

    ComponentOutput? connectFrom;

    // UI VARIABLES
    int newComponentBits;
    bool newComponentMultibit;

    public override void Initialize()
    {
        this.OnWindowResized += (width, height) =>
        {
            Vector2 windowSize = new Vector2(width, height);
            this.editorCamera = new Camera2D(windowSize / 2.0f, Vector2.Zero, 0f, 1f);
        };

        availableGateLogics = new IGateLogic[] {
            new ANDLogic(),
            new ORLogic()
        };
    }

    public override void LoadContent()
    {
        Vector2 windowSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        this.editorCamera = new Camera2D(windowSize / 2.0f, Vector2.Zero, 0f, 1f);
        this.editorState = EditorState.None;

        this.simulator = new Simulator();
    }

    public override void SubmitUI()
    {
        // MAIN MENU BAR
        ImGui.BeginMainMenuBar();

        // FILE
        if (ImGui.BeginMenu("File"))
        {
            ImGui.EndMenu();
        }

        // EDIT
        if (ImGui.BeginMenu("Edit"))
        {
            ImGui.EndMenu();
        }

        ImGui.EndMainMenuBar();

        // SIDEBAR

        ImGui.SetNextWindowPos(new Vector2(0, 19), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(120, Raylib.GetScreenHeight() - 19), ImGuiCond.Always);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.3f, 0.3f, 0.3f, 0.8f));
        ImGui.Begin("Sidebar", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDecoration);
        ImGui.End();
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();

        // TESTING WINDOW

        ImGui.Begin("Components", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysVerticalScrollbar);
        Vector2 buttonSize = new Vector2(94, 22);
        ImGui.Text("Gates");
        for (int i = 0; i < this.availableGateLogics.Length; i++)
        {
            CreateNewGateButton(this.availableGateLogics[i]);
        }
        ImGui.Separator();
        ImGui.Text("Inputs");

        CreateNewComponentButton("Switch", true, (bits, multibit, worldPos) =>
        {
            return new Switch(bits, worldPos);
        }, 1, false);

        ImGui.End();

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
        ImGui.End();
    }

    public void CreateNewComponentButton(string text, bool multibitPop, Func<int, bool, Vector2, Component> createComponent, int defaultBits, bool defaultMultibit)
    {
        Vector2 buttonSize = new Vector2(94, 22);
        ImGui.Button(text, buttonSize);
        if (ImGui.IsItemClicked())
        {
            // Create new component
            Component comp = createComponent(defaultBits, defaultMultibit, UserInput.GetMousePositionInWorld(this.editorCamera));
            this.NewComponent(comp);
        }

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

    public void CreateNewGateButton(IGateLogic logic)
    {
        CreateNewComponentButton(logic.GetLogicText(), true, (bits, multibit, worldPos) =>
        {
            return new LogicGate(bits, multibit, logic, worldPos);
        }, 2, false);
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
                    Wire wire = new Wire(1, this.hoveredInput.OnComponent, this.hoveredInput.OnComponentIndex, this.connectFrom!.OnComponent, this.connectFrom!.OnComponentIndex);
                    this.hoveredInput.SetSignal(wire);
                    this.connectFrom.AddOutputWire(wire);
                    this.simulator.AddWire(wire);
                    this.editorState = EditorState.None;
                }
            }
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
                this.editorCamera.zoom *= 1.05F;
            }
            if (Raylib.GetMouseWheelMove() < 0)
            {
                this.editorCamera.zoom *= 1.0F / 1.05F;
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

    public void NewComponent(Component comp)
    {
        this.simulator.AddComponent(comp);
        this.simulator.ClearSelection();
        this.simulator.SelectComponent(comp);
        this.editorState = EditorState.MovingSelection;
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

        Raylib.EndMode2D();
    }
}