namespace LogiX.Architecture.StateMachine;

public class FSM<TUpdate, TOnEnter>
{
    public State<TUpdate, TOnEnter> CurrentState { get; set; }
    public List<State<TUpdate, TOnEnter>> States { get; set; }

    public FSM()
    {
        this.States = new List<State<TUpdate, TOnEnter>>();
    }

    public void SetState<TState>(TUpdate updateArg, TOnEnter gotoArg) where TState : State<TUpdate, TOnEnter>
    {
        this.CurrentState = this.GetState(typeof(TState));
        this.CurrentState.OnEnter(updateArg, gotoArg);
    }

    public FSM<TUpdate, TOnEnter> AddNewState(State<TUpdate, TOnEnter> s)
    {
        s.fsm_ = this;
        this.States.Add(s);
        return this;
    }

    public State<TUpdate, TOnEnter> GetState(Type type)
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

            foreach (State<TUpdate, TOnEnter> s in this.States)
            {
                if (s.WantsTransition(out Type goTo))
                {
                    if (goTo != null)
                    {
                        State<TUpdate, TOnEnter> newState = this.GetState(goTo);
                        if (newState != null)
                        {
                            TOnEnter onEnter = s.onEnterArg;
                            this.CurrentState = newState;
                            this.CurrentState.OnEnter(arg, onEnter);
                        }
                    }
                }
            }
        }
    }

    public void PreSimRender(TUpdate arg)
    {
        this.CurrentState?.PreSimRender(arg);
    }

    public void PostSimRender(TUpdate arg)
    {
        this.CurrentState?.PostSimRender(arg);
    }

    public void SubmitUI(TUpdate arg)
    {
        this.CurrentState?.SubmitUI(arg);
    }
}