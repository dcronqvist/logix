using LogiX.Components;

namespace LogiX.SaveSystem;

public enum ArithmeticType
{
    ADDER,
    MULTIPLIER
}

public class ArithmeticDescription : ComponentDescription
{
    [JsonPropertyName("arithType")]
    public ArithmeticType ArithType { get; set; }

    [JsonPropertyName("arithBits")]
    public int ArithBits { get; set; }

    [JsonPropertyName("arithMultibit")]
    public bool ArithMultibit { get; set; }

    public ArithmeticDescription(Vector2 position, int rotation, List<IODescription> inputs, List<IODescription> outputs, ArithmeticType arithType, int arithBits, bool arithMultibit) : base(position, inputs, outputs, rotation, ComponentType.Arithmetic)
    {
        this.ArithType = arithType;
        this.ArithBits = arithBits;
        this.ArithMultibit = arithMultibit;
    }

    public override Component ToComponent(bool preserveIDs)
    {
        Component c = null;
        switch (this.ArithType)
        {
            case ArithmeticType.ADDER:
                c = new AdderComponent(this.ArithBits, this.ArithMultibit, this.Position);
                break;

            case ArithmeticType.MULTIPLIER:
                c = new MultiplierComponent(this.ArithBits, this.ArithMultibit, this.Position);
                break;
        }
        if (preserveIDs)
            c.SetUniqueID(this.ID);
        c.Rotation = Rotation;
        return c;
    }
}