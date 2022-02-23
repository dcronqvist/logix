using LogiX.Components;

namespace LogiX.Editor;

public class EditorFSM : FSM<Editor, int>
{
    public EditorFSM()
    {
        this.AddNewState(new ESNone());
        this.AddNewState(new ESMovingSelection());
        this.AddNewState(new ESRectangleSelecting());
        this.AddNewState(new ESHoveringIO());
        this.AddNewState(new ESConnectIOToOther());

        this.SetState<ESNone>(null, 0);
    }
}

public class ESNone : State<Editor, int>
{
    public override void Update(Editor arg)
    {
        if (!ImGui.GetIO().WantCaptureMouse && !ImGui.GetIO().WantCaptureKeyboard)
        {
            if (Raylib.IsKeyDown(KeyboardKey.KEY_SPACE) || Raylib.IsMouseButtonDown(MouseButton.MOUSE_MIDDLE_BUTTON))
            {
                arg.camera.target -= UserInput.GetMouseDelta(arg.camera);
            }

            float zoomSpeed = 1.15f;
            if (Raylib.GetMouseWheelMove() > 0)
            {
                arg.camera.zoom = zoomSpeed * arg.camera.zoom;
            }
            if (Raylib.GetMouseWheelMove() < 0)
            {
                arg.camera.zoom = (1f / zoomSpeed) * arg.camera.zoom;
            }
            arg.camera.zoom = MathF.Min(MathF.Max(arg.camera.zoom, 0.1f), 4f);

            if (arg.Simulator.TryGetIOFromWorldPosition(arg.GetWorldMousePos(), out (IO, int)? io))
            {
                this.GoToState<ESHoveringIO>(0);
            }
        }

        if (!ImGui.GetIO().WantCaptureMouse)
        {
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
            {
                // PRESSING DOWN LEFT MOUSE BUTTON
                if (arg.Simulator.TryGetComponentFromWorldPosition(arg.GetWorldMousePos(), out Component? comp))
                {
                    // PRESSED DOWN ON A COMPONENT

                    if (arg.Simulator.IsComponentSelected(comp))
                    {
                        // IF ALREADY SELECTED, GO TO MOVESELECTION STATE
                        this.GoToState<ESMovingSelection>(1);
                    }
                    else
                    {
                        // IF NOT SELECTED, SELECT THIS AND CLEAR THE PREVIOUS SELECTION, GO TO MOVESELECTION STATE
                        arg.Simulator.Selection.Clear();
                        arg.Simulator.SelectComponent(comp);
                        this.GoToState<ESMovingSelection>(1);
                    }
                }
                else if (arg.Simulator.TryGetFreeWireNodeFromPosition(arg.GetWorldMousePos(), out WireNode? comesFrom, out WireNode? wireNode))
                {
                    // PRESSED DOWN ON A WIRE NODE, SELECT WIRE NODE AND GO TO MOVE SELECTION STATE
                    if (wireNode.CanBeMoved())
                    {
                        arg.Simulator.Selection.Clear();
                        arg.Simulator.Selection.Add(wireNode);
                        this.GoToState<ESMovingSelection>(1);
                    }
                }
                else if (arg.Simulator.TryGetWireNodeFromPosition(arg.GetWorldMousePos(), out WireNode? from, out WireNode? to, out Wire? wire))
                {
                    // PRESSED DOWN ON A WIRE, CLEAR SELECTION AND CREATE FREE WIRE NODE WHERE CLICKED, SELECT NEWLY CREATED WIRENODE AS WELL
                    arg.Simulator.Selection.Clear();
                    FreeWireNode fwn = from.InsertFreeNode(arg.GetWorldMousePos(), to);
                    arg.Simulator.Selection.Add(fwn);
                    this.GoToState<ESMovingSelection>(1);
                }
                else
                {
                    // NOT PRESSING DOWN ON A COMPONENT - TODO: MIGHT BE PRESSING DOWN ON WIRES
                    // GO TO RECTANGLE SELECTING
                    this.GoToState<ESRectangleSelecting>(0);
                }
            }

            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON))
            {
                if (arg.Simulator.TryGetWireNodeFromPosition(arg.GetWorldMousePos(), out WireNode? from, out WireNode? to, out Wire? wire))
                {
                    // PRESSED DOWN ON A WIRE
                    if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON))
                    {
                        // REMOVE THE TO NODE
                        List<WireNode> removedWireNodes = from.RemoveNode(to, out List<IO> iosToRemove);
                        foreach (IO i in iosToRemove)
                        {
                            wire.DisconnectIO(i);
                        }
                        foreach (WireNode wn in removedWireNodes)
                        {
                            if (arg.Simulator.IsSelected(wn))
                            {
                                arg.Simulator.Selection.Remove(wn);
                            }
                        }
                    }
                }
                else if (arg.Simulator.TryGetFreeWireNodeFromPosition(arg.GetWorldMousePos(), out WireNode? comesFrom, out WireNode? wireNode))
                {
                    // REMOVE THE RIGHT CLICKED FREE WIRE NODE
                    comesFrom.Next.Remove(wireNode);
                    comesFrom.Next.AddRange(wireNode.Next);
                }
            }
        }
    }

    public override void Render(Editor arg)
    {
        if (arg.Simulator.TryGetWireNodeFromPosition(arg.GetWorldMousePos(), out WireNode wireNodeFrom, out WireNode wireNodeTo, out Wire wire))
        {
            Raylib.DrawLineV(wireNodeFrom.GetPosition(), wireNodeTo.GetPosition(), Color.BLUE);
        }
    }
}

