using LogiX.Components;
using LogiX.SaveSystem;

namespace LogiX.Editor.Commands;

public abstract class Command<TArg>
{
    public abstract void Execute(TArg arg);
    public abstract void Undo(TArg arg);
    public virtual void Redo(TArg arg) { Execute(arg); }

    public virtual string ToString()
    {
        return $"{GetType().Name}";
    }
}