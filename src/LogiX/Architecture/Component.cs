using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX;

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
}

public abstract class Component
{
    // There are no implicit inputs or outputs, since all IOs can be driven to a value.
    private Dictionary<string, IO> _ioDict = new();
    public IO[] IOs => _ioDict.Values.ToArray();
    private IOMapping _mapping;
    private Dictionary<IO, Wire> _connections = new();

    public Vector2i Position { get; set; }
    public abstract string Name { get; }

    public Component(IOMapping mapping)
    {
        _mapping = mapping;
    }

    private Vector2i _size = Vector2i.Zero;

    public Vector2i GetSize()
    {
        if (_size != Vector2i.Zero)
        {
            return _size;
        }

        // Otherwise, calculate the size of the component.
        // In order to trigger a recalculation of the component's size, set _size to Vector2i.Zero.

        var font = LogiX.ContentManager.GetContentItem<Font>("content_1.font.default");
        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shaderprogram.primitive");
        var tShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shaderprogram.text");

        var textScale = 1f;
        var gridSize = 16;
        var textMeasure = font.MeasureString(Name, textScale);
        var textWidth = textMeasure.X.CeilToMultipleOf(gridSize);
        var textHeight = textMeasure.Y.CeilToMultipleOf(gridSize);

        var topIOGroups = this.GetIOGroupsOnSide(ComponentSide.TOP);
        var bottomIOGroups = this.GetIOGroupsOnSide(ComponentSide.BOTTOM);
        var leftIOGroups = this.GetIOGroupsOnSide(ComponentSide.LEFT);
        var rightIOGroups = this.GetIOGroupsOnSide(ComponentSide.RIGHT);

        var maxHorizontalIOs = Math.Max(topIOGroups.Length, bottomIOGroups.Length) - 1;
        var maxVerticalIOs = Math.Max(leftIOGroups.Length, rightIOGroups.Length) - 1;

        var ioWith = maxHorizontalIOs * gridSize;
        var ioHeight = maxVerticalIOs * gridSize;

        var width = Math.Max(textWidth, ioWith);
        var height = Math.Max(textHeight, ioHeight);

        _size = new Vector2i((int)(width / gridSize), (int)(height / gridSize));
        return _size;
    }

    public void TriggerSizeRecalculation()
    {
        _size = Vector2i.Zero;
    }

    public void RegisterIO(string identifier, params string[] tags)
    {
        _ioDict.Add(identifier, new IO(tags));
    }

    public IO[] GetIOsWithTag(string tag)
    {
        return _ioDict.Values.Where(io => io.Tags.Contains(tag)).ToArray();
    }

    public IO GetIO(int index)
    {
        return this.IOs[index];
    }

    public IOGroup GetGroupForIO(IO io)
    {
        int ioIndex = Array.IndexOf(this.IOs, io);

        for (int i = 0; i < _mapping.GetAmountOfGroups(); i++)
        {
            var group = _mapping.GetGroup(i);
            if (group.IOIndices.Contains(ioIndex))
            {
                return group;
            }
        }
        return null;
    }

    public IOGroup[] GetIOGroupsOnSide(ComponentSide side)
    {
        var groups = new List<IOGroup>();
        for (int i = 0; i < _mapping.GetAmountOfGroups(); i++)
        {
            var group = _mapping.GetGroup(i);
            if (group.Side == side)
            {
                groups.Add(group);
            }
        }
        return groups.ToArray();
    }

    public IO[] GetIOsOnSide(ComponentSide side)
    {
        return _ioDict.Values.Where(io => GetGroupForIO(io).Side == side).ToArray();
    }

    public void ConnectIO(int ioIndexThis, Wire wire)
    {
        IO thisIO = this.GetIO(ioIndexThis);
        _connections.Add(thisIO, wire);
    }

    public IO GetIOFromIdentifier(string id)
    {
        return _ioDict[id];
    }

    public ColorF GetGroupColor(int groupIndex)
    {
        var group = _mapping.GetGroup(groupIndex);
        var iosInGroup = group.IOIndices.Select(x => this.GetIO(x)).ToArray();
        var valuesInGroup = iosInGroup.Select(x => x.GetValue()).ToArray();

        if (valuesInGroup.All(x => x == LogicValue.UNDEFINED))
        {
            return ColorF.Gray;
        }

        var amountHigh = valuesInGroup.Count(x => x == LogicValue.HIGH);

        return ColorF.Lerp(ColorF.White, ColorF.Blue, (float)amountHigh / valuesInGroup.Length);
    }

