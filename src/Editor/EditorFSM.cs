using LogiX.Components;

namespace LogiX.Editor;

public class EditorFSM : FSM<Editor, int>
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
        this.AddNewState(new StateMeasuringSteps());

        this.SetState<StateNone>(null, 0);
    }
}

public class StateNone : State<Editor, int>
{
    public override void Update(Editor editor)
    {
        if (!ImGui.GetIO().WantCaptureMouse && !ImGui.GetIO().WantCaptureKeyboard)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE) || Raylib.IsMouseButtonPressed(MouseButton.MOUSE_MIDDLE_BUTTON))
            {
                // TODO: Goto StateMovingCamera
                this.GoToState<StateMovingCamera>(0);
                return;
            }

            if (editor.hoveredInput != null)
            {
                this.GoToState<StateHoveringInput>(0);
            }

            if (editor.hoveredOutput != null)
            {
                this.GoToState<StateHoveringOutput>(0);
            }
        }

        if (!ImGui.GetIO().WantCaptureMouse)
        {
            (Wire? w, int wpi) = editor.simulator.GetWireAndPointFromWorldPos(UserInput.GetMousePositionInWorld(editor.editorCamera));

            if (w != null && editor.simulator.SelectedWirePoints.Contains((w, wpi)) && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
            {
                editor.simulator.ClearSelection();
                this.GoToState<StateMovingSelection>(1);
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
                this.GoToState<StateMovingSelection>(1);
                return;
            }

            if (w == null && editor.hoveredComponent == null && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
            {
                editor.simulator.SelectedWirePoints.Clear();
                editor.recSelectFirstCorner = UserInput.GetMousePositionInWorld(editor.editorCamera);
                this.GoToState<StateRectangleSelecting>(0);
                return;
            }


            if (editor.hoveredComponent != null && editor.simulator.IsComponentSelected(editor.hoveredComponent) && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
            {
                this.GoToState<StateMovingSelection>(0);
            }
            else if (editor.hoveredComponent != null && !editor.simulator.IsComponentSelected(editor.hoveredComponent) && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON) && Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT))
            {
                editor.simulator.SelectComponent(editor.hoveredComponent);
                //this.GoToState<StateMovingSelection>();
            }
            else if (editor.hoveredComponent != null && !editor.simulator.IsComponentSelected(editor.hoveredComponent) && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
            {
                editor.simulator.ClearSelection();
                editor.simulator.SelectedWirePoints.Clear();
                editor.simulator.SelectComponent(editor.hoveredComponent);
                this.GoToState<StateMovingSelection>(0);
            }


            if (editor.hoveredComponent == null && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
            {
                editor.simulator.ClearSelection();
                editor.recSelectFirstCorner = UserInput.GetMousePositionInWorld(editor.editorCamera);
                this.GoToState<StateRectangleSelecting>(0);
            }
        }
    }
}

public class StateMeasuringSteps : State<Editor, int>
{
    public override void Update(Editor editor)
    {
        if (!ImGui.GetIO().WantCaptureMouse)
        {
            if (editor.hoveredComponent != null && !editor.simulator.IsComponentSelected(editor.hoveredComponent) && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
            {
                editor.simulator.SelectComponent(editor.hoveredComponent);
            }

            Component fromComponent = editor.simulator.SelectedComponents[0];

            if (editor.simulator.SelectedComponents.Count == 2)
            {
                Component toComponent = editor.simulator.SelectedComponents[1];

                int steps = fromComponent.GetMaxStepsToOtherComponent(toComponent);
                editor.Modal("Measured steps", "Max steps: " + steps, ModalButtonsType.OK);
                this.GoToState<StateNone>(0);
            }
        }
    }

    public override void Render(Editor editor)
    {

    }

    public override void SubmitUI(Editor editor)
    {

    }
}

public class StateMovingCamera : State<Editor, int>
{
    public override void Update(Editor editor)
    {
        Vector2 mouseDelta = UserInput.GetMouseDelta(editor.editorCamera);
        editor.editorCamera.target = editor.editorCamera.target - mouseDelta;

        if (!ImGui.GetIO().WantCaptureMouse && !ImGui.GetIO().WantCaptureKeyboard)
        {
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_SPACE) || Raylib.IsMouseButtonReleased(MouseButton.MOUSE_MIDDLE_BUTTON))
            {
                this.GoToState<StateNone>(0);
                return;
            }
        }
    }
}

public class StateHoveringInput : State<Editor, int>
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
            this.GoToState<StateNone>(0);
        }
    }
}

