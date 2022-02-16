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

            float ioWidth = this.DrawIOIdentifiers ? GetMaxIOIDWidth() : 0;
            float textWidth = Raylib.MeasureTextEx(Util.OpenSans, this.Text, 18, 1).X;

            return new Vector2(ioWidth * 2f + textWidth + 20f, maxIOs * 25f);
        }
    }
    public Vector2 RotatedSize
    {
        get
        {
            if (this.Rotation == 1 || this.Rotation == 3)
            {
                return new Vector2(this.Size.Y, this.Size.X);
            }
            else
            {
                return this.Size;
            }
        }
    }
    public Rectangle Box
    {
        get
        {
            return new Rectangle(this.Position.X - this.RotatedSize.X / 2f, this.Position.Y - this.RotatedSize.Y / 2f, this.RotatedSize.X, this.RotatedSize.Y);
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
    public virtual bool DrawIOIdentifiers => false;
    public virtual bool DrawBoxNormal => true;
    public virtual bool HasContextMenu => this.AdditionalUISubmitters.Count > 0 || this.Documentation != null;
    public virtual string? Documentation => null;

    public List<Action<Editor.Editor, Component>> AdditionalUISubmitters { get; set; }

    public string uniqueID;

    public int Rotation { get; set; }

    public Component(IEnumerable<int> bitsPerInput, IEnumerable<int> bitsPerOutput, Vector2 position)
    {
        this.Position = position;
        this.AdditionalUISubmitters = Util.GetAdditionalComponentContexts(this.GetType());

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
        this.Rotation = 0;
    }

    public List<LogicValue> GetLogicValuesFromSingleBitInputs(params int[] inputIndices)
    {
        List<LogicValue> values = new List<LogicValue>();
        foreach (int i in inputIndices)
        {
            values.Add(this.Inputs[i].Values[0]);
        }
        return values;
    }

    public virtual int GetMaxStepsToOtherComponent(Component other)
    {
        if (other == this)
        {
            return 1;
        }
        else
        {
            int maxSteps = 0;
            foreach (ComponentOutput co in this.Outputs)
            {
                foreach (Wire w in co.Signals)
                {
                    int steps = w.To.GetMaxStepsToOtherComponent(other);
                    maxSteps = Math.Max(maxSteps, steps);
                }

            }
            return maxSteps + 1;
        }
    }

    internal void AddAdditionalUISubmitter(Action action)
    {
        //this.AdditionalUISubmitters.Add(action);
    }

    public Component SetPosition(Vector2 pos)
    {
        this.Position = pos;
        return this;
    }

    public void SetUniqueID(string id)
    {
        this.uniqueID = id;
    }

    public float GetMaxIOIDWidth()
    {
        float max = 0f;

        foreach (ComponentInput ci in this.Inputs)
        {
            Vector2 measure = Raylib.MeasureTextEx(Util.OpenSans, ci.Identifier, 14, 1);
            max = MathF.Max(max, measure.X);
        }

        foreach (ComponentOutput co in this.Outputs)
        {
            Vector2 measure = Raylib.MeasureTextEx(Util.OpenSans, co.Identifier, 14, 1);
            max = MathF.Max(max, measure.X);
        }

        return max;
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

    public virtual void Update(Vector2 mousePosInWorld, Simulator simulator)
    {
        this.UpdateInputs();
        this.PerformLogic();
        this.UpdateOutputs();
    }

    public abstract void PerformLogic();

    public Vector2 GetInputPosition(int index)
    {
        float dist = 25f;
        float len = 10f;
        int inputs = this.Inputs.Count;

        // Left to right
        if (this.Rotation == 0)
        {
            float x = this.Position.X - this.RotatedSize.X / 2f;
            float y = this.Position.Y - ((inputs - 1) * dist) / 2;
            return new Vector2(x - len, y + (index * dist));
        }
        else if (this.Rotation == 1)
        {
            // Top to bottom
            float x = this.Position.X + ((inputs - 1) * dist) / 2;
            float y = this.Position.Y - this.RotatedSize.Y / 2f;
            return new Vector2(x - (index * dist), y - len);
        }
        else if (this.Rotation == 2)
        {
            // Right to left
            float x = this.Position.X + this.RotatedSize.X / 2f;
            float y = this.Position.Y + ((inputs - 1) * dist) / 2;
            return new Vector2(x + len, y - (index * dist));
        }
        else if (this.Rotation == 3)
        {
            // Bottom to top
            float x = this.Position.X - ((inputs - 1) * dist) / 2;
            float y = this.Position.Y + this.RotatedSize.Y / 2f;
            return new Vector2(x + (index * dist), y + len);
        }

        return Vector2.Zero;
    }

    public Vector2 GetOutputPosition(int index)
    {
        float dist = 25f;
        float len = 10f;
        int outputs = this.Outputs.Count;

        // Left to right
        if (this.Rotation == 0)
        {
            float x = this.Position.X + this.RotatedSize.X / 2f;
            float y = this.Position.Y - ((outputs - 1) * dist) / 2;
            return new Vector2(x + len, y + (index * dist));
        }
        else if (this.Rotation == 1)
        {
            // Top to bottom
            float x = this.Position.X + ((outputs - 1) * dist) / 2;
            float y = this.Position.Y + this.RotatedSize.Y / 2f;
            return new Vector2(x - (index * dist), y + len);
        }
        else if (this.Rotation == 2)
        {
            // Right to left
            float x = this.Position.X - this.RotatedSize.X / 2f;
            float y = this.Position.Y + ((outputs - 1) * dist) / 2;
            return new Vector2(x - len, y - (index * dist));
        }
        else if (this.Rotation == 3)
        {
            // Bottom to top
            float x = this.Position.X + ((outputs - 1) * dist) / 2;
            float y = this.Position.Y - this.RotatedSize.Y / 2f;
            return new Vector2(x - (index * dist), y - len);
        }

        return Vector2.Zero;
    }

    public bool TryGetInputFromPosition(Vector2 position, out ComponentInput? input)
    {
        foreach (ComponentInput ci in this.Inputs)
        {
            if (Raylib.CheckCollisionPointCircle(position, ci.Position, 7f))
            {
                input = ci;
                return true;
            }
        }
        input = null;
        return false;
    }

    public bool TryGetOutputFromPosition(Vector2 position, out ComponentOutput? output)
    {
        foreach (ComponentOutput co in this.Outputs)
        {
            if (Raylib.CheckCollisionPointCircle(position, co.Position, 7f))
            {
                output = co;
                return true;
            }
        }
        output = null;
        return false;
    }

    public Tuple<Vector2, Vector2> GetInputLinePositions(int index)
    {
        Vector2 startPos = this.GetInputPosition(index);
        Vector2 endPos = startPos;
        switch (this.Rotation)
        {
            case 0:
                endPos += new Vector2(10, 0);
                break;
            case 1:
                endPos += new Vector2(0, 10);
                break;
            case 2:
                endPos += new Vector2(-10, 0);
                break;
            case 3:
                endPos += new Vector2(0, -10);
                break;
        }
        return Tuple.Create<Vector2, Vector2>(startPos, endPos);
    }

    public Tuple<Vector2, Vector2> GetOutputLinePositions(int index)
    {
        Vector2 startPos = this.GetOutputPosition(index);
        Vector2 endPos = startPos;
        switch (this.Rotation)
        {
            case 0:
                endPos += new Vector2(-10, 0);
                break;
            case 1:
                endPos += new Vector2(0, -10);
                break;
            case 2:
                endPos += new Vector2(10, 0);
                break;
            case 3:
                endPos += new Vector2(0, 10);
                break;
        }
        return Tuple.Create<Vector2, Vector2>(startPos, endPos);
    }

    public virtual void RenderBox()
    {
        if (this.DrawBoxNormal)
        {
            Raylib.DrawRectanglePro(this.Box, Vector2.Zero, 0f, this.BodyColor);
            Raylib.DrawRectangleLinesEx(this.Box, 1, Color.BLACK);
        }
    }

    public bool IsPositionOnInput(int index, Vector2 pos)
    {
        return (GetInputPosition(index) - pos).Length() < 7f;
    }

    public bool IsPositionOnOutput(int index, Vector2 pos)
    {
        return (GetOutputPosition(index) - pos).Length() < 7f;
    }

    public virtual void RenderIOs(Vector2 mousePosInWorld)
    {
        int ioWidth = 7;

        for (int i = 0; i < this.Inputs.Count; i++)
        {
            (Vector2 start, Vector2 end) = GetInputLinePositions(i);
            Color color = Util.InterpolateColors(Color.WHITE, Color.BLUE, this.Inputs[i].GetHighFraction());

            if ((mousePosInWorld - start).Length() < ioWidth)
            {
                color = Color.ORANGE;
            }

            Raylib.DrawLineEx(start, end, 4f, Color.GRAY);
            Raylib.DrawCircleV(start, ioWidth + 1f, Color.BLACK);
            Raylib.DrawCircleV(start, ioWidth, color);

            int bits = this.InputAt(i).Bits;

            if (bits > 1)
            {
                // Draw text displaying amount of bits
                Vector2 measure = Raylib.MeasureTextEx(Util.OpenSans, bits.ToString(), 13, 0);
                Raylib.DrawTextEx(Util.OpenSans, $"{bits}", start - measure / 2f, 13, 0f, Color.BLACK);
            }

            if (this.DrawIOIdentifiers)
            {
                Vector2 measure = Raylib.MeasureTextEx(Util.OpenSans, this.Inputs[i].Identifier, 13, 0);
                if (this.Rotation == 1 || this.Rotation == 3)
                {
                    this.RenderTextRotated(end + new Vector2(measure.Y / 2f, 5 - (this.Rotation == 3 ? 10 + measure.X : 0)), Util.OpenSans, 13, 0, this.Inputs[i].Identifier, 90f, Color.BLACK);
                }
                else
                {
                    Raylib.DrawTextEx(Util.OpenSans, this.Inputs[i].Identifier, end + new Vector2(5 - (this.Rotation == 2 ? 10 + measure.X : 0), measure.Y / -2f), 13, 0f, Color.BLACK);
                }
            }
        }

        for (int i = 0; i < this.Outputs.Count; i++)
        {
            (Vector2 start, Vector2 end) = GetOutputLinePositions(i);
            Color color = Util.InterpolateColors(Color.WHITE, Color.BLUE, this.Outputs[i].GetHighFraction());

            if ((mousePosInWorld - start).Length() < ioWidth)
            {
                color = Color.ORANGE;
            }

            Raylib.DrawLineEx(start, end, 4f, Color.GRAY);
            Raylib.DrawCircleV(start, ioWidth + 1f, Color.BLACK);
            Raylib.DrawCircleV(start, ioWidth, color);

            int bits = this.OutputAt(i).Bits;

            if (bits > 1)
            {
                // Draw text displaying amount of bits
                Vector2 measure = Raylib.MeasureTextEx(Util.OpenSans, bits.ToString(), 13, 0);
                Raylib.DrawTextEx(Util.OpenSans, $"{bits}", start - measure / 2f, 13, 0f, Color.BLACK);
            }

            if (this.DrawIOIdentifiers)
            {
                Vector2 measure = Raylib.MeasureTextEx(Util.OpenSans, this.Outputs[i].Identifier, 13, 0);
                if (this.Rotation == 1 || this.Rotation == 3)
                {
                    this.RenderTextRotated(end + new Vector2(measure.Y / 2f, 5 + (this.Rotation == 3 ? 0 : -measure.X - 10)), Util.OpenSans, 13, 0, this.Outputs[i].Identifier, 90f, Color.BLACK);
                }
                else
                {
                    Raylib.DrawTextEx(Util.OpenSans, this.Outputs[i].Identifier, end - new Vector2(5 + (this.Rotation == 2 ? -10 : measure.X), measure.Y / 2f), 13, 0f, Color.BLACK);
                }
            }
        }
    }

    public void RotateRight()
    {
        this.Rotation = (this.Rotation + 1) % 4;
    }

    public void RotateLeft()
    {
        this.Rotation = (this.Rotation + 3) % 4;
    }

    public void RenderTextRotated(Vector2 position, Font font, int fontSize, int spacing, string text, float rotation, Color color)
    {
        Rlgl.rlPushMatrix();
        Rlgl.rlTranslatef(position.X, position.Y, 0);
        Rlgl.rlRotatef(rotation, 0, 0, 1);
        Raylib.DrawTextEx(font, text, Vector2.Zero, fontSize, spacing, color);
        Rlgl.rlPopMatrix();
    }

    public virtual void RenderComponentText(Vector2 mousePosInWorld, int fontSize, bool alwaysHorizontalText = false)
    {
        if (this.TextVisible)
        {
            Vector2 middleOfBox = new Vector2(this.Box.x, this.Box.y) + new Vector2(this.Box.width / 2f, this.Box.height / 2f);
            Vector2 textSize = Raylib.MeasureTextEx(Util.OpenSans, this.Text, fontSize, 1);
            Vector2 flippedSize = new Vector2(-textSize.Y, textSize.X);

            if ((this.Rotation == 1 || this.Rotation == 3) && !alwaysHorizontalText)
            {
                // On its side, render text vertically
                Rlgl.rlPushMatrix();
                Vector2 pos = middleOfBox - flippedSize / 2f;
                Rlgl.rlTranslatef(pos.X, pos.Y, 0);
                Rlgl.rlRotatef(90f, 0, 0, 1);
                //
                Raylib.DrawTextEx(Util.OpenSans, this.Text, Vector2.Zero, fontSize, 1, Color.BLACK);
                Rlgl.rlPopMatrix();
            }
            else
            {
                // Straight, render normally
                Raylib.DrawTextEx(Util.OpenSans, this.Text, middleOfBox - textSize / 2f, fontSize, 1, Color.BLACK);
            }
        }
    }

    public virtual void Render(Vector2 mousePosInWorld)
    {
        RenderBox();
        RenderIOs(mousePosInWorld);
        this.RenderComponentText(mousePosInWorld, 18);
    }

    // Is called between Update and Render
    public virtual void Interact(Vector2 mousePosInWorld, Simulator simulator)
    {

    }

    public virtual void OnSingleSelectedSubmitUI()
    {

    }

    public virtual void SubmitContextPopup(LogiX.Editor.Editor editor)
    {
        foreach (Action<Editor.Editor, Component> a in this.AdditionalUISubmitters)
        {
            a(editor, this);
        }

        if (this.Documentation != null)
        {
            if (ImGui.Button("Show Help"))
            {
                editor.currentComponentDocumentation = this;
            }
        }
    }

    public virtual void RenderSelected()
    {
        int offset = 4;
        Raylib.DrawRectangleLinesEx(new Rectangle(this.Box.x - offset, this.Box.y - offset, this.Box.width + offset * 2, this.Box.height + offset * 2), 4, Color.ORANGE);

        // Render all wires from inputs and outputs in orange as well
        foreach (ComponentInput cio in this.Inputs)
        {
            cio.Signal?.RenderSelected();
        }

        foreach (ComponentOutput cio in this.Outputs)
        {
            cio.Signals.ForEach(x => x.RenderSelected());
        }
    }

    public abstract Dictionary<string, int> GetGateAmount();

    public abstract ComponentDescription ToDescription();
}