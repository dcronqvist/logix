using LogiX.Components;

namespace LogiX.SaveSystem;

public class SplitterDescription : ComponentDescription
{
    public SplitterDescription(Vector2 position, int rotation, List<IODescription> inputs, List<IODescription> outputs) : base(position, inputs, outputs, rotation, ComponentType.Splitter)
    {

    }

    public override Component ToComponent(bool preserveID)
    {
        int inBits = 0;
        int outBits = 0;
        bool multibitIn = false;
        bool multibitOut = false;
        if (this.Inputs[0].Bits > 1)
        {
            // Is Multibit input
            inBits = this.Inputs[0].Bits;
            multibitIn = true;
        }
        else
        {
            // Is single bit input
            inBits = this.Inputs.Count;
            multibitIn = false;
        }

        if (this.Outputs[0].Bits > 1)
        {
            // Is Multibit output
            outBits = this.Outputs[0].Bits;
            multibitOut = true;
        }
        else
        {
            // Is single bit output
            outBits = this.Outputs.Count;
            multibitOut = false;
        }

        Splitter s = new Splitter(inBits, outBits, multibitIn, multibitOut, this.Position);
        if (preserveID)
            s.SetUniqueID(this.ID);
        s.Rotation = Rotation;
        return s;
    }
}