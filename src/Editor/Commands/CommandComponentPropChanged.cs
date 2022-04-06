using System.Reflection;
using LogiX.Components;
using QuikGraph;

namespace LogiX.Editor.Commands;

public class CommandComponentPropChanged : Command<Editor>
{
    public Component component;
    public PropertyInfo prop;
    public object? newVal;
    public object? oldVal;
    public string description;

    public CommandComponentPropChanged(string description, Component component, PropertyInfo prop, object? newVal)
    {
        this.component = component;
        this.prop = prop;
        this.newVal = newVal;
        this.oldVal = prop.GetValue(component);
        this.description = description;
    }

    public override void Execute(Editor arg)
    {
        prop.SetValue(component, newVal);
    }

    public override string ToString()
    {
        return this.description;
    }

    public override void Undo(Editor arg)
    {
        prop.SetValue(component, oldVal);
    }
}