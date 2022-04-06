using LogiX.SaveSystem;

namespace LogiX.Components;

public class IntegratedComponent : Component
{
    private Dictionary<IO, string> _ioToId;
    private Circuit _circuit;
    public Circuit Circuit
    {
        get => _circuit;
        set
        {
            if (this._circuit is null)
            {
                // FIRST TIME

                List<(int, IOConfig, string)> configs = value.GetIOConfigs();

                foreach ((int bits, IOConfig config, string id) in configs)
                {
                    this.AddIO(bits, config);
                    this._ioToId.Add(this.GetIO(this.IOs.Count - 1), id);
                }
            }
            else
            {
                // ALREADY INITIALIZED
                List<(int, IOConfig, string id)> configs = value.GetIOConfigs();

                for (int i = 0; i < configs.Count; i++)
                {
                    if (i < this.IOs.Count)
                    {
                        // Update existing IO
                        this.IOs[i].Item1.UpdateConfig(configs[i].Item2);
                        this._ioToId[this.IOs[i].Item1] = configs[i].Item3;
                    }
                    else
                    {
                        // Add new IO
                        this.AddIO(configs[i].Item1, configs[i].Item2);
                        this._ioToId.Add(this.GetIO(this.IOs.Count - 1), configs[i].Item3);
                    }
                }

                int removedIOs = this.IOs.Count - configs.Count;

                for (int i = 0; i < removedIOs; i++)
                {
                    this._ioToId.Remove(this.GetIO(configs.Count + i));
                    this.IOs.RemoveAt(configs.Count + i);
                }
            }

            this._circuit = value;
            this.simulator = value.GetSimulatorForCircuit();
        }
    }
    Simulator simulator;
    public override string Text => this.Circuit.Name;

    public IntegratedComponent(Vector2 position, Circuit circuit, string? uniqueID = null) : base(position, ComponentType.INTEGRATED, uniqueID)
    {
        this._ioToId = new Dictionary<IO, string>();
        this.Circuit = circuit;
    }

    public void UpdateCircuit(Circuit newCircuit)
    {
        this.Circuit = Circuit;
    }

    public override void PerformLogic()
    {
        this.simulator.PerformLogic();

        for (int i = 0; i < this.IOs.Count; i++)
        {
            IO io = this.GetIO(i);

            if (this.simulator.TryGetComponentByID(this._ioToId[io], out Component? comp))
            {
                if (comp is Switch sw)
                {
                    // This is an input
                    sw.Values = io.Values;
                }
                else
                {
                    // This is an output
                    Lamp la = comp as Lamp;
                    io.PushValues(la.Values);
                }
            }
        }

    }

    public override ComponentDescription ToDescription()
    {
        return new DescriptionIntegrated(this.Position, this.Rotation, this.UniqueID, this.Circuit);
    }
}