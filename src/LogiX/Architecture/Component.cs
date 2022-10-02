using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using LogiX.Architecture.Serialization;
using LogiX.GLFW;
using LogiX.Graphics;
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
    private IOMapping _mapping;
    private Dictionary<IO, Wire> _connections = new();

    public Vector2i Position { get; set; }
    public abstract string Name { get; }

    public abstract bool DisplayIOGroupIdentifiers { get; }

    public Component(IOMapping mapping)
    {
        _mapping = mapping;
    }

    private Vector2 _textSize = Vector2.Zero;
    private Vector2i _size = Vector2i.Zero;

    public Vector2i GetSize(out Vector2 textSize)
    {
        if (_size != Vector2i.Zero)
        {
            textSize = _textSize;
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
        _textSize = textMeasure;
        textSize = _textSize;

        if (this.DisplayIOGroupIdentifiers)
        {
            float ioGroupTextScale = 1f;

            var horizontalIOs = leftIOGroups.Concat(rightIOGroups).ToArray();
            var horizontalWidths = horizontalIOs.Select(io => font.MeasureString(io.Identifier, ioGroupTextScale).X);
            var maxHorizontalWidth = (horizontalWidths.Max() * 2f).CeilToMultipleOf(gridSize);

            _size = new Vector2i((int)((width + maxHorizontalWidth) / gridSize), (int)(height / gridSize));
        }


        return _size;
    }

    public Vector2i GetSize()
    {
        return GetSize(out _);
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

    public IOGroup GetGroupForIO(IO io, out int groupIndex)
    {
        int ioIndex = Array.IndexOf(this.IOs, io);

        for (int i = 0; i < _mapping.GetAmountOfGroups(); i++)
        {
            var group = _mapping.GetGroup(i);
            if (group.IOIndices.Contains(ioIndex))
            {
                groupIndex = i;
                return group;
            }
        }
        groupIndex = -1;
        return null;
    }

    public IOGroup GetGroupForIO(IO io)
    {
        return this.GetGroupForIO(io, out _);
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

        return Utilities.GetValueColor(valuesInGroup);
    }

    public Vector2i GetPositionForGroup(IOGroup group, out Vector2i lineEndPosition)
    {
        var index = Array.IndexOf(_mapping.Groups, group);
        return this.GetPositionForGroup(index, out lineEndPosition);
    }

    public Vector2i GetPositionForGroup(int groupIndex, out Vector2i lineEndPosition)
    {
        var group = this._mapping.GetGroup(groupIndex);
        var basePosition = this.Position;
        var size = this.GetSize();

        int onSide = this._mapping.Groups.Where(g => g.Side == group.Side).Count();
        var sideIndex = this._mapping.Groups.Where(g => g.Side == group.Side).ToList().IndexOf(group);

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

    public Vector2i[] GetAllGroupPositions()
    {
        var positions = new List<Vector2i>();
        for (int i = 0; i < _mapping.GetAmountOfGroups(); i++)
        {
            positions.Add(this.GetPositionForGroup(i, out _));
        }
        return positions.ToArray();
    }

    public virtual void Interact(Camera2D cam) { }

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
        var pos = this.Position.ToVector2(16);
        var size = this.GetSize(out var textSize);
        var realSize = size.ToVector2(16);

        var font = LogiX.ContentManager.GetContentItem<Font>("content_1.font.default");
        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");
        var tShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.text");

        var rect = pos.CreateRect(realSize);

        // Draw the component
        var textPos = pos + realSize / 2f - textSize / 2f - new Vector2(0, 1);
        PrimitiveRenderer.RenderRectangle(pShader, Utilities.Inflate(rect, 0, 5), Vector2.Zero, 0f, ColorF.White, camera);
        //PrimitiveRenderer.RenderRectangle(pShader, textPos.CreateRect(textSize), Vector2.Zero, 0f, ColorF.Red, camera);
        TextRenderer.RenderText(tShader, font, this.Name, textPos, 1, ColorF.Black, camera);

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
            var groupCol = this.GetGroupColor(i);

            if ((Input.GetMousePosition(camera) - gPos).Length() <= 4f)
            {
                groupCol = ColorF.Orange;
            }

            PrimitiveRenderer.RenderLine(pShader, gPos, lineEndPos, lineThickness, groupCol.Darken(0.5f), camera);
            PrimitiveRenderer.RenderCircle(pShader, gPos, 4f, 0f, groupCol, camera);

            if (this.DisplayIOGroupIdentifiers)
            {
                var measure = font.MeasureString(group.Identifier, 0.5f);
                var offset = new Vector2(0, 0);

                if (group.Side == ComponentSide.RIGHT)
                {
                    offset = new Vector2(-measure.X, 0);
                }

                TextRenderer.RenderText(tShader, font, group.Identifier, lineEndPos + new Vector2(0, -measure.Y / 2f) + offset, 1, ColorF.Black, camera);
            }
        }

        //this.TriggerSizeRecalculation();
    }

    public IOGroup GetIOGroup(int index)
    {
        return this._mapping.GetGroup(index);
    }

    public bool IsPositionOn(Vector2 mouseWorldPosition)
    {
        var pos = this.Position.ToVector2(16);
        var size = this.GetSize().ToVector2(16);
        var rect = pos.CreateRect(size);

        return rect.Contains(mouseWorldPosition);
    }

    public void Move(Vector2i delta)
    {
        this.Position += delta;
    }

    public void RenderSelected(Camera2D camera)
    {
        // Position of component
        var pos = new Vector2(this.Position.X * 16, this.Position.Y * 16);
        var size = this.GetSize();

        var font = LogiX.ContentManager.GetContentItem<Font>("content_1.font.default");
        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");
        var tShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.text");

        var rect = new RectangleF(pos.X, pos.Y, size.X * 16, size.Y * 16);

        // Draw the component
        PrimitiveRenderer.RenderRectangle(pShader, rect.Inflate(3), Vector2.Zero, 0f, ColorF.Orange, camera);
    }

    public abstract IComponentDescriptionData GetDescriptionData();

    public abstract void Initialize(IComponentDescriptionData data);

    private string GetComponentTypeID()
    {
        return ComponentDescription.GetComponentTypeID(this.GetType());
    }

    public ComponentDescription GetDescriptionOfInstance()
    {
        return new ComponentDescription(this.GetComponentTypeID(), this.Position, this.GetDescriptionData(), this._mapping);
    }
}

