namespace LogiX.Architecture.StateMachine;

public class TabFSM : FSM<EditorTab, int>
{
    public TabFSM()
    {
        this.AddNewState(new StateIdle());
        this.AddNewState(new StateHoveringIOGroup());
        this.AddNewState(new StateDraggingWire());

        this.SetState<StateIdle>(null, 0);
    }
}