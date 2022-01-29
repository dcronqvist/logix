namespace LogiX.Editor;

public abstract class State<TUpdate>
{
    public FSM<TUpdate>? fsm_;
    private Type? transitionTo;

    public State()
    {
        this.transitionTo = null;
    }

    public abstract void Update(TUpdate arg);
    public virtual void Render(TUpdate arg) { }
    public virtual void SubmitUI(TUpdate arg) { }

    public void GoToState<TState>() where TState : State<TUpdate>
    {
        this.transitionTo = typeof(TState);
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

public class FSM<TUpdate>
{
    public State<TUpdate>? CurrentState { get; set; }
    public List<State<TUpdate>> States { get; set; }

    public FSM()
    {
        this.States = new List<State<TUpdate>>();
    }

    public void SetState<TState>() where TState : State<TUpdate>
    {
        this.CurrentState = this.GetState(typeof(TState));
    }

    public FSM<TUpdate> AddNewState(State<TUpdate> s)
    {
        s.fsm_ = this;
        this.States.Add(s);
        return this;
    }

    public State<TUpdate>? GetState(Type type)
    {
        foreach (State<TUpdate> s in this.States)
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
                    State<TUpdate>? newState = this.GetState(goTo);
                    if (newState != null)
                    {
                        this.CurrentState = newState;
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