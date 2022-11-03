namespace LogiX.Architecture.Commands;

public abstract class Command<TArg>
{
    public abstract void Execute(TArg arg);
    public abstract void Undo(TArg arg);
    public virtual void Redo(TArg arg) { Execute(arg); }

    public new virtual string ToString()
    {
        return $"{GetType().Name}";
    }
}