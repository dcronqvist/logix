using LogiX.Components;
using QuikGraph;

namespace LogiX.Editor.Commands;

public class MultiCommand<T> : Command<T>
{
    List<Command<T>> commands;
    string description;

    public MultiCommand(string description, params Command<T>[] commands)
    {
        this.commands = new List<Command<T>>(commands);
        this.description = description;
    }

    public override void Execute(T arg)
    {
        for (int i = 0; i < this.commands.Count; i++)
        {
            this.commands[i].Execute(arg);
        }
    }

    public override void Undo(T arg)
    {
        for (int i = this.commands.Count - 1; i >= 0; i--)
        {
            this.commands[i].Undo(arg);
        }
    }

    public override string ToString()
    {
        return this.description;
    }
}