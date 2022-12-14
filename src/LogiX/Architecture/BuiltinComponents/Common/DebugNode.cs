using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

[ScriptType("DEBUG"), NodeInfo("Debug", "Common", "core.markdown.constant")]
public class DebugNode : BoxNode<NoData>
{
    private NoData _data;

    public override string Text => "DEBUG";
    public override float TextScale => 1f;

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        var r = pins.Get("X").Read(this._data.DataBits);
        Console.WriteLine($"DEBUGNODE: {r.GetAsHexString()}");
        yield break;
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        yield return new PinConfig("X", this._data.DataBits, true, new Vector2i(0, 1));
    }

    public override Vector2i GetSize()
    {
        return new Vector2i(4, 2);
    }

    public override void Initialize(NoData data)
    {
        this._data = data;
    }

    protected override bool Interact(Scheduler scheduler, PinCollection pins, Camera2D camera)
    {
        return false; // No interaction.
    }

    protected override IEnumerable<(ObservableValue, LogicValue[])> Prepare(PinCollection pins)
    {
        yield break;
    }
}