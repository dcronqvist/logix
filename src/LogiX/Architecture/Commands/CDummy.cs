using System.Reflection;
using LogiX.Architecture.Serialization;

namespace LogiX.Architecture.Commands;

public class CDummy : Command<Editor>
{
    public Action<Editor> Action { get; set; }
    public string Description { get; set; }

    public CDummy(string desc, Action<Editor> action = null)
    {
        this.Action = action;
        this.Description = desc;
    }

    public override void Execute(Editor arg)
    {
        this.Action?.Invoke(arg);
    }

    public override string GetDescription()
    {
        return this.Description;
    }
}