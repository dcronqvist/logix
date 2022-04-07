using LogiX.Components;
using LogiX.Editor.Commands;

namespace LogiX.Editor.StateMachine;

public class EditorFSM : FSM<Editor, int>
{
    public EditorFSM()
    {
        this.AddNewState(new ESNone());
        this.AddNewState(new ESMovingSelection());
        this.AddNewState(new ESRectangleSelecting());
        this.AddNewState(new ESHoveringIO());
        this.AddNewState(new ESHoveringWire());
        this.AddNewState(new ESHoveringJunctionNode());
        this.AddNewState(new ESCreateWireFromIO());
        this.AddNewState(new ESCreateWireFromWireNode());

        this.SetState<ESNone>(null, 0);
    }
}