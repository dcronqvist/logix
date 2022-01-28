using LogiX.Components;

namespace LogiX.Editor;

public class EditorFSM : FSM<Editor>
{
    public EditorFSM() : base()
    {
        this.AddNewState(new StateNone());
        this.AddNewState(new StateMovingCamera());
        this.AddNewState(new StateHoveringInput());
        this.AddNewState(new StateHoveringOutput());
        this.AddNewState(new StateOutputToInput());
        this.AddNewState(new StateMovingSelection());
        this.AddNewState(new StateRectangleSelecting());

        this.SetState<StateNone>();
    }
}

public class StateNone : State<Editor>
{
    public override void Update(Editor editor)
    {
        if (!ImGui.GetIO().WantCaptureMouse && !ImGui.GetIO().WantCaptureKeyboard)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE) || Raylib.IsMouseButtonPressed(MouseButton.MOUSE_MIDDLE_BUTTON))
            {
                // TODO: Goto StateMovingCamera
                this.GoToState<StateMovingCamera>();
                return;
            }

            if (editor.hoveredInput != null)
            {
                this.GoToState<StateHoveringInput>();
            }

            if (editor.hoveredOutput != null)
            {
                this.GoToState<StateHoveringOutput>();
            }
        }

        if (!ImGui.GetIO().WantCaptureMouse)
        {
            (Wire? w, int wpi) = editor.simulator.GetWireAndPointFromWorldPos(UserInput.GetMousePositionInWorld(editor.editorCamera));

            if (w != null && editor.simulator.SelectedWirePoints.Contains((w, wpi)) && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
            {
                this.GoToState<StateMovingSelection>();
                return;
            }

            if (w != null && wpi >= 0 && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
            {
                if (!Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT))
                {
                    editor.simulator.SelectedWirePoints.Clear();
                    editor.simulator.ClearSelection();
                }
                editor.simulator.SelectWirePoint(w, wpi);
                this.GoToState<StateMovingSelection>();
                return;
            }

            if (w == null && editor.hoveredComponent == null && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
            {
                editor.simulator.SelectedWirePoints.Clear();
                editor.recSelectFirstCorner = UserInput.GetMousePositionInWorld(editor.editorCamera);
                this.GoToState<StateRectangleSelecting>();
                return;
            }


            if (editor.hoveredComponent != null && editor.simulator.IsComponentSelected(editor.hoveredComponent) && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
            {
                this.GoToState<StateMovingSelection>();
            }
            else if (editor.hoveredComponent != null && !editor.simulator.IsComponentSelected(editor.hoveredComponent) && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
            {
                editor.simulator.ClearSelection();
                editor.simulator.SelectedWirePoints.Clear();
                editor.simulator.SelectComponent(editor.hoveredComponent);
                this.GoToState<StateMovingSelection>();
            }

            if (editor.hoveredComponent == null && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
            {
                editor.simulator.ClearSelection();
                editor.recSelectFirstCorner = UserInput.GetMousePositionInWorld(editor.editorCamera);
                this.GoToState<StateRectangleSelecting>();
            }
        }
    }
}

public class StateMovingCamera : State<Editor>
{
    public override void Update(Editor editor)
    {
        Vector2 mouseDelta = UserInput.GetMouseDelta(editor.editorCamera);
        editor.editorCamera.target = editor.editorCamera.target - mouseDelta;

        if (!ImGui.GetIO().WantCaptureMouse && !ImGui.GetIO().WantCaptureKeyboard)
        {
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_SPACE) || Raylib.IsMouseButtonReleased(MouseButton.MOUSE_MIDDLE_BUTTON))
            {
                this.GoToState<StateNone>();
                return;
            }
        }
    }
}

public class StateHoveringInput : State<Editor>
{
    public override void Update(Editor editor)
    {
        if (!ImGui.GetIO().WantCaptureMouse && editor.hoveredInput != null && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON))
        {
            if (editor.hoveredInput.HasSignal())
            {
                editor.simulator.DeleteWire(editor.hoveredInput.Signal);
                editor.hoveredInput.RemoveSignal();
            }
        }

        if (editor.hoveredInput == null)
        {
            // Go back to None
            this.GoToState<StateNone>();
        }
    }
}

public class StateHoveringOutput : State<Editor>
{
    public override void Update(Editor editor)
    {
        ComponentOutput? hoveredOutput = editor.hoveredOutput;

        if (!ImGui.GetIO().WantCaptureMouse && hoveredOutput != null && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
        {
            // TODO: Goto output-to-input
            editor.connectFrom = hoveredOutput;
            this.GoToState<StateOutputToInput>();
        }

        if (hoveredOutput == null)
        {
            // Go back to None
            this.GoToState<StateNone>();
        }
    }
}

public class StateOutputToInput : State<Editor>
{
    public override void Update(Editor editor)
    {
        if (!ImGui.GetIO().WantCaptureMouse && editor.connectFrom != null && editor.hoveredInput != null && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
        {
            // Create wire.
            ComponentOutput? connectFrom = editor.connectFrom;
            ComponentInput? hoveredInput = editor.hoveredInput;

            Wire wire = new Wire(connectFrom.Bits, hoveredInput.OnComponent, hoveredInput.OnComponentIndex, connectFrom!.OnComponent, connectFrom!.OnComponentIndex);
            if (hoveredInput.SetSignal(wire))
            {
                connectFrom.AddOutputWire(wire);
                editor.simulator.AddWire(wire);
            }

            this.GoToState<StateNone>();
            return;
        }

        if (!ImGui.GetIO().WantCaptureKeyboard && Raylib.IsKeyPressed(KeyboardKey.KEY_ESCAPE))
        {
            this.GoToState<StateNone>();
        }
    }

    public override void Render(Editor editor)
    {
        ComponentOutput? connectFrom = editor.connectFrom;
        if (connectFrom != null)
        {
            Raylib.DrawLineBezier(connectFrom.Position, UserInput.GetMousePositionInWorld(editor.editorCamera), 4, Color.BLACK);
            Raylib.DrawLineBezier(connectFrom.Position, UserInput.GetMousePositionInWorld(editor.editorCamera), 2, Color.WHITE);
        }
    }
}

public class StateMovingSelection : State<Editor>
{
    public override void Update(Editor editor)
    {
        editor.simulator.MoveSelection(editor.editorCamera);

        if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON))
        {
            this.GoToState<StateNone>();
            return;
        }
    }
}

public class StateRectangleSelecting : State<Editor>
{
    public override void Update(Editor editor)
    {
        Rectangle rec = Util.CreateRecFromTwoCorners(editor.recSelectFirstCorner, UserInput.GetMousePositionInWorld(editor.editorCamera));
        editor.simulator.ClearSelection();
        editor.simulator.SelectedWirePoints.Clear();
        editor.simulator.SelectComponentsInRectangle(rec);
        editor.simulator.SelectWirePointsInRectangle(rec);

        if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON))
        {
            this.GoToState<StateNone>();
        }
    }

    public override void Render(Editor editor)
    {
        Vector2 mousePosInWorld = UserInput.GetMousePositionInWorld(editor.editorCamera);
        Raylib.DrawRectangleLinesEx(Util.CreateRecFromTwoCorners(editor.recSelectFirstCorner, mousePosInWorld), 2, Color.BLUE.Opacity(0.3f));
        Raylib.DrawRectangleRec(Util.CreateRecFromTwoCorners(editor.recSelectFirstCorner, mousePosInWorld), Color.BLUE.Opacity(0.3f));
    }
}
