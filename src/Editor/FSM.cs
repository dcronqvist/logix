namespace LogiX.Editor;

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

public class FSM<TUpdate, TOnEnter>
{
    public State<TUpdate, TOnEnter>? CurrentState { get; set; }
    public List<State<TUpdate, TOnEnter>> States { get; set; }

    public FSM()
    {
        this.States = new List<State<TUpdate, TOnEnter>>();
    }

    public void SetState<TState>(TUpdate updateArg, TOnEnter gotoArg) where TState : State<TUpdate, TOnEnter>
    {
        this.CurrentState = this.GetState(typeof(TState));
        this.CurrentState?.OnEnter(updateArg, gotoArg);
    }

    public FSM<TUpdate, TOnEnter> AddNewState(State<TUpdate, TOnEnter> s)
    {
        s.fsm_ = this;
        this.States.Add(s);
        return this;
    }

    public State<TUpdate, TOnEnter>? GetState(Type type)
    {
        foreach (State<TUpdate, TOnEnter> s in this.States)
        {
            if (s.GetType() == type)
            {
                return s;
            }
        }
        return null;
    }

    public void Update(TUpdate arg)
    {
        if (this.CurrentState != null)
        {
            this.CurrentState.Update(arg);

            if (this.CurrentState.WantsTransition(out Type? goTo))
            {
                if (goTo != null)
                {
                    State<TUpdate, TOnEnter>? newState = this.GetState(goTo);
                    if (newState != null)
                    {
                        TOnEnter onEnter = this.CurrentState.onEnterArg;
                        this.CurrentState = newState;
                        this.CurrentState.OnEnter(arg, onEnter);
                    }
                }
            }
        }
    }

    public void Render(TUpdate arg)
    {
        this.CurrentState?.Render(arg);
    }

    public void SubmitUI(TUpdate arg)
    {
        this.CurrentState?.SubmitUI(arg);
    }
}