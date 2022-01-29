using LogiX.Components;

namespace LogiX.SaveSystem;

public class MUXDescription : ComponentDescription
{
    public int selectorBits;
    public bool selectorMultibit;
    public int dataBits;
    public bool dataMultibit;

    public MUXDescription(Vector2 position, int rotation, int selectorBits, bool selectorMultibit, int dataBits, bool dataMultibit, ComponentType type) : base(position,
                                                                                                                                            (type == ComponentType.Mux ?
                                                                                                                                                (Multiplexer.GetBitsPerInput(selectorBits, selectorMultibit, dataBits, dataMultibit).Select(x => new IODescription(x)).ToList()) :
                                                                                                                                                (Demultiplexer.GetBitsPerInput(selectorBits, selectorMultibit, dataBits, dataMultibit).Select(x => new IODescription(x)).ToList())),
                                                                                                                                            (type == ComponentType.Mux ?
                                                                                                                                                (dataMultibit ? Util.Listify(dataBits) : Util.NValues(1, dataBits)).Select(x => new IODescription(x)).ToList() :
                                                                                                                                                (dataMultibit ? Util.NValues(dataBits, (int)Math.Pow(2, selectorBits)) : Util.NValues(1, dataBits * (int)Math.Pow(2, selectorBits))).Select(x => new IODescription(x)).ToList()), rotation, type)
    {
        this.selectorBits = selectorBits;
        this.selectorMultibit = selectorMultibit;
        this.dataBits = dataBits;
        this.dataMultibit = dataMultibit;
    }

    public override Component ToComponent(bool preserveID)
    {
        if (this.Type == ComponentType.Mux)
        {
            Multiplexer m = new Multiplexer(this.selectorBits, this.selectorMultibit, this.dataBits, this.dataMultibit, this.Position);
            if (preserveID)
                m.SetUniqueID(this.ID);
            m.Rotation = Rotation;
            return m;
        }
        else
        {
            Demultiplexer d = new Demultiplexer(this.selectorBits, this.selectorMultibit, this.dataBits, this.dataMultibit, this.Position);
            if (preserveID)
                d.SetUniqueID(this.ID);
            d.Rotation = Rotation;
            return d;
        }
    }
}