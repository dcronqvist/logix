using LogiX.Editor.Commands;

namespace LogiX.Editor.StateMachine;

public abstract class Invoker<TArg>
{
    public List<Command<TArg>> Commands { get; set; }
    public int CurrentCommandIndex { get; set; }

    public Invoker()
    {
        this.Commands = new List<Command<TArg>>();
        this.CurrentCommandIndex = -1;
    }

    public void ResetInvoker()
    {
        this.Commands.Clear();
        this.CurrentCommandIndex = -1;
    }

    public void Execute(Command<TArg> command, TArg arg, bool doExecute = true)
    {
        if (this.CurrentCommandIndex < this.Commands.Count - 1)
        {
            this.Commands.RemoveRange(this.CurrentCommandIndex + 1, this.Commands.Count - this.CurrentCommandIndex - 1);
        }

        if (doExecute)
        {
            command.Execute(arg);
        }
        this.Commands.Add(command);
        this.CurrentCommandIndex++;
    }

    public void Undo(TArg arg)
    {
        if (this.CurrentCommandIndex >= 0)
        {
            this.Commands[this.CurrentCommandIndex].Undo(arg);
            this.CurrentCommandIndex--;
        }
    }

    public void Undo(TArg arg, int count)
    {
        for (int i = 0; i < count; i++)
        {
            this.Undo(arg);
        }
    }

    public void Redo(TArg arg)
    {
        if (this.CurrentCommandIndex < this.Commands.Count - 1)
        {
            this.CurrentCommandIndex++;
            this.Commands[this.CurrentCommandIndex].Redo(arg);
        }
    }

    public void Redo(TArg arg, int count)
    {
        for (int i = 0; i < count; i++)
        {
            this.Redo(arg);
        }
    }
}