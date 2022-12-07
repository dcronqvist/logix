using System.Drawing;
using System.Numerics;
using System.Text.RegularExpressions;
using ImGuiNET;
using LogiX.Architecture.Commands;
using LogiX.Architecture.Serialization;
using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX.Architecture;

public abstract class Node : Observer
{
    protected Scheduler _scheduler;

    public Vector2i Position { get; set; }
    public int Rotation { get; set; }
    public Guid ID { get; set; }

    public Node()
    {
        this.ID = Guid.NewGuid();
    }

    public abstract void Initialize(INodeDescriptionData data);
    public abstract IEnumerable<PinConfig> GetPinConfiguration();
    protected abstract IEnumerable<(ObservableValue, LogicValue[])> Prepare(PinCollection pins);
    public abstract IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins);
    public abstract INodeDescriptionData GetNodeData();
    public abstract bool IsNodeInRect(RectangleF rect);
    protected abstract bool Interact(Scheduler scheduler, PinCollection pins, Camera2D camera);
    public abstract void RenderSelected(Camera2D camera);
    public abstract Vector2i GetSize();
    public virtual Vector2i GetSizeRotated() => this.GetSize().ApplyRotation(this.Rotation);

    public Vector2 GetMiddleOffset() { return this.GetSizeRotated().ToVector2(Constants.GRIDSIZE) / 2f; }
    /// <summary>
    /// Used to interact with the node in some way.
    /// Must return true if right clicking the node should take precedence over other interactions.
    /// </summary>
    public bool Interact(PinCollection pins, Camera2D camera) => this.Interact(this._scheduler, pins, camera);

    public virtual void Register(Scheduler scheduler) { }

    public virtual void Render(PinCollection pins, Camera2D camera)
    {
        var pos = this.Position;

        if (pins is not null)
        {
            foreach (var (ident, (config, value)) in pins)
            {
                var p = this.GetPinPosition(pins, ident);
                var color = value is null ? ColorF.Black : value.Read().GetValueColor();
                PrimitiveRenderer.RenderCircle(p.ToVector2(Constants.GRIDSIZE), Constants.PIN_RADIUS, 0f, color, 1f);
            }
        }
    }

    public Vector2i GetPinPosition(PinCollection pins, string identifier)
    {
        if (this.Rotation == 0)
        {
            // Just normal
            return this.Position + pins[identifier].Item1.Offset;
        }
        else if (this.Rotation == 1)
        {
            // Left -> Top etc.
            var newOrigin = this.Position + new Vector2i(this.GetSizeRotated().X, 0);
            var offset = pins[identifier].Item1.Offset;
            return newOrigin + new Vector2i(-offset.Y, offset.X);
        }
        else if (this.Rotation == 2)
        {
            // Left -> Right etc.
            var newOrigin = this.Position + new Vector2i(this.GetSizeRotated().X, this.GetSizeRotated().Y);
            var offset = pins[identifier].Item1.Offset;
            return newOrigin + new Vector2i(-offset.X, -offset.Y);
        }
        else
        {
            // Left -> Bottom etc.
            var newOrigin = this.Position + new Vector2i(0, this.GetSizeRotated().Y);
            var offset = pins[identifier].Item1.Offset;
            return newOrigin + new Vector2i(offset.Y, -offset.X);
        }
    }

    public void SetScheduler(Scheduler scheduler)
    {
        this._scheduler = scheduler;
    }

    public override void Update()
    {
        var pins = this._scheduler.GetPinCollectionForNode(this);

        if (pins is null)
            return;

        var ports = this.Evaluate(pins);
        foreach (var (p, v, d) in ports)
        {
            this._scheduler.Schedule(this, p, v, d);
        }
    }

    public void Prepare()
    {
        this.Initialize(this.GetNodeData());
        var ports = this.Prepare(this._scheduler!.GetPinCollectionForNode(this));
        foreach (var (p, v) in ports)
        {
            this._scheduler.Schedule(this, p, v, 1);
        }
    }

    public void Rotate(int rotation)
    {
        int mod(int x, int m)
        {
            return (x % m + m) % m;
        }

        this.Rotation = mod(this.Rotation + rotation, 4);
    }

    public void Move(Vector2i delta)
    {
        this.Position += delta;
    }

    protected string GetNodeTypeID()
    {
        return NodeDescription.GetNodeTypeID(this.GetType());
    }

    public NodeDescription GetDescriptionOfInstance()
    {
        return new NodeDescription(this.GetNodeTypeID(), this.Position, this.Rotation, this.ID, (INodeDescriptionData)Utilities.GetCopyOfInstance(this.GetNodeData()));
    }

    public virtual void CompleteSubmitUISelected(Editor editor, int componentIndex)
    {
        if (ImGui.Begin($"Node Properties: {NodeDescription.GetNodeInfo(this.GetNodeTypeID()).DisplayName}###NODEPROPS", ImGuiWindowFlags.AlwaysAutoResize))
        {
            this.SubmitUISelected(editor, componentIndex);
        }
        ImGui.End();
    }

    public virtual unsafe void SubmitUISelected(Editor editor, int componentIndex)
    {
        var id = this.ID.ToString();

        var data = Utilities.GetCopyOfInstance(this.GetNodeData()) as INodeDescriptionData;
        var props = data.GetType().GetProperties();

        foreach (var prop in props)
        {
            var propType = prop.PropertyType;
            var propValue = prop.GetValue(data);

            var attrib = prop.GetCustomAttributes(typeof(NodeDescriptionPropertyAttribute), false).FirstOrDefault() as NodeDescriptionPropertyAttribute;

            if (attrib is null)
            {
                continue; // Skip properties without the attribute
            }

            var displayName = $"{attrib.DisplayName}##{id}";

            ImGui.PushItemWidth(200);
            if (propType == typeof(int))
            {
                var val = (int)propValue;
                if (ImGui.InputInt(displayName, ref val))
                {
                    val = Math.Clamp(val, attrib.IntMinValue, attrib.IntMaxValue);
                    editor.Execute(new CModifyComponentDataProp(this.ID, prop, val), editor);
                }
            }
            else if (propType == typeof(string))
            {
                string val = (string)propValue;
                var hint = attrib.StringHint;

                var checkRegex = (string s) =>
                {
                    if (attrib.StringRegexFilter is null)
                    {
                        return true;
                    }

                    return Regex.IsMatch(s, attrib.StringRegexFilter);
                };

                ImGuiInputTextCallback callback = (data) =>
                {
                    var addedChar = (char)data->EventChar;
                    if (!checkRegex(val + addedChar))
                    {
                        // Remove the added character
                        return 1;
                    }

                    return 0;
                };

                if (hint is null)
                {
                    // No hint
                    if (attrib.StringMultiline)
                    {
                        if (ImGui.InputTextMultiline(displayName, ref val, attrib.StringMaxLength, new Vector2(300, 150), attrib.StringFlags, callback))
                        {
                            editor.Execute(new CModifyComponentDataProp(this.ID, prop, val), editor);
                        }
                    }
                    else
                    {
                        if (ImGui.InputText(displayName, ref val, attrib.StringMaxLength, attrib.StringFlags, callback))
                        {
                            editor.Execute(new CModifyComponentDataProp(this.ID, prop, val), editor);
                        }
                    }
                }
                else
                {
                    if (ImGui.InputTextWithHint(displayName, attrib.StringHint, ref val, attrib.StringMaxLength, attrib.StringFlags, callback))
                    {
                        editor.Execute(new CModifyComponentDataProp(this.ID, prop, val), editor);
                    }
                }
            }
            else if (propType == typeof(bool))
            {
                bool val = (bool)propValue;
                if (ImGui.Checkbox(displayName, ref val))
                {
                    editor.Execute(new CModifyComponentDataProp(this.ID, prop, val), editor);
                }
            }
            else if (propType == typeof(ColorF))
            {
                ColorF val = (ColorF)propValue;
                var colorV3 = new Vector3(val.R, val.G, val.B);
                if (ImGui.ColorEdit3(displayName, ref colorV3))
                {
                    val = new ColorF(colorV3.X, colorV3.Y, colorV3.Z, val.A);
                    editor.Execute(new CModifyComponentDataProp(this.ID, prop, val), editor);
                }
            }
            else if (propType == typeof(Keys))
            {
                Keys val = (Keys)propValue;
                if (ImGui.Button($"Change hotkey##{id}"))
                {
                    editor.OpenPopup($"Change hotkey", (e) =>
                    {
                        ImGui.Text("Waiting for key press...");
                        if (Input.TryGetNextKeyPressed(out var key))
                        {
                            editor.Execute(new CModifyComponentDataProp(this.ID, prop, key), editor);
                            ImGui.CloseCurrentPopup();
                        }
                    });
                }
                ImGui.SameLine();

                if (val == 0 || val == Keys.Unknown)
                {
                    ImGui.Text("None");
                }
                else
                {
                    ImGui.Text($"{attrib.DisplayName}: {val.PrettifyKey()}");
                }
            }
            else if (propType.IsEnum)
            {
                var val = (int)propValue;
                if (ImGui.Combo(displayName, ref val, propType.GetEnumNames(), propType.GetEnumNames().Length))
                {
                    editor.Execute(new CModifyComponentDataProp(this.ID, prop, val), editor);
                }
            }

            if (attrib.HelpTooltip is not null)
            {
                ImGui.SameLine();
                Utilities.ImGuiHelp(attrib.HelpTooltip);
            }
        }
    }
}

public abstract class Node<T> : Node where T : INodeDescriptionData
{
    public override void Initialize(INodeDescriptionData data)
    {
        this.Initialize((T)data);
    }

    public abstract void Initialize(T data);
}