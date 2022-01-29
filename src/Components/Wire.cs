using LogiX.SaveSystem;
using LogiX.Editor;

namespace LogiX.Components;

public class Wire
{
    public int Bits { get; private set; }
    public List<LogicValue> Values { get; private set; }

    public Component To { get; private set; }
    public int ToIndex { get; private set; }
    public Component From { get; private set; }
    public int FromIndex { get; private set; }

    public List<Vector2> IntermediatePoints { get; set; }

    public Wire(int bits, Component to, int toIndex, Component from, int fromIndex)
    {
        this.Bits = bits;
        this.Values = Util.NValues(LogicValue.LOW, bits);
        this.To = to;
        this.From = from;
        this.ToIndex = toIndex;
        this.FromIndex = fromIndex;
        this.IntermediatePoints = new List<Vector2>();
    }

    public void SetValues(IEnumerable<LogicValue> values)
    {
        if (values.Count() != this.Bits)
        {
            throw new ArgumentException("The number of values does not match the number of bits.");
        }

        this.Values = new List<LogicValue>(values);
    }

    public void SetAllValues(LogicValue value)
    {
        this.Values = Enumerable.Repeat(value, this.Bits).ToList();
    }

    public float GetHighFraction()
    {
        int highCount = 0;

        foreach (LogicValue value in this.Values)
        {
            if (value == LogicValue.HIGH)
            {
                highCount++;
            }
        }

        return (float)highCount / (float)this.Bits;
    }

    public bool IsPositionOnWire(Vector2 pos, out Vector2 lStart, out Vector2 lEnd)
    {
        float width = 4f;

        bool posIsOnOutput = this.From.IsPositionOnOutput(this.FromIndex, pos);
        bool posIsOnInput = this.To.IsPositionOnInput(this.ToIndex, pos);
        bool posIsOnIO = posIsOnOutput || posIsOnInput;

        // Get distance to line describing wire, if less than width, then on wire
        if (this.IntermediatePoints.Count == 0)
        {
            // Single line, should be easy.
            Vector2 lineStart = this.From.GetOutputLinePositions(this.FromIndex).Item1;
            Vector2 lineEnd = this.To.GetInputLinePositions(this.ToIndex).Item1;

            if (!Raylib.CheckCollisionPointRec(pos, Util.CreateRecFromTwoCorners(lineStart.Vector2Towards(5, lineEnd), lineEnd.Vector2Towards(5, lineStart), width / 2)))
            {
                lStart = lEnd = Vector2.Zero;
                return false;
            }

            float distance = Util.DistanceToLine(lineEnd, lineStart, pos);
            if (distance < width)
            {
                lStart = lineStart;
                lEnd = lineEnd;
                return true && !this.IsPositionOnIntermediatePoint(pos, out Vector2 iPoint) && !posIsOnIO;
            }
        }
        else
        {
            // Multiple lines, need to check each line.
            Vector2 start = this.From.GetOutputLinePositions(this.FromIndex).Item1;
            Vector2 end = this.To.GetInputLinePositions(this.ToIndex).Item1;

            List<Vector2> linePoints = IntermediatePoints.Concat(new List<Vector2>() { end }).ToList();
            Vector2 previousPos = start;
            for (int i = 0; i < linePoints.Count; i++)
            {
                Rectangle rec = Util.CreateRecFromTwoCorners(previousPos.Vector2Towards(5, linePoints[i]), linePoints[i].Vector2Towards(5, previousPos), width / 2);
                if (Raylib.CheckCollisionPointRec(pos, rec))
                {
                    if (Util.DistanceToLine(previousPos, linePoints[i], pos) < width)
                    {
                        lStart = previousPos;
                        lEnd = linePoints[i];
                        return true && !this.IsPositionOnIntermediatePoint(pos, out Vector2 iPoint) && !posIsOnIO;
                    }
                }
                previousPos = linePoints[i];
            }
        }

        lStart = Vector2.Zero;
        lEnd = Vector2.Zero;
        return false;
    }

    public bool IsPositionOnIntermediatePoint(Vector2 pos, out Vector2 point)
    {
        foreach (Vector2 p in this.IntermediatePoints)
        {
            if ((p - pos).Length() < 5)
            {
                point = p;
                return true;
            }
        }
        point = Vector2.Zero;
        return false;
    }