public class ESMovingSelection : State<Editor, int>
{
    Vector2 startPos;
    bool willDoCommand;

    public override void OnEnter(Editor updateArg, int arg)
    {
        this.startPos = updateArg.GetWorldMousePos();
        this.willDoCommand = arg == 1;
    }

    public override void Update(Editor arg)
    {
        arg.Simulator.MoveSelection(UserInput.GetMouseDelta(arg.camera));

        if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON))
        {
            Vector2 endPos = arg.GetWorldMousePos();

            if (this.willDoCommand)
            {
                CommandMovedSelection cms = new CommandMovedSelection(arg.Simulator.Selection.Copy(), endPos - startPos);
                arg.Execute(cms, arg);
            }

            this.GoToState<ESNone>(0);
        }
    }
}

public class ESRectangleSelecting : State<Editor, int>
{
    Vector2 startPos;

    public override void OnEnter(Editor updateArg, int arg)
    {
        this.startPos = updateArg.GetWorldMousePos();
    }

    public override void Update(Editor arg)
    {
        arg.Simulator.Selection.Clear();
        arg.Simulator.SelectInRect(Util.CreateRecFromTwoCorners(startPos, arg.GetWorldMousePos()));
        if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON))
        {
            this.GoToState<ESNone>(0);
        }
    }

    public override void Render(Editor arg)
    {
        Raylib.DrawRectangleLinesEx(Util.CreateRecFromTwoCorners(startPos, arg.GetWorldMousePos()).Inflate(2), 2, Color.BLUE.Opacity(0.5f));
        Raylib.DrawRectangleRec(Util.CreateRecFromTwoCorners(startPos, arg.GetWorldMousePos()), Color.BLUE.Opacity(0.3f));
    }
}

public class ESHoveringIO : State<Editor, int>
{
    public override void Update(Editor arg)
    {
        if (arg.Simulator.TryGetIOFromWorldPosition(arg.GetWorldMousePos(), out (IO, int)? io))
        {
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
            {
                arg.FirstClickedIO = io.Value.Item1;
                // GO TO STATE IO->CONNECT
                // LEFT PRESSED ON IO, WE ARE PERFORMING SOME KIND OF CONNECTIONTHINGY HERE
                // EITHER WE HAVE TWO OPTIONS
                // 1. CONNECT DIRECTLY TO ANOTHER IO -> NEW WIRE MUST BE CREATED BETWEEN THESE
                // 2. CONNECT TO AN EXISTING WIRE -> WE MUST CONNECT THIS IO TO THE WIRE AND ADD AN IOWIREPOINT TO THAT WIRE WHICH POINTS TO THIS IO
                this.GoToState<ESConnectIOToOther>(0);
            }
        }
        else
        {
            this.GoToState<ESNone>(0);
        }
    }
}

public class ESConnectIOToOther : State<Editor, int>
{
    public override void Update(Editor arg)
    {

    }

    public override void SubmitUI(Editor arg)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_ESCAPE) && !ImGui.GetIO().WantCaptureKeyboard)
        {
            this.GoToState<ESNone>(0);
        }

        if (arg.Simulator.TryGetIOFromWorldPosition(arg.GetWorldMousePos(), out (IO, int)? io))
        {
            // HOVERING IO AGAIN, MAKE SURE IT IS NOT THE SAME

            if (io.Value.Item1 == arg.FirstClickedIO)
            {
                Util.Tooltip("Cannot connect to self");
                return;
            }

            // IF WE GET TO HERE WE KNOW THAT WE ARE HOVERING OTHER IO AND IT IS NOT THE SAME AS THE FIRST

            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
            {
                Wire wire = new Wire(arg.FirstClickedIO);
                wire.ConnectIO(io.Value.Item1);
                wire.RootWireNode.AddIONode(io.Value.Item1);

                arg.Simulator.AddWire(wire);

                this.GoToState<ESNone>(0);
            }
        }

        if (arg.Simulator.TryGetWireNodeFromPosition(arg.GetWorldMousePos(), out WireNode? from, out WireNode? to, out Wire? w))
        {
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
            {
                Vector2 pos = arg.GetWorldMousePos();
                FreeWireNode fwn = from.InsertFreeNode(pos, to);

                fwn.AddIONode(arg.FirstClickedIO);
                w.ConnectIO(arg.FirstClickedIO);
                this.GoToState<ESNone>(0);
            }
        }
    }

    public override void Render(Editor arg)
    {
        Raylib.DrawLineV(arg.FirstClickedIO.OnComponent.GetIOPosition(arg.FirstClickedIO), arg.GetWorldMousePos(), Color.BLUE);
    }
}