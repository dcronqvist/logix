namespace LogiX.Components;

public abstract class Component
{
    // Logic
    public List<ComponentInput> Inputs { get; private set; }
    public List<ComponentOutput> Outputs { get; private set; }

    // Visual stuff, position etc.
    public Vector2 Position { get; set; }
    public Vector2 Size
    {
        get
        {
            float maxIOs = Math.Max(this.Inputs.Count, this.Outputs.Count);

            return new Vector2(100, maxIOs * 25f);
        }
    }
    public Rectangle Box
    {
        get
        {
            return new Rectangle(this.Position.X, this.Position.Y, this.Size.X, this.Size.Y);
        }
    }

    public Component(IEnumerable<int> bitsPerInput, IEnumerable<int> bitsPerOutput, Vector2 position)
    {
        this.Position = position;

        // Creating inputs
        this.Inputs = new List<ComponentInput>();
        for (int i = 0; i < bitsPerInput.Count(); i++)
        {
            ComponentInput ci = new ComponentInput(bitsPerInput.ElementAt(i), $"{i}");
            this.Inputs.Add(ci);
        }

        // Creating outputs
        this.Outputs = new List<ComponentOutput>();
        for (int i = 0; i < bitsPerOutput.Count(); i++)
        {
            ComponentOutput ci = new ComponentOutput(bitsPerOutput.ElementAt(i), $"{i}");
            this.Outputs.Add(ci);
        }
    }

    public void SetInputWire(int index, Wire wire)
    {
        this.Inputs[index].SetSignal(wire);
    }

    public void RemoveInputWire(int index)
    {
        this.Inputs[index].RemoveSignal();
    }

    public void AddOutputWire(int index, Wire wire)
    {
        this.Outputs[index].AddOutputWire(wire);
    }

    public void RemoveOutputWire(int index, Wire wire)
    {
        ComponentOutput co = this.Outputs[index];
        for (int i = 0; i < co.Signals.Count; i++)
        {
            if (wire == co.Signals[i])
            {
                co.RemoveOutputSignal(i);
                return;
            }
        }
    }

    public ComponentInput InputAt(int index)
    {
        return this.Inputs[index];
    }

    public ComponentOutput OutputAt(int index)
    {
        return this.Outputs[index];
    }

    public void UpdateInputs()
    {
        for (int i = 0; i < this.Inputs.Count; i++)
        {
            this.Inputs[i].GetSignalValue();
        }
    }

    public void UpdateOutputs()
    {
        for (int i = 0; i < this.Outputs.Count; i++)
        {
            this.Outputs[i].SetSignals();
        }
    }

    public void Update()
    {
        this.UpdateInputs();
        this.PerformLogic();
        this.UpdateOutputs();
    }

    public abstract void PerformLogic();

    // RENDER RELATED STUFF

    float GetIOYPosition(int ios, int index)
    {
        float dist = 25f;
        float start = this.Size.Y / 2 - ((ios - 1) * dist) / 2;
        float pos = start + index * dist;
        return pos;
    }

    public Tuple<Vector2, Vector2> GetInputLinePositions(int index)
    {
        Vector2 startPos = new Vector2(-10f + this.Position.X, GetIOYPosition(this.Inputs.Count, index) + this.Position.Y);
        Vector2 endPos = startPos + new Vector2(10f, 0);
        return Tuple.Create<Vector2, Vector2>(startPos, endPos);
    }

    public Tuple<Vector2, Vector2> GetOutputLinePositions(int index)
    {
        Vector2 startPos = new Vector2(this.Position.X + this.Size.X + 10f, GetIOYPosition(this.Outputs.Count, index) + this.Position.Y);
        Vector2 endPos = startPos + new Vector2(-10f, 0);
        return Tuple.Create<Vector2, Vector2>(startPos, endPos);
    }

    public void RenderIO(Func<int, Tuple<Vector2, Vector2>> getIOLinePositions, List<ComponentIO> ios)
    {
        for (int i = 0; i < ios.Count; i++)
        {
            ComponentIO cio = ios[i];
            Tuple<Vector2, Vector2> linePositions = getIOLinePositions(i);
            Raylib.DrawLineEx(linePositions.Item1, linePositions.Item2, 1.5f, Color.BLACK);

            if (cio.Bits == 1)
            {
                // Render as single bit io
                Raylib.DrawCircleV(linePositions.Item1, 7f, cio.Values[0] == LogicValue.HIGH ? Color.BLUE : Color.WHITE);
            }
            else
            {
                // Render as multibit io
                Color col = Util.InterpolateColors(Color.WHITE, Color.BLUE, cio.GetHighFraction());
                Raylib.DrawCircleV(linePositions.Item1, 7f, col);
            }
        }
    }

    public void Render()
    {
        Raylib.DrawRectanglePro(this.Box, Vector2.Zero, 0f, Color.WHITE);
        Raylib.DrawRectangleLinesEx(this.Box, 2, Color.BLACK);

        this.RenderIO(GetInputLinePositions, this.Inputs.Cast<ComponentIO>().ToList());
        this.RenderIO(GetOutputLinePositions, this.Outputs.Cast<ComponentIO>().ToList());
    }
}