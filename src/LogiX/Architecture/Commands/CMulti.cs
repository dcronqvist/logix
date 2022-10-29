using LogiX.Architecture.Serialization;

namespace LogiX.Architecture.Commands;

public class CMulti : Command<Editor>
{
    public List<Command<Editor>> Commands { get; set; }

    public CMulti(params Command<Editor>[] commands)
    {
        this.Commands = new List<Command<Editor>>(commands);
    }

    public override void Execute(Editor arg)
    {
        foreach (var cmd in this.Commands)
        {
            cmd.Execute(arg);
        }
    }

    public override void Undo(Editor arg)
    {
        foreach (var cmd in this.Commands)
        {
            cmd.Undo(arg);
        }
    }
}