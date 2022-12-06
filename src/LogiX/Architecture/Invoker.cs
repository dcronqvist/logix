using LogiX.Architecture.Commands;

namespace LogiX.Architecture;

public abstract class Invoker<TState, TArg>
{
    public List<(TState, Command<TArg>, TState)> Commands { get; set; }
    public int CurrentCommandIndex { get; set; }

    public Invoker()
    {
        this.Commands = new();
        this.CurrentCommandIndex = -1;
    }

    public void ResetInvoker()
    {
        this.Commands.Clear();
        this.CurrentCommandIndex = -1;
    }

    public abstract TState GetCurrentInvokerState();

    public virtual void Execute(Command<TArg> command, TArg arg, bool doExecute = true)
    {
        var stateBefore = this.GetCurrentInvokerState();
        if (this.CurrentCommandIndex < this.Commands.Count - 1)
        {
            this.Commands.RemoveRange(this.CurrentCommandIndex + 1, this.Commands.Count - this.CurrentCommandIndex - 1);
        }
        try
        {
            if (doExecute)
            {
                command.Execute(arg);
            }
        }
        finally
        {
            var stateAfter = this.GetCurrentInvokerState();
            this.Commands.Add((stateBefore, command, stateAfter));
            this.CurrentCommandIndex++;
        }
    }

    public virtual void Execute(TState stateBefore, Command<TArg> command, TArg arg, bool doExecute = true)
    {
        if (this.CurrentCommandIndex < this.Commands.Count - 1)
        {
            this.Commands.RemoveRange(this.CurrentCommandIndex + 1, this.Commands.Count - this.CurrentCommandIndex - 1);
        }

        if (doExecute)
        {
            command.Execute(arg);
        }
        var stateAfter = this.GetCurrentInvokerState();
        this.Commands.Add((stateBefore, command, stateAfter));
        this.CurrentCommandIndex++;
    }

    /// <summary>
    /// Undo the last command by simply returning the state before the command was executed.
    /// </summary>
    public TState Undo(TArg arg)
    {
        if (this.CurrentCommandIndex >= 0)
        {
            var (stateBefore, command, stateAfter) = this.Commands[this.CurrentCommandIndex];
            this.CurrentCommandIndex--;
            return stateBefore;
        }

        return default;
    }

    public TState Undo(TArg arg, int count)
    {
        TState stateToReturn = default;
        for (int i = 0; i < count; i++)
        {
            stateToReturn = this.Undo(arg);
        }
        return stateToReturn;
    }

    /// <summary>
    /// Redo the last command by simply returning the state after the command was executed.
    /// </summary>
    public TState Redo(TArg arg)
    {
        if (this.CurrentCommandIndex < this.Commands.Count - 1)
        {
            this.CurrentCommandIndex++;
            var (stateBefore, command, stateAfter) = this.Commands[this.CurrentCommandIndex];
            return stateAfter;
        }

        return default;
    }

    public TState Redo(TArg arg, int count)
    {
        TState stateToReturn = default;
        for (int i = 0; i < count; i++)
        {
            stateToReturn = this.Redo(arg);
        }
        return stateToReturn;
    }
}