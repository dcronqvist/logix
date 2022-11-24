namespace LogiX.Architecture.Commands;

public abstract class Command<TArg>
{
    public abstract void Execute(TArg arg);
    public abstract string GetDescription();
}