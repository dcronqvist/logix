using LogiX.Components;

namespace LogiX.SaveSystem;

public class GenIODescription : ComponentDescription
{
    public GenIODescription(Vector2 position, List<IODescription> inputs, List<IODescription> outputs, ComponentType ct) : base(position, inputs, outputs, ct)
    {

    }

    public override Component ToComponent()
    {
        switch (this.Type)
        {
            case ComponentType.Button:
                return new Button(this.Outputs[0].Bits, this.Position);

            case ComponentType.HexViewer:
                bool singlebit = this.Inputs.Count == 1 && this.Inputs[0].Bits == 1;

                if (singlebit)
                {
                    return new HexViewer(1, false, this.Position);
                }
                else
                {
                    return new HexViewer(this.Inputs.Count, true, this.Position);
                }

        }

        return null;
    }
}