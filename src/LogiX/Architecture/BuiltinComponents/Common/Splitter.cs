using System.Drawing;
using System.Numerics;
using ImGuiNET;
using LogiX;
using LogiX.Architecture;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public enum SplitterDirection
{
    Split = 0,
    Combine = 1
}

public class SplitterData : INodeDescriptionData
{
    [NodeDescriptionProperty("Bits", HelpTooltip = "The number of bits to split or combine depending on the direction.", IntMinValue = 1, IntMaxValue = 256)]
    public int BitsToSplit { get; set; }

    [NodeDescriptionProperty("Direction")]
    public SplitterDirection Direction { get; set; }

    public INodeDescriptionData GetDefault()
    {
        return new SplitterData
        {
            BitsToSplit = 4,
            Direction = SplitterDirection.Split
        };
    }
}

[ScriptType("SPLITTER"), NodeInfo("Splitter", "Common", "core.markdown.splitter")]
public class Splitter : BoxNode<SplitterData>
{
    public override string Text => this._data.Direction == SplitterDirection.Split ? "SPLT" : "COMB";
    public override float TextScale => 1f;

    private SplitterData _data;

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        if (this._data.Direction == SplitterDirection.Split)
        {
            var read = pins.Get("multi").Read();

            for (int i = 0; i < this._data.BitsToSplit; i++)
            {
                yield return (pins.Get($"single_{i}"), read[this._data.BitsToSplit - i - 1].Multiple(1), 1);
            }
        }
        else
        {
            var read = new LogicValue[this._data.BitsToSplit];

            for (int i = 0; i < this._data.BitsToSplit; i++)
            {
                read[this._data.BitsToSplit - i - 1] = pins.Get($"single_{i}").Read()[0];
            }

            yield return (pins.Get("multi"), read, 1);
        }
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        if (this._data.Direction == SplitterDirection.Split)
        {
            yield return new PinConfig("multi", this._data.BitsToSplit, true, new Vector2i(0, 1));
            for (int i = 0; i < this._data.BitsToSplit; i++)
            {
                yield return new PinConfig($"single_{i}", 1, false, new Vector2i(3, i + 1));
            }
        }
        else
        {
            yield return new PinConfig("multi", this._data.BitsToSplit, false, new Vector2i(3, 1));
            for (int i = 0; i < this._data.BitsToSplit; i++)
            {
                yield return new PinConfig($"single_{i}", 1, true, new Vector2i(0, i + 1));
            }
        }
    }

    public override Vector2i GetSize()
    {
        return new Vector2i(3, this._data.BitsToSplit + 1);
    }

    public override void Initialize(SplitterData data)
    {
        this._data = data;
    }

    protected override bool Interact(Scheduler scheduler, PinCollection pins, Camera2D camera)
    {
        return false;
    }

    protected override IEnumerable<(ObservableValue, LogicValue[])> Prepare(PinCollection pins)
    {
        return Enumerable.Empty<(ObservableValue, LogicValue[])>();
    }

    // public override string Name => "SPLT";
    // public override bool DisplayIOGroupIdentifiers => true;
    // public override bool ShowPropertyWindow => true;

    // private SplitterData _data;

    // public override IComponentDescriptionData GetDescriptionData()
    // {
    //     return _data;
    // }

    // public override void Initialize(SplitterData data)
    // {
    //     this.ClearIOs();

    //     this._data = data;
    //     this.RegisterIO("multi", data.BitsToSplit, ComponentSide.LEFT);

    //     for (int i = 0; i < data.BitsToSplit; i++)
    //     {
    //         this.RegisterIO($"single_{i}", 1, ComponentSide.RIGHT, "out");
    //     }

    //     this.TriggerSizeRecalculation();
    // }

    // public override void PerformLogic()
    // {
    //     if (this._data.Direction == SplitterDirection.Split)
    //     {
    //         var input = this.GetIOFromIdentifier("multi");
    //         var outputs = this.GetIOsWithTag("out");

    //         var values = input.GetValues();

    //         for (int i = 0; i < values.Length; i++)
    //         {
    //             outputs[i].Push(values[values.Length - i - 1]);
    //         }
    //     }
    //     else
    //     {
    //         var inputs = this.GetIOsWithTag("out");
    //         var output = this.GetIOFromIdentifier("multi");

    //         var values = new LogicValue[inputs.Length];

    //         for (int i = 0; i < inputs.Length; i++)
    //         {
    //             values[values.Length - i - 1] = inputs[i].GetValues()[0];
    //         }

    //         output.Push(values);
    //     }
    // }

    // public override RectangleF GetBoundingBox(out Vector2 textSize)
    // {
    //     if (this._bounds != RectangleF.Empty)
    //     {
    //         textSize = Vector2.Zero;
    //         return this._bounds;
    //     }

    //     textSize = Vector2.Zero;
    //     var bits = this._data.BitsToSplit;

    //     var height = (bits - 1) * Constants.GRIDSIZE;
    //     var pos = this.Position.ToVector2(Constants.GRIDSIZE);

    //     if (this.Rotation == 0 || this.Rotation == 2)
    //     {
    //         this._bounds = pos.CreateRect(new Vector2(Constants.GRIDSIZE, height)).Inflate(1);
    //         return this._bounds;
    //     }
    //     else
    //     {
    //         this._bounds = pos.CreateRect(new Vector2(height, Constants.GRIDSIZE)).Inflate(1);
    //         return this._bounds;
    //     }
    // }

    // public override void Render(Camera2D camera)
    // {
    //     //this.TriggerSizeRecalculation();
    //     // Position of component
    //     var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");
    //     var pos = this.Position.ToVector2(Constants.GRIDSIZE);
    //     var rect = this.GetBoundingBox(out var textSize);
    //     var size = rect.GetSize().ToVector2i(Constants.GRIDSIZE);
    //     var realSize = size.ToVector2(Constants.GRIDSIZE);

    //     var inputPos = this.GetPositionForIO(this.GetIOFromIdentifier("multi"), out var end).ToVector2(Constants.GRIDSIZE);
    //     var halfWay = this.Rotation switch
    //     {
    //         0 => end.ToVector2(Constants.GRIDSIZE) + new Vector2(Constants.GRIDSIZE / 2f, 0f),
    //         1 => end.ToVector2(Constants.GRIDSIZE) + new Vector2(0f, Constants.GRIDSIZE / 2f),
    //         2 => end.ToVector2(Constants.GRIDSIZE) - new Vector2(Constants.GRIDSIZE / 2f, 0f),
    //         3 => end.ToVector2(Constants.GRIDSIZE) - new Vector2(0f, Constants.GRIDSIZE / 2f),
    //         _ => throw new NotImplementedException("Received abnormal rotation for splitter.") // Should never happen
    //     };

    //     var bits = this._data.BitsToSplit - 1;

    //     // PrimitiveRenderer.RenderRectangle(pShader, rect, Vector2.Zero, 0f, ColorF.White, camera);

    //     var ios = this.IOs;
    //     for (int i = 1; i < ios.Length; i++)
    //     {
    //         var io = ios[i];
    //         var ioPos = this.GetPositionForIO(io, out var lineEnd);
    //         var offset = this.Rotation switch
    //         {
    //             0 => -new Vector2(Constants.GRIDSIZE / 2f, 0),
    //             1 => -new Vector2(0f, Constants.GRIDSIZE / 2f),
    //             2 => new Vector2(Constants.GRIDSIZE / 2f, 0),
    //             3 => new Vector2(0f, Constants.GRIDSIZE / 2f),
    //             _ => Vector2.Zero
    //         };
    //         var lineEndPos = new Vector2(lineEnd.X * Constants.GRIDSIZE, lineEnd.Y * Constants.GRIDSIZE) + offset;

    //         // Draw the group
    //         var gPos = new Vector2(ioPos.X * Constants.GRIDSIZE, ioPos.Y * Constants.GRIDSIZE);
    //         int lineThickness = 2;
    //         var groupCol = this.GetIOColor(i);

    //         PrimitiveRenderer.RenderLine(gPos, lineEndPos, lineThickness, groupCol.Darken(0.5f));
    //         PrimitiveRenderer.RenderCircle(gPos, Constants.IO_GROUP_RADIUS, 0f, groupCol);
    //     }

    //     var col = this.GetIOColor(0);
    //     PrimitiveRenderer.RenderLine(inputPos, halfWay, 2, col.Darken(0.5f));
    //     PrimitiveRenderer.RenderCircle(inputPos, Constants.IO_GROUP_RADIUS, 0f, col);

    //     if (this.Rotation == 0 || this.Rotation == 2)
    //     {
    //         PrimitiveRenderer.RenderLine(halfWay - new Vector2(0, 1), new Vector2(halfWay.X, halfWay.Y + bits * Constants.GRIDSIZE + 1), 2, ColorF.Black);
    //     }
    //     else
    //     {
    //         PrimitiveRenderer.RenderLine(halfWay - new Vector2(1, 0), new Vector2(halfWay.X + bits * Constants.GRIDSIZE + 1, halfWay.Y), 2, ColorF.Black);
    //     }
    // }
}