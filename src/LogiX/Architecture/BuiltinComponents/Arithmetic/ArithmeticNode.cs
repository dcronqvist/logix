using System.Drawing;
using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public class ArithmeticData : INodeDescriptionData
{
    [NodeDescriptionProperty("Bits", IntMinValue = 1, IntMaxValue = 32)]
    public int DataBits { get; set; }

    [NodeDescriptionProperty("Data Pin Mode")]
    public PinModeMulti PinMode { get; set; }

    public virtual INodeDescriptionData GetDefault()
    {
        return new ArithmeticData()
        {
            DataBits = 4,
            PinMode = PinModeMulti.Combined
        };
    }
}

public abstract class ArithmeticNode<TData> : BoxNode<TData> where TData : ArithmeticData
{
    public override float TextScale => 1f;
    protected TData Data { get; private set; }

    protected abstract (uint, bool) Calculate(uint a, uint b, bool carryIn);

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        LogicValue[] a;
        LogicValue[] b;
        LogicValue carryIn = pins.Get("CarryIn").Read().First();

        if (this.Data.PinMode == PinModeMulti.Separate)
        {
            a = Enumerable.Range(0, this.Data.DataBits).Select(x => pins.Get($"A{x}").Read().First()).ToArray();
            b = Enumerable.Range(0, this.Data.DataBits).Select(x => pins.Get($"B{x}").Read().First()).ToArray();
        }
        else
        {
            a = pins.Get("A").Read();
            b = pins.Get("B").Read();
        }

        if (a.Any(x => x == LogicValue.Z) || b.Any(x => x == LogicValue.Z) || carryIn == LogicValue.Z)
        {
            yield return (pins.Get("Result"), LogicValue.Z.Multiple(this.Data.DataBits), 1);
            yield return (pins.Get("CarryOut"), LogicValue.Z.Multiple(1), 1);
            yield break;
        }

        var (result, carryOut) = this.Calculate(a.Reverse().GetAsUInt(), b.Reverse().GetAsUInt(), carryIn.GetAsBool());
        var resultBits = result.GetAsLogicValues(this.Data.DataBits);
        if (this.Data.PinMode == PinModeMulti.Separate)
        {
            for (int i = 0; i < this.Data.DataBits; i++)
            {
                yield return (pins.Get($"Result{i}"), resultBits[i].Multiple(1), 1);
            }
        }
        else
        {
            yield return (pins.Get("Result"), result.GetAsLogicValues(this.Data.DataBits), 1);
        }

        yield return (pins.Get("CarryOut"), (carryOut ? LogicValue.HIGH : LogicValue.LOW).Multiple(1), 1);
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this.Data;
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        if (this.Data.PinMode == PinModeMulti.Separate)
        {
            for (int i = 0; i < this.Data.DataBits; i++)
            {
                yield return new PinConfig($"A{i}", 1, true, new Vector2i(0, i + 1));
                yield return new PinConfig($"B{i}", 1, true, new Vector2i(0, i + this.Data.DataBits + 1));
            }
        }
        else
        {
            yield return new PinConfig("A", this.Data.DataBits, true, new Vector2i(0, 1));
            yield return new PinConfig("B", this.Data.DataBits, true, new Vector2i(0, 2));
        }

        yield return new PinConfig("CarryIn", 1, true, new Vector2i(1, 0));
        yield return new PinConfig("CarryOut", 1, false, new Vector2i(1, this.GetSize().Y));

        if (this.Data.PinMode == PinModeMulti.Separate)
        {
            for (int i = 0; i < this.Data.DataBits; i++)
            {
                yield return new PinConfig($"Result{i}", 1, false, new Vector2i(this.GetSize().X, i + 1));
            }
        }
        else
        {
            yield return new PinConfig("Result", this.Data.DataBits, false, new Vector2i(this.GetSize().X, this.GetSize().Y / 2));
        }
    }

    public override Vector2i GetSize()
    {
        int width = 3;
        int height = 3;

        if (this.Data.PinMode == PinModeMulti.Separate)
        {
            height = this.Data.DataBits * 2 + 1;
        }

        return new Vector2i(width, height);
    }

    public override void Initialize(TData data)
    {
        this.Data = data;
    }

    protected override bool Interact(Scheduler scheduler, PinCollection pins, Camera2D camera)
    {
        return false;
    }

    protected override IEnumerable<(ObservableValue, LogicValue[])> Prepare(PinCollection pins)
    {
        yield break;
    }
}