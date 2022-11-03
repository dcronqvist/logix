using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Graphics.UI;
using LogiX.Rendering;

namespace LogiX.Architecture;

public class Vector2i
{
    public int X { get; set; }
    public int Y { get; set; }

    public static readonly Vector2i Zero = new Vector2i(0, 0);

    public Vector2i(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override bool Equals(object obj)
    {
        if (obj is Vector2i pos)
        {
            return pos.X == X && pos.Y == Y;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public Vector2 ToVector2(float scale = 1f)
    {
        return new Vector2(X * scale, Y * scale);
    }

    public static Vector2i operator +(Vector2i left, Vector2i right)
    {
        return new Vector2i(left.X + right.X, left.Y + right.Y);
    }

    public static Vector2i operator -(Vector2i left, Vector2i right)
    {
        return new Vector2i(left.X - right.X, left.Y - right.Y);
    }

    public static Vector2i operator -(Vector2i x)
    {
        return new Vector2i(-x.X, -x.Y);
    }

    public static bool operator ==(Vector2i left, Vector2i right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector2i left, Vector2i right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }

    public float Length()
    {
        return (float)Math.Sqrt(X * X + Y * Y);
    }

    public Vector2 Normalized()
    {
        var length = Length();
        return new Vector2(X / length, Y / length);
    }
}

public abstract class Component
{
    // There are no implicit inputs or outputs, since all IOs can be driven to a value.
    private Dictionary<string, IO> _ioDict = new();
    public IO[] IOs => _ioDict.Values.ToArray();

    public Vector2i Position { get; set; }
    public abstract string Name { get; }

    public abstract bool DisplayIOGroupIdentifiers { get; }
    public abstract bool ShowPropertyWindow { get; }

    public Component()
    {

    }

    protected Vector2 _textSize;
    protected RectangleF _bounds;

    public void TriggerSizeRecalculation()
    {
        _bounds = RectangleF.Empty;
        this._ioPositions.Clear();
    }

    public void ClearIOs()
    {
        _ioDict.Clear();
    }

    public void RegisterIO(string identifier, int bits, ComponentSide side, params string[] tags)
    {
        _ioDict.Add(identifier, new IO(identifier, bits, side, tags));
    }

    public IO[] GetIOsOnSide(ComponentSide side)
    {
        return this.IOs.Where(io => io.Side == side).ToArray();
    }

    public IO[] GetIOsWithTag(string tag)
    {
        return _ioDict.Values.Where(io => io.Tags.Contains(tag)).ToArray();
    }

    public IO GetIO(int index)
    {
        return this.IOs[index];
    }

    public IO GetIOFromIdentifier(string id)
    {
        return _ioDict[id];
    }

    public ColorF GetIOColor(int ioIndex)
    {
        var io = this.GetIO(ioIndex);
        return Utilities.GetValueColor(io.GetValues());
    }

    public IEnumerable<Vector2i> GetAllIOPositions()
    {
        if (this._ioPositions.Count == this.IOs.Length)
        {
            return this._ioPositions.Select(x => x.Value.Item1);
        }

        var positions = new List<Vector2i>();
        for (int i = 0; i < this.IOs.Length; i++)
        {
            positions.Add(this.GetPositionForIO(i, out var lineEnd));
        }
        return positions.ToArray();
    }

    public virtual void Interact(Camera2D cam) { }

    public abstract void PerformLogic();

    public int GetIndexOfIO(IO io)
    {
        return Array.IndexOf(this.IOs, io);
    }

    public Vector2i GetPositionForIO(IO io, out Vector2i lineEndPosition)
    {
        return GetPositionForIO(GetIndexOfIO(io), out lineEndPosition);
    }

    private Dictionary<int, (Vector2i, Vector2i)> _ioPositions = new();
    public Vector2i GetPositionForIO(int index, out Vector2i lineEndPosition)
    {
        if (_ioPositions.TryGetValue(index, out var pos))
        {
            lineEndPosition = pos.Item2;
            return pos.Item1;
        }

        var io = this.GetIO(index);
        var basePosition = this.Position;
        var size = this.GetBoundingBox(out _).GetSize().ToVector2i(Constants.GRIDSIZE);

        int onSide = this.IOs.Where(i => i.Side == io.Side).Count();
        var sideIndex = this.IOs.Where(i => i.Side == io.Side).ToList().IndexOf(io);

        var side = io.Side;

        if (side == ComponentSide.LEFT)
        {
            lineEndPosition = new Vector2i(basePosition.X, basePosition.Y + sideIndex);
            this._ioPositions[index] = (new Vector2i(basePosition.X - 1, basePosition.Y + sideIndex), lineEndPosition);
            return new Vector2i(basePosition.X - 1, basePosition.Y + sideIndex);
        }
        else if (side == ComponentSide.RIGHT)
        {
            lineEndPosition = new Vector2i(basePosition.X + size.X, basePosition.Y + sideIndex);
            this._ioPositions[index] = (new Vector2i(basePosition.X + size.X + 1, basePosition.Y + sideIndex), lineEndPosition);
            return new Vector2i(basePosition.X + size.X + 1, basePosition.Y + sideIndex);
        }
        else if (side == ComponentSide.TOP)
        {
            lineEndPosition = new Vector2i(basePosition.X + sideIndex, basePosition.Y);
            this._ioPositions[index] = (new Vector2i(basePosition.X + sideIndex, basePosition.Y - 1), lineEndPosition);
            return new Vector2i(basePosition.X + sideIndex, basePosition.Y - 1);
        }
        else if (side == ComponentSide.BOTTOM)
        {
            lineEndPosition = new Vector2i(basePosition.X + sideIndex, basePosition.Y + size.Y);
            this._ioPositions[index] = (new Vector2i(basePosition.X + sideIndex, basePosition.Y + size.Y + 1), lineEndPosition);
            return new Vector2i(basePosition.X + sideIndex, basePosition.Y + 1 + size.Y);
        }
        else
        {
            throw new Exception("Invalid side");
        }
    }

    public virtual void Update(Simulation simulation, bool performLogic = true)
    {
        var ios = this.IOs;
        var amountOfIOs = ios.Length;
        for (int i = 0; i < amountOfIOs; i++)
        {
            var io = ios[i];
            var ioPos = this.GetPositionForIO(i, out var lineEnd);

            if (simulation.TryGetLogicValuesAtPosition(ioPos, io.Bits, out var values, out var status, out var fromIO, out var fromComp))
            {
                if (fromIO != io)
                    io.SetValues(values);
            }
            else
            {
                io.SetValues(Enumerable.Repeat(LogicValue.UNDEFINED, io.Bits).ToArray());

                if (status == LogicValueRetrievalStatus.DIFF_WIDTH)
                {
                    simulation.AddError(new ReadWrongAmountOfBitsError(this, ioPos, io.Bits, values.Length));
                }
            }
        }

        if (performLogic)
        {
            PerformLogic();
        }

        for (int i = 0; i < amountOfIOs; i++)
        {
            var io = ios[i];
            var ioPos = this.GetPositionForIO(i, out var lineEnd);

            // Process group of IO
            if (io.IsPushing())
            {
                simulation.PushValuesAt(ioPos, io, this, io.GetPushedValues());
                io.SetValues(io.GetPushedValues());
                io.ResetPushed();
            }
        }
    }

    public virtual RectangleF GetBoundingBox(out Vector2 textSize)
    {
        if (this._bounds != RectangleF.Empty)
        {
            textSize = _textSize;
            return this._bounds;
        }

        // Otherwise, calculate the size of the component.
        // In order to trigger a recalculation of the component's size, set _size to Vector2i.Zero.
        var font = LogiX.ContentManager.GetContentItem<Font>("content_1.font.default");
        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shaderprogram.primitive");
        var tShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shaderprogram.text");

        var textScale = 1f;
        var gridSize = Constants.GRIDSIZE;
        var textMeasure = font.MeasureString(Name, textScale);
        var textWidth = textMeasure.X.CeilToMultipleOf(gridSize);
        var textHeight = textMeasure.Y.CeilToMultipleOf(gridSize);

        var topIOs = this.GetIOsOnSide(ComponentSide.TOP);
        var bottomIOs = this.GetIOsOnSide(ComponentSide.BOTTOM);
        var leftIOs = this.GetIOsOnSide(ComponentSide.LEFT);
        var rightIOs = this.GetIOsOnSide(ComponentSide.RIGHT);

        var maxHorizontalIOs = Math.Max(topIOs.Length, bottomIOs.Length) - 1;
        var maxVerticalIOs = Math.Max(leftIOs.Length, rightIOs.Length) - 1;

        var ioWith = maxHorizontalIOs * gridSize;
        var ioHeight = maxVerticalIOs * gridSize;

        var width = Math.Max(textWidth, ioWith);
        var height = Math.Max(textHeight, ioHeight);

        var size = new Vector2i((int)(width / gridSize), (int)(height / gridSize));
        textSize = textMeasure;
        this._textSize = textMeasure;

        // if (this.DisplayIOGroupIdentifiers)
        // {
        //     float ioGroupTextScale = 1f;

        //     var horizontalIOs = leftIOs.Concat(rightIOs).ToArray();
        //     var horizontalWidths = horizontalIOs.Select(io => font.MeasureString(io.Identifier, ioGroupTextScale).X);
        //     var maxHorizontalWidth = (horizontalWidths.Max() * 2f).CeilToMultipleOf(gridSize);

        //     size = new Vector2i((int)((width + maxHorizontalWidth) / gridSize), (int)(height / gridSize));
        // }

        size = new Vector2i(Math.Max(size.X, 1), Math.Max(size.Y, 1));

        var pos = this.Position.ToVector2(Constants.GRIDSIZE);
        var sizeWorld = size.ToVector2(Constants.GRIDSIZE);
        var rect = pos.CreateRect(sizeWorld).Inflate(1);

        this._bounds = rect;

        return rect;
    }

    public virtual void Render(Camera2D camera)
    {
        //this.TriggerSizeRecalculation();
        // Position of component

        var font = LogiX.ContentManager.GetContentItem<Font>("content_1.font.default");
        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");
        var tShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.text");

        var pos = this.Position.ToVector2(Constants.GRIDSIZE);
        var rect = this.GetBoundingBox(out var textSize);
        var size = rect.GetSize().ToVector2i(Constants.GRIDSIZE);
        var realSize = size.ToVector2(Constants.GRIDSIZE);

        // Draw the component
        var textPos = pos + realSize / 2f - textSize / 2f - new Vector2(0, 1);

        var ios = this.IOs;
        for (int i = 0; i < ios.Length; i++)
        {
            var io = ios[i];
            var ioPos = this.GetPositionForIO(io, out var lineEnd);
            var lineEndPos = new Vector2(lineEnd.X * Constants.GRIDSIZE, lineEnd.Y * Constants.GRIDSIZE);

            // Draw the group
            var gPos = new Vector2(ioPos.X * Constants.GRIDSIZE, ioPos.Y * Constants.GRIDSIZE);
            int lineThickness = 2;
            var groupCol = this.GetIOColor(i);

            PrimitiveRenderer.RenderLine(gPos, lineEndPos, lineThickness, groupCol.Darken(0.5f));
            PrimitiveRenderer.RenderCircle(gPos, Constants.IO_GROUP_RADIUS, 0f, groupCol);

            // if (this.DisplayIOGroupIdentifiers)
            // {
            //     var measure = font.MeasureString(io.Identifier, 0.5f);
            //     var offset = new Vector2(2, 0);

            //     if (io.Side == ComponentSide.RIGHT)
            //     {
            //         offset = new Vector2(-measure.X - 2, 0);
            //     }

            //     TextRenderer.RenderText(tShader, font, io.Identifier, gPos - measure / 2f, 0.5f, ColorF.Black, camera);
            // }
        }

        PrimitiveRenderer.RenderRectangle(rect, Vector2.Zero, 0f, ColorF.White);
        //PrimitiveRenderer.RenderRectangle(pShader, textPos.CreateRect(textSize), Vector2.Zero, 0f, ColorF.Red, camera);
        TextRenderer.RenderText(tShader, font, this.Name, textPos, 1, ColorF.Black, camera);

        //this.TriggerSizeRecalculation();
    }

    public bool IsPositionOn(Vector2 mouseWorldPosition)
    {
        var pos = this.Position.ToVector2(Constants.GRIDSIZE);
        var size = this.GetBoundingBox(out _).GetSize();
        var rect = pos.CreateRect(size);

        return rect.Contains(mouseWorldPosition);
    }

    public void Move(Vector2i delta)
    {
        this.Position += delta;
        this._ioPositions = this._ioPositions.ToDictionary(kvp => kvp.Key, kvp => (kvp.Value.Item1 + delta, kvp.Value.Item2 + delta));
        this.TriggerSizeRecalculation();
    }

    public void RenderSelected(Camera2D camera)
    {
        // Position of component
        var font = LogiX.ContentManager.GetContentItem<Font>("content_1.font.default");
        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");
        var tShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.text");

        var rect = this.GetBoundingBox(out _);

        // Draw the component
        PrimitiveRenderer.RenderRectangle(rect.Inflate(1), Vector2.Zero, 0f, Constants.COLOR_SELECTED);
    }

    public abstract IComponentDescriptionData GetDescriptionData();

    public abstract void Initialize(IComponentDescriptionData data);

    private string GetComponentTypeID()
    {
        return ComponentDescription.GetComponentTypeID(this.GetType());
    }

    public ComponentDescription GetDescriptionOfInstance()
    {
        return new ComponentDescription(this.GetComponentTypeID(), this.Position, this.GetDescriptionData());
    }

    public virtual void CompleteSubmitUISelected(Editor editor, int componentIndex)
    {
        if (this.ShowPropertyWindow && ImGui.Begin($"Component Properties", ImGuiWindowFlags.AlwaysAutoResize))
        {
            this.SubmitUISelected(editor, componentIndex);
        }
        ImGui.End();
    }

    public abstract void SubmitUISelected(Editor editor, int componentIndex);

    public string GetUniqueIdentifier()
    {
        return Utilities.GetHash($"{this.GetComponentTypeID()}{this.Position.X}{this.Position.Y}{this.GetHashCode()}");
    }
}

public abstract class Component<TData> : Component where TData : IComponentDescriptionData
{
    public override void Initialize(IComponentDescriptionData data)
    {
        this.Initialize((TData)data);
    }

    public abstract void Initialize(TData data);
}