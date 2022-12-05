namespace LogiX.Architecture.StateMachine;

public class EditorFSM : FSM<Editor, int>
{
    public EditorFSM()
    {
        this.AddNewState(new StateIdle());
        this.AddNewState(new StateHoveringPin());
        this.AddNewState(new StateHoveringWireVertex());
        this.AddNewState(new StateHoveringWireSegment());
        this.AddNewState(new StateDraggingWire());
        this.AddNewState(new StateMovingSelection());
        this.AddNewState(new StateRectangleSelecting());
        this.AddNewState(new StateAddingNewComponent());

        this.SetState<StateIdle>(null, 0);
    }
}