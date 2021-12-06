using LogiX.Components;

namespace LogiX.Editor;

public class Editor : Application
{
    Camera2D editorCamera;
    EditorState editorState;

    public Vector2 GetViewSize()
    {
        int windowWidth = Raylib.GetScreenWidth();
        int windowHeight = Raylib.GetScreenHeight();
        Vector2 viewSize = new Vector2(windowWidth / this.editorCamera.zoom, windowHeight / this.editorCamera.zoom);
        return viewSize;
    }

    public override void Initialize()
    {
        this.OnWindowResized += (width, height) =>
        {
            Vector2 windowSize = new Vector2(width, height);
            this.editorCamera = new Camera2D(windowSize / 2.0f, Vector2.Zero, 0f, 1f);
        };
    }

    public override void LoadContent()
    {
        Vector2 windowSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        this.editorCamera = new Camera2D(windowSize / 2.0f, Vector2.Zero, 0f, 1f);

        this.editorState = EditorState.None;
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
        ImGui.Button("AND", buttonSize);
        ImGui.Button("OR", buttonSize);
        ImGui.Button("XOR", buttonSize);
        ImGui.Button("NAND", buttonSize);
        ImGui.Button("NOR", buttonSize);
        ImGui.Button("XNOR", buttonSize);
        ImGui.Separator();
        ImGui.Text("Inputs");
        ImGui.Button("Switch", buttonSize);
        ImGui.Button("Button", buttonSize);
        ImGui.End();
    }

    public void DrawGrid()
    {
        // Get camera's position and the size of the current view
        // This depends on the zoom of the camera. Dividing by the
        // zoom gives a correct representation of the actual visible view.
        Vector2 camPos = this.editorCamera.target;
        Vector2 viewSize = GetViewSize();

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
    }

    public override void Update()
    {
        FindEditorState();
        PerformEditorState();
    }

    public override void Render()
    {
        Raylib.BeginMode2D(this.editorCamera);
        Raylib.ClearBackground(Color.LIGHTGRAY);
        DrawGrid();

        Wire a = new Wire(1);
        Wire b = new Wire(1);

        Component c = new LogicGate(2, false, new ANDLogic(), Vector2.Zero);

        c.SetInputWire(0, a);
        c.SetInputWire(1, b);

        if (Raylib.IsKeyDown(KeyboardKey.KEY_A))
        {
            a.SetAllValues(LogicValue.HIGH);
        }
        if (Raylib.IsKeyDown(KeyboardKey.KEY_B))
        {
            b.SetAllValues(LogicValue.HIGH);
        }

        c.Update();

        c.Render();

        Raylib.EndMode2D();
    }
}