    public void Update(Vector2 mousePosInWorld, Simulator simulator)
    {
        if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON) && this.IsPositionOnWire(mousePosInWorld, out Vector2 lStart, out Vector2 lEnd))
        {
            // If mouse is on wire, update intermediate points.
            int index = this.IntermediatePoints.IndexOf(lStart);
            this.IntermediatePoints.Insert(index + 1, mousePosInWorld);
            simulator.SelectedWirePoints.Clear();
            simulator.SelectedWirePoints.Add((this, index + 1));
        }

        if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON) && this.IsPositionOnIntermediatePoint(mousePosInWorld, out Vector2 point))
        {
            // If mouse is on wire, remove intermediate point
            int index = this.IntermediatePoints.IndexOf(point);
            if (simulator.SelectedWirePoints.Contains((this, index)))
            {
                simulator.SelectedWirePoints.Remove((this, index));
            }
            // Check if any of the following intermediate points on this wire is in the selected wire points.
            // If any are, then decrement their index to fit in the new list.
            for (int i = index; i < this.IntermediatePoints.Count; i++)
            {
                if (simulator.SelectedWirePoints.Contains((this, i)))
                {
                    simulator.SelectedWirePoints.Remove((this, i));
                    simulator.SelectedWirePoints.Add((this, i - 1));
                }
            }

            this.IntermediatePoints.Remove(point);
        }
    }

    public void AddIntermediatePoint(Vector2 pos)
    {
        this.IntermediatePoints.Add(pos);
    }

    public (Vector2, Vector2) GetAdjacentPositionsToIntermediate(Vector2 point)
    {
        int index = this.IntermediatePoints.IndexOf(point);
        if (index == 0)
        {
            return (this.From.GetOutputPosition(this.FromIndex), this.IntermediatePoints.Count > 1 ? this.IntermediatePoints[1] : this.To.GetInputPosition(this.ToIndex));
        }
        else if (index == this.IntermediatePoints.Count - 1)
        {
            return (this.IntermediatePoints.Count > 1 ? this.IntermediatePoints[this.IntermediatePoints.Count - 2] : this.From.GetOutputPosition(this.FromIndex), this.To.GetInputPosition(this.ToIndex));
        }
        else
        {
            return (this.IntermediatePoints[index - 1], this.IntermediatePoints[index + 1]);
        }
    }

    public void Render(Vector2 mousePosInWorld)
    {
        Vector2 fromPos = this.From.GetOutputLinePositions(this.FromIndex).Item1;
        Vector2 toPos = this.To.GetInputLinePositions(this.ToIndex).Item1;
        float width = 4f;
        Color color = Util.InterpolateColors(Color.WHITE, Color.BLUE, this.GetHighFraction());

        List<Vector2> linePoints = IntermediatePoints.Concat(new List<Vector2>() { toPos }).ToList();
        Vector2 previousPos = fromPos;
        for (int i = 0; i < linePoints.Count; i++)
        {
            Raylib.DrawLineEx(previousPos, linePoints[i], width + 2f, Color.BLACK);
            Raylib.DrawLineEx(previousPos, linePoints[i], width, color);
            Rectangle rec = Util.CreateRecFromTwoCorners(previousPos.Vector2Towards(5, linePoints[i]), linePoints[i].Vector2Towards(5, previousPos), width / 2);
            //Raylib.DrawRectangleRec(rec, Color.BLUE.Opacity(0.3f));
            previousPos = linePoints[i];
        }

        if (this.IsPositionOnWire(mousePosInWorld, out Vector2 lStart, out Vector2 lEnd))
        {
            Raylib.DrawLineEx(lStart, lEnd, width + 2f, Color.ORANGE);
            Raylib.DrawLineEx(lStart, lEnd, width, color);
        }

        for (int i = 0; i < linePoints.Count - 1; i++)
        {
            Raylib.DrawCircleV(linePoints[i], width + 1f, Color.BLACK);
            Raylib.DrawCircleV(linePoints[i], width, color);
        }

        if (this.IsPositionOnIntermediatePoint(mousePosInWorld, out Vector2 point))
        {
            Raylib.DrawCircleV(point, width, Color.ORANGE);
        }
    }

    public void RenderSelected()
    {
        Vector2 fromPos = this.From.GetOutputLinePositions(this.FromIndex).Item1;
        Vector2 toPos = this.To.GetInputLinePositions(this.ToIndex).Item1;
        //Raylib.DrawLineEx(fromPos, toPos, 6f, Color.ORANGE);
        //Raylib.DrawLineEx(fromPos, toPos, 4, Util.InterpolateColors(Color.WHITE, Color.BLUE, this.GetHighFraction()));
    }
}