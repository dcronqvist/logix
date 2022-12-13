using System.Reflection;
using LogiX.Architecture.Serialization;

namespace LogiX.Architecture.Commands;

public class CModifyComponentDataProp : Command<Editor>
{
    public Guid Component { get; set; }
    public PropertyInfo Property { get; set; }
    public object Value { get; set; }

    public CModifyComponentDataProp(Guid component, PropertyInfo property, object value)
    {
        this.Component = component;
        this.Property = property;
        this.Value = value;
    }

    public override void Execute(Editor arg)
    {
        arg.Sim.LockedAction(s =>
        {
            var comp = s.GetNodeFromID(this.Component);
            var data = Utilities.GetCopyOfInstance(comp.GetNodeData()) as INodeDescriptionData;
            this.Property.SetValue(data, this.Value);
            comp.Initialize(data);
            s.RecalculateConnectionsInScheduler();
        }, (e) => { throw e; });
    }

    public override string GetDescription()
    {
        return $"Modify {this.Component} {this.Property.Name} to {this.Value}";
    }
}