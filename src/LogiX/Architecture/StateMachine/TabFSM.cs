namespace LogiX.Architecture.StateMachine;

public class TabFSM : FSM<EditorTab, int>
{
    public TabFSM()
    {
        this.AddNewState(new StateIdle());
        this.AddNewState(new StateHoveringIOGroup());
        this.AddNewState(new StateHoveringWireVertex());
        this.AddNewState(new StateHoveringWireSegment());
        this.AddNewState(new StateDraggingWire());
        this.AddNewState(new StateMovingSelection());
        this.AddNewState(new StateRectangleSelecting());

        this.SetState<StateIdle>(null, 0);
    }
}