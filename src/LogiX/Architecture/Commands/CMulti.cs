using LogiX.Architecture.Serialization;

namespace LogiX.Architecture.Commands;

public class CMulti : Command<Editor>
{
    public List<Command<Editor>> Commands { get; set; }
    public string Description { get; set; }

    public CMulti(string desc, params Command<Editor>[] commands)
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

    public override string GetDescription()
    {
        return this.Description;
    }
}