public class StateHoveringOutput : State<Editor, int>
{
    public override void Update(Editor editor)
    {
        ComponentOutput? hoveredOutput = editor.hoveredOutput;

        if (!ImGui.GetIO().WantCaptureMouse && hoveredOutput != null && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
        {
            // TODO: Goto output-to-input
            editor.connectFrom = hoveredOutput;
            this.GoToState<StateOutputToInput>(0);
        }

        if (hoveredOutput == null)
        {
            // Go back to None
            this.GoToState<StateNone>(0);
        }
    }
}

public class StateOutputToInput : State<Editor, int>
{
    public override void Update(Editor editor)
    {
        if (!ImGui.GetIO().WantCaptureMouse && editor.connectFrom != null && editor.hoveredInput != null && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
        {
            // Create wire.
            ComponentOutput? connectFrom = editor.connectFrom;
            ComponentInput? hoveredInput = editor.hoveredInput;

            if (hoveredInput.HasSignal() || hoveredInput.Bits != connectFrom.Bits)
            {
                return;
            }

            ConnectWireCommand cwc = new ConnectWireCommand(connectFrom, hoveredInput);
            editor.Execute(cwc, editor);

            editor.connectFrom = null;

            this.GoToState<StateNone>(0);
            return;
        }

        if (!ImGui.GetIO().WantCaptureKeyboard && Raylib.IsKeyPressed(KeyboardKey.KEY_ESCAPE))
        {
            this.GoToState<StateNone>(0);
        }
    }

    public override void Render(Editor editor)
    {
        ComponentOutput? connectFrom = editor.connectFrom;
        if (connectFrom != null)
        {
            Color color = Util.InterpolateColors(Color.WHITE, Color.BLUE, connectFrom.GetHighFraction());

            if (editor.hoveredInput != null)
            {
                if (editor.hoveredInput.HasSignal() || editor.hoveredInput.Bits != connectFrom.Bits)
                {
                    color = new Color(255, 97, 97, 255);
                }
            }

            if (editor.hoveredOutput != null)
            {
                color = new Color(255, 97, 97, 255);
            }

            Raylib.DrawLineEx(connectFrom.Position, UserInput.GetMousePositionInWorld(editor.editorCamera), 6, Color.BLACK);
            Raylib.DrawLineEx(connectFrom.Position, UserInput.GetMousePositionInWorld(editor.editorCamera), 4, color);
        }
    }

    public override void SubmitUI(Editor editor)
    {
        if (editor.hoveredInput != null && editor.connectFrom != null)
        {
            ComponentInput? hoveredInput = editor.hoveredInput;
            if (hoveredInput.HasSignal())
            {
                Util.Tooltip("Already connected");
            }

            if (editor.hoveredInput.Bits != editor.connectFrom.Bits)
            {
                Util.Tooltip("Different bit widths");
            }
        }

        if (editor.hoveredOutput != null && editor.connectFrom != null && editor.hoveredOutput != editor.connectFrom)
        {
            Util.Tooltip("Cannot connect to output");
        }
    }
}

public class StateMovingSelection : State<Editor, int>
{
    Vector2 startPos;
    bool doCommand = false;

    public override void OnEnter(Editor editor, int arg)
    {
        startPos = UserInput.GetMousePositionInWorld(editor.editorCamera);
        doCommand = arg == 1 ? false : true;
    }

    public override void Update(Editor editor)
    {
        editor.simulator.MoveSelection(editor.editorCamera);

        if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON))
        {
            if (doCommand)
            {
                Vector2 endPos = UserInput.GetMousePositionInWorld(editor.editorCamera);
                MovedSelectionCommand msc = new MovedSelectionCommand(editor.simulator.SelectedComponents.Copy(), editor.simulator.SelectedWirePoints.Copy(), (endPos - startPos));
                editor.Execute(msc, editor, doExecute: false);
            }
            this.GoToState<StateNone>(0);
            return;
        }
    }
}

public class StateRectangleSelecting : State<Editor, int>
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
            this.GoToState<StateNone>(0);
        }
    }

    public override void Render(Editor editor)
    {
        Vector2 mousePosInWorld = UserInput.GetMousePositionInWorld(editor.editorCamera);
        Raylib.DrawRectangleLinesEx(Util.CreateRecFromTwoCorners(editor.recSelectFirstCorner, mousePosInWorld), 2, Color.BLUE.Opacity(0.3f));
        Raylib.DrawRectangleRec(Util.CreateRecFromTwoCorners(editor.recSelectFirstCorner, mousePosInWorld), Color.BLUE.Opacity(0.3f));
    }
}
