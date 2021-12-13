using LogiX.SaveSystem;

namespace LogiX.Components;

public abstract class Component
{
    // Logic
    public List<ComponentInput> Inputs { get; private set; }
    public List<ComponentOutput> Outputs { get; private set; }

    // Visual stuff, position etc.
    public Vector2 Position { get; set; }
    public virtual Vector2 Size
    {
        get
        {
            float maxIOs = Math.Max(this.Inputs.Count, this.Outputs.Count);

            return new Vector2(Raylib.MeasureTextEx(Raylib.GetFontDefault(), this.Text, 12, 1).X + 30f, maxIOs * 25f);
        }
    }
    public Rectangle Box
    {
        get
        {
            return new Rectangle(this.Position.X - this.Size.X / 2f, this.Position.Y - this.Size.Y / 2f, this.Size.X, this.Size.Y);
        }
    }
    public virtual Color BodyColor
    {
        get
        {
            return Color.WHITE;
        }
    }
    public virtual string Text => "Component";
    public virtual bool TextVisible => true;

    protected string uniqueID;

    public Component(IEnumerable<int> bitsPerInput, IEnumerable<int> bitsPerOutput, Vector2 position)
    {
        this.Position = position;

        // Creating inputs
        this.Inputs = new List<ComponentInput>();
        for (int i = 0; i < bitsPerInput.Count(); i++)
        {
            ComponentInput ci = new ComponentInput(bitsPerInput.ElementAt(i), $"{i}", this, i);
            this.Inputs.Add(ci);
        }

        // Creating outputs
        this.Outputs = new List<ComponentOutput>();
        for (int i = 0; i < bitsPerOutput.Count(); i++)
        {
            ComponentOutput ci = new ComponentOutput(bitsPerOutput.ElementAt(i), $"{i}", this, i);
            this.Outputs.Add(ci);
        }

        this.uniqueID = Guid.NewGuid().ToString();
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

    public virtual void Update(Vector2 mousePosInWorld)
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

    public bool TryGetIOFromPosition(Vector2 position, List<ComponentIO> cios, Func<int, Tuple<Vector2, Vector2>> getIOLinePositions, out ComponentIO? cio)
    {
        for (int i = 0; i < cios.Count; i++)
        {
            Tuple<Vector2, Vector2> positions = getIOLinePositions(i);
            if (Raylib.CheckCollisionPointCircle(position, positions.Item1, 7f))
            {
                cio = cios[i];
                return true;
            }
        }

        cio = null;
        return false;
    }

    public bool TryGetInputFromPosition(Vector2 position, out ComponentInput? input)
    {
        bool res = TryGetIOFromPosition(position, this.Inputs.Cast<ComponentIO>().ToList(), GetInputLinePositions, out ComponentIO? cio);
        input = cio != null ? (ComponentInput)cio : null;
        return res;
    }

    public bool TryGetOutputFromPosition(Vector2 position, out ComponentOutput? output)
    {
        bool res = TryGetIOFromPosition(position, this.Outputs.Cast<ComponentIO>().ToList(), GetOutputLinePositions, out ComponentIO? cio);
        output = cio != null ? (ComponentOutput)cio : null;
        return res;
    }

    public Tuple<Vector2, Vector2> GetInputLinePositions(int index)
    {
        Vector2 startPos = new Vector2(-10f + this.Box.x, GetIOYPosition(this.Inputs.Count, index) + this.Box.y);
        Vector2 endPos = startPos + new Vector2(10f, 0);
        return Tuple.Create<Vector2, Vector2>(startPos, endPos);
    }

    public Tuple<Vector2, Vector2> GetOutputLinePositions(int index)
    {
        Vector2 startPos = new Vector2(this.Box.x + this.Box.width + 10f, GetIOYPosition(this.Outputs.Count, index) + this.Box.y);
        Vector2 endPos = startPos + new Vector2(-10f, 0);
        return Tuple.Create<Vector2, Vector2>(startPos, endPos);
    }

    public void RenderIO(Func<int, Tuple<Vector2, Vector2>> getIOLinePositions, List<ComponentIO> ios, Vector2 mousePosInWorld)
    {
        for (int i = 0; i < ios.Count; i++)
        {
            ComponentIO cio = ios[i];
            Tuple<Vector2, Vector2> linePositions = getIOLinePositions(i);
            Raylib.DrawLineEx(linePositions.Item1, linePositions.Item2, 1.5f, Color.BLACK);

            Color col = Util.InterpolateColors(Color.WHITE, Color.BLUE, cio.GetHighFraction());

            if (Raylib.CheckCollisionPointCircle(mousePosInWorld, linePositions.Item1, 7f))
            {
                col = Color.ORANGE;
            }

            if (cio.Bits == 1)
            {
                // Render as single bit io
                Raylib.DrawCircleV(linePositions.Item1, 7f, col);
                Raylib.DrawRing(linePositions.Item1, 7f, 8f, 0, 360, 30, Color.BLACK);
                //Raylib.DrawCircleLines((int)linePositions.Item1.X, (int)linePositions.Item1.Y, 7f, Color.BLACK);
            }
            else
            {
                // Render as multibit io
                Raylib.DrawCircleV(linePositions.Item1, 7f, col);
                Raylib.DrawRing(linePositions.Item1, 7f, 8f, 0, 360, 30, Color.BLACK);

                int bitNumberSize = 10;
                Vector2 measure = Raylib.MeasureTextEx(Raylib.GetFontDefault(), cio.Bits.ToString(), bitNumberSize, 1);
                Raylib.DrawTextEx(Raylib.GetFontDefault(), cio.Bits.ToString(), linePositions.Item1 - measure / 2f, bitNumberSize, 1, Color.BLACK);
            }
        }
    }

    public virtual void Render(Vector2 mousePosInWorld)
    {
        Raylib.DrawRectanglePro(this.Box, Vector2.Zero, 0f, this.BodyColor);
        Raylib.DrawRectangleLinesEx(this.Box, 1, Color.BLACK);

        this.RenderIO(GetInputLinePositions, this.Inputs.Cast<ComponentIO>().ToList(), mousePosInWorld);
        this.RenderIO(GetOutputLinePositions, this.Outputs.Cast<ComponentIO>().ToList(), mousePosInWorld);

        if (this.TextVisible)
        {
            Vector2 middleOfBox = new Vector2(this.Box.x, this.Box.y) + new Vector2(this.Box.width / 2f, this.Box.height / 2f);
            Vector2 textSize = Raylib.MeasureTextEx(Raylib.GetFontDefault(), this.Text, 12, 1);

            Raylib.DrawTextEx(Raylib.GetFontDefault(), this.Text, middleOfBox - textSize / 2f, 12, 1, Color.BLACK);
        }
    }

    public virtual void OnSingleSelectedSubmitUI()
    {

    }

    public virtual void RenderSelected()
    {
        int offset = 4;
        Raylib.DrawRectangleLinesEx(new Rectangle(this.Box.x - offset, this.Box.y - offset, this.Size.X + offset * 2, this.Size.Y + offset * 2), 4, Color.ORANGE);
    }

    public abstract ComponentDescription ToDescription();
}