public abstract class Component<TData> : Component where TData : IComponentDescriptionData
{
    protected Component(IOMapping mapping) : base(mapping)
    {
    }

    public override void Initialize(IComponentDescriptionData data)
    {
        this.Initialize((TData)data);
    }

    public abstract void Initialize(TData data);
}

// public class ANDGate : Component
// {
//     public override string Name => "AND";

//     public ANDGate(IOMapping mapping, int inputBits = 2) : base(mapping)
//     {
//         for (int i = 0; i < inputBits; i++)
//         {
//             this.RegisterIO($"A{i}", "in");
//         }


//         this.RegisterIO("Y", "out");
//     }

//     public override void PerformLogic()
//     {
//         IO[] inputs = this.GetIOsWithTag("in");
//         IO output = this.GetIOsWithTag("out").First();

//         if (inputs.Any(io => io.GetValue() == LogicValue.UNDEFINED))
//         {
//             return;
//         }

//         int highs = 0;

//         foreach (IO input in inputs)
//         {
//             if (input.GetValue() == LogicValue.HIGH)
//             {
//                 highs++;
//             }
//         }

//         output.Push(highs == inputs.Length ? LogicValue.HIGH : LogicValue.LOW);
//     }

//     public override IComponentDescriptionData GetDescriptionData()
//     {
//         throw new NotImplementedException();
//     }

//     public override void Initialize(IComponentDescriptionData data)
//     {
//         throw new NotImplementedException();
//     }
// }

// public class Switch : Component
// {
//     public override string Name => this._value.ToString().Substring(0, 1);

//     private LogicValue _value = LogicValue.LOW;

//     public Switch(IOMapping mapping) : base(mapping)
//     {
//         this.RegisterIO("Z", "out");
//     }

//     public override void PerformLogic()
//     {
//         var output = this.GetIOsWithTag("out").First();
//         output.Push(this._value);
//     }

//     public override void Interact(Camera2D cam)
//     {
//         if (Input.IsMouseButtonPressed(MouseButton.Left))
//         {
//             var mousePos = Input.GetMousePosition(cam);
//             var size = this.GetSize();

//             var rect = new RectangleF(this.Position.X * 16, this.Position.Y * 16, size.X * 16, size.Y * 16);

//             if (rect.Contains(mousePos))
//             {
//                 this._value = this._value == LogicValue.LOW ? LogicValue.HIGH : LogicValue.LOW;
//             }
//         }
//     }

//     public override IComponentDescriptionData GetDescriptionData()
//     {
//         throw new NotImplementedException();
//     }

//     public override void Initialize(IComponentDescriptionData data)
//     {
//         throw new NotImplementedException();
//     }
// }