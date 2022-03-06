namespace LogiX.Editor.StateMachine;

public abstract class State<TUpdate, TOnEnter>
{
    public abstract bool ForcesSameTab { get; }
    public FSM<TUpdate, TOnEnter>? fsm_;
    private Type? transitionTo;
    public TOnEnter onEnterArg;

    public State()
    {
        this.transitionTo = null;
    }

    public abstract void Update(TUpdate arg);
    public virtual void Render(TUpdate arg) { }
    public virtual void SubmitUI(TUpdate arg) { }
    public virtual void OnEnter(TUpdate updateArg, TOnEnter arg) { }

    public void GoToState<TState>(TOnEnter arg) where TState : State<TUpdate, TOnEnter>
    {
        this.transitionTo = typeof(TState);
        this.onEnterArg = arg;
    }

    public bool WantsTransition(out Type? goTo)
    {
        if (transitionTo != null)
        {
            goTo = this.transitionTo;
            this.transitionTo = null;
            return true;
        }
        else
        {
            goTo = null;
            return false;
        }
    }
}