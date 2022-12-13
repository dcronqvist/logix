namespace LogiX.Architecture.StateMachine;

public abstract class State<TUpdate, TOnEnter>
{
    public FSM<TUpdate, TOnEnter> fsm_;
    private Type transitionTo;
    public TOnEnter onEnterArg;

    public State()
    {
        this.transitionTo = null;
    }

    public abstract void Update(TUpdate arg);
    public virtual void PostSimRender(TUpdate arg) { }
    public virtual void PreSimRender(TUpdate arg) { }
    public virtual void SubmitUI(TUpdate arg) { }
    public virtual void OnEnter(TUpdate updateArg, TOnEnter arg) { }
    public virtual bool RenderAboveGUI() => false;

    public void GoToState<TState>(TOnEnter arg) where TState : State<TUpdate, TOnEnter>
    {
        this.transitionTo = typeof(TState);
        this.onEnterArg = arg;
    }

    public bool WantsTransition(out Type goTo)
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