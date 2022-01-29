using LogiX.Components;

namespace LogiX.SaveSystem;

public class GenIODescription : ComponentDescription
{
    public GenIODescription(Vector2 position, int rotation, List<IODescription> inputs, List<IODescription> outputs, ComponentType type) : base(position, inputs, outputs, rotation, type)
    {

    }

    public override Component ToComponent(bool preserveIDs)
    {
        Component c = null;
        switch (this.Type)
        {
            case ComponentType.Button:
                c = new Button(this.Outputs[0].Bits, this.Position);
                break;

            case ComponentType.HexViewer:
                bool multibit = this.Inputs.Count == 1;

                if (multibit)
                {
                    c = new HexViewer(this.Inputs[0].Bits, true, this.Position);
                }
                else
                {
                    c = new HexViewer(this.Inputs.Count, false, this.Position);
                }
                break;

        }

        if (preserveIDs)
            c.SetUniqueID(this.ID);

        c.Rotation = Rotation;

        return c;
    }
}