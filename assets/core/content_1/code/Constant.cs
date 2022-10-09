using System.Collections.Generic;
using System.Linq;
using LogiX;
using LogiX.Architecture;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.GLFW;
using LogiX.Graphics.UI;
using LogiX.Rendering;

namespace content_1;

public class ConstData : IComponentDescriptionData
{
    public bool Value { get; set; }

    public static IComponentDescriptionData GetDefault()
    {
        return new ConstData() { Value = true };
    }

    public static IOMapping GetDefaultMapping(IComponentDescriptionData data)
    {
        return IOMapping.FromGroups(IOGroup.FromIndexList("Z", ComponentSide.RIGHT, 0));
    }
}

[ScriptType("CONST")]
public class Constant : Component<ConstData>
{
    public override string Name => this._data.Value ? "1" : "0";
    public override bool DisplayIOGroupIdentifiers => false;
    public override bool ShowPropertyWindow => true;

    public Constant(IOMapping mapping) : base(mapping) { }

    public override void PerformLogic()
    {
        var z = this.GetIOFromIdentifier("Z");

        if (this._data.Value)
        {
            z.Push(LogicValue.HIGH);
        }
        else
        {
            z.Push(LogicValue.LOW);
        }
    }

    private ConstData _data;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return this._data;
    }

    public override void Initialize(ConstData data)
    {
        this._data = data;
        this.RegisterIO("Z", "out");
    }

    public override void Interact(Camera2D cam)
    {
        var rect = this.Position.ToVector2(16).CreateRect(this.GetSize().ToVector2(16));

        if (Input.IsMouseButtonPressed(MouseButton.Right))
        {
            if (rect.Contains(Input.GetMousePosition(cam)))
            {
                this._data.Value = !this._data.Value;
            }
        }
    }

    public override void SubmitUISelected()
    {
        NewGUI.Spacer(5);
        NewGUI.Label($"Value: {(this._data.Value ? "HIGH" : "LOW")}");
    }
}