    private Vector2i GetPositionForGroup(int groupIndex, out Vector2i lineEndPosition)
    {
        var group = this._mapping.GetGroup(groupIndex);
        var basePosition = this.Position;
        var size = this.GetSize();

        int onSide = this._mapping.Mapping.Where(g => g.Side == group.Side).Count();
        var sideIndex = this._mapping.Mapping.Where(g => g.Side == group.Side).ToList().IndexOf(group);

        var side = group.Side;

        if (side == ComponentSide.LEFT)
        {
            lineEndPosition = new Vector2i(basePosition.X, basePosition.Y + sideIndex);
            return new Vector2i(basePosition.X - 1, basePosition.Y + sideIndex);
        }
        else if (side == ComponentSide.RIGHT)
        {
            lineEndPosition = new Vector2i(basePosition.X + size.X, basePosition.Y + sideIndex);
            return new Vector2i(basePosition.X + size.X + 1, basePosition.Y + sideIndex);
        }
        else if (side == ComponentSide.TOP)
        {
            lineEndPosition = new Vector2i(basePosition.X + sideIndex, basePosition.Y);
            return new Vector2i(basePosition.X + sideIndex, basePosition.Y - 1);
        }
        else if (side == ComponentSide.BOTTOM)
        {
            lineEndPosition = new Vector2i(basePosition.X + sideIndex, basePosition.Y + size.Y);
            return new Vector2i(basePosition.X + sideIndex, basePosition.Y + 1 + size.Y);
        }
        else
        {
            throw new Exception("Invalid side");
        }
    }

    public abstract void PerformLogic();

    public void Update(Simulation simulation)
    {
        var amountOfGroups = this._mapping.GetAmountOfGroups();
        for (int i = 0; i < amountOfGroups; i++)
        {
            var group = this._mapping.GetGroup(i);
            var iosInGroup = group.IOIndices.Select(i => this.GetIO(i)).ToArray();
            var groupPos = this.GetPositionForGroup(i, out var lineEnd);

            if (simulation.TryGetLogicValuesAtPosition(groupPos, iosInGroup.Length, out var values, out var status))
            {
                for (int j = 0; j < values.Length; j++)
                {
                    iosInGroup[j].SetValue(values[j]);
                }
            }
            else
            {
                for (int j = 0; j < iosInGroup.Length; j++)
                {
                    iosInGroup[j].SetValue(LogicValue.UNDEFINED);
                }
            }
        }

        PerformLogic();

        amountOfGroups = this._mapping.GetAmountOfGroups();
        for (int i = 0; i < amountOfGroups; i++)
        {
            var group = this._mapping.GetGroup(i);
            var iosInGroup = group.IOIndices.Select(i => this.GetIO(i)).ToArray();

            // Process group of IO
            List<LogicValue> values = new();
            foreach (IO io in iosInGroup)
            {
                if (io.IsPushing())
                {
                    values.Add(io.GetPushedValue());
                    io.SetValue(io.GetPushedValue());
                    io.ResetPushed();
                }
            }

            var groupPos = this.GetPositionForGroup(i, out var lineEnd);

            if (values.Count > 0)
            {
                simulation.PushValuesAt(groupPos, values.ToArray());
            }
        }
    }

    public void Render(Camera2D camera)
    {
        // Position of component
        var pos = new Vector2(this.Position.X * 16, this.Position.Y * 16);
        var size = this.GetSize();

        var font = LogiX.ContentManager.GetContentItem<Font>("content_1.font.default");
        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");
        var tShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.text");

        var rect = new RectangleF(pos.X, pos.Y, size.X * 16, size.Y * 16);

        // Draw the component
        PrimitiveRenderer.RenderRectangle(pShader, rect, Vector2.Zero, 0f, ColorF.White, camera);
        TextRenderer.RenderText(tShader, font, this.Name, pos, 1f, ColorF.Black, camera);

        var amountOfGroups = this._mapping.GetAmountOfGroups();
        for (int i = 0; i < amountOfGroups; i++)
        {
            var group = this._mapping.GetGroup(i);
            var iosInGroup = group.IOIndices.Select(i => this.GetIO(i)).ToArray();
            var groupPos = this.GetPositionForGroup(i, out var lineEnd);
            var lineEndPos = new Vector2(lineEnd.X * 16, lineEnd.Y * 16);

            // Draw the group
            var gPos = new Vector2(groupPos.X * 16, groupPos.Y * 16);
            int lineThickness = 2;
            PrimitiveRenderer.RenderLine(pShader, gPos, lineEndPos, lineThickness, ColorF.Green, camera);
            PrimitiveRenderer.RenderCircle(pShader, gPos, 4f, 0f, this.GetGroupColor(i), camera);
        }
    }
}

public class ANDGate : Component
{
    public override string Name => "AND";

    public ANDGate(IOMapping mapping, int inputBits = 2) : base(mapping)
    {
        for (int i = 0; i < inputBits; i++)
        {
            this.RegisterIO($"A{i}", "in");
        }


        this.RegisterIO("Y", "out");
    }

    public override void PerformLogic()
    {
        IO[] inputs = this.GetIOsWithTag("in");
        IO output = this.GetIOsWithTag("out").First();

        if (inputs.Any(io => io.GetValue() == LogicValue.UNDEFINED))
        {
            // Do not push anything
            return;
        }

        int highs = 0;

        foreach (IO input in inputs)
        {
            if (input.GetValue() == LogicValue.HIGH)
            {
                highs++;
            }
        }

        output.Push(highs == inputs.Length ? LogicValue.HIGH : LogicValue.LOW);
    }
}