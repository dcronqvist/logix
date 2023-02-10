using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using ImGuiNET;
using LogiX.Architecture.Commands;
using LogiX.Architecture.Serialization;
using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX.Architecture;

public abstract class Node : Observer<IEnumerable<(ValueEvent, int)>>
{
    protected Scheduler _scheduler;

    public Vector2i Position { get; set; }
    public int Rotation { get; set; }
    public Guid ID { get; set; }

    public virtual bool DisableSelfEvaluation { get; } = false;

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

    public void TriggerEvaluationNextTick()
    {
        this._scheduler.ForceEvaluationNextStep(this);
    }

    public Vector2 GetMiddleOffset() { return this.GetSizeRotated().ToVector2(Constants.GRIDSIZE) / 2f; }
    /// <summary>
    /// Used to interact with the node in some way.
    /// Must return true if right clicking the node should take precedence over other interactions.
    /// </summary>
    public bool Interact(PinCollection pins, Camera2D camera) => this.Interact(this._scheduler, pins, camera);

    public virtual void Register(Scheduler scheduler) { }

    public virtual void Unregister(Scheduler scheduler) { }

    public virtual void Render(PinCollection pins, Camera2D camera)
    {
        var pos = this.Position;

        if (pins is not null)
        {
            foreach (var (ident, (config, value)) in pins)
            {
                var p = this.GetPinPosition(pins, ident);
                var color = value is null ? ColorF.Black : value.Read(config.Bits).GetValueColor();
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

    public override IEnumerable<(ValueEvent, int)> Update(Observer<IEnumerable<(ValueEvent, int)>> origin)
    {
        if (this.DisableSelfEvaluation && origin == this)
        {
            yield break;
        }

        var pins = this._scheduler.GetPinCollectionForNode(this);

        if (pins is null)
            yield break;

        var ports = this.Evaluate(pins);
        foreach (var (p, v, d) in ports)
        {
            yield return (new ValueEvent(this, p, v), d);
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

    internal string GetNodeTypeID()
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

    protected unsafe void SubmitPropValue(Editor editor, string displayName, NodeDescriptionPropertyAttribute attrib, PropertyInfo prop, object value, out object newValue)
    {
        var id = this.ID.ToString();
        newValue = value;

        ImGui.PushItemWidth(200);
        if (value is int i)
        {
            if (ImGui.InputInt(displayName, ref i))
            {
                i = Math.Clamp(i, attrib.IntMinValue, attrib.IntMaxValue);
                newValue = i;
            }
        }
        else if (value is string str)
        {
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
                if (!checkRegex(str + addedChar))
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
                    if (ImGui.InputTextMultiline(displayName, ref str, attrib.StringMaxLength, new Vector2(300, 150), attrib.StringFlags, callback))
                    {
                        newValue = str;
                    }
                }
                else
                {
                    if (ImGui.InputText(displayName, ref str, attrib.StringMaxLength, attrib.StringFlags, callback))
                    {
                        newValue = str;
                    }
                }
            }
            else
            {
                if (ImGui.InputTextWithHint(displayName, attrib.StringHint, ref str, attrib.StringMaxLength, attrib.StringFlags, callback))
                {
                    newValue = str;
                }
            }
        }
        else if (value is bool b)
        {
            if (ImGui.Checkbox(displayName, ref b))
            {
                newValue = b;
            }
        }
        else if (value is ColorF color)
        {
            var colorV3 = new Vector3(color.R, color.G, color.B);
            var startCol = colorV3;
            if (ImGui.ColorEdit3(displayName, ref colorV3))
            {
                newValue = new ColorF(colorV3.X, colorV3.Y, colorV3.Z, color.A);
            }
        }
        else if (value is Keys k)
        {
            Keys val = k;
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
        else if (value is Enum e)
        {
            var val = (int)value;
            if (ImGui.Combo(displayName, ref val, e.GetType().GetEnumNames(), e.GetType().GetEnumNames().Length))
            {
                newValue = Enum.ToObject(e.GetType(), val);
            }
        }
        else if (value is Array a)
        {
            a = a.Clone() as Array;
            if (ImGui.CollapsingHeader(displayName, ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.TreePush("TRE" + displayName);

                for (int j = 0; j < a.Length; j++)
                {
                    var startEleVal = a.GetValue(j);
                    this.SubmitPropValue(editor, $"{j}", attrib, prop, startEleVal, out var newEleVal);
                    if (newEleVal != startEleVal)
                    {
                        a.SetValue(newEleVal, j);
                        newValue = a;
                    }
                }

                ImGui.BeginDisabled(a.Length == attrib.ArrayMaxLength);
                if (ImGui.SmallButton("Add"))
                {
                    var newA = Array.CreateInstance(a.GetType().GetElementType(), a.Length + 1);
                    Array.Copy(a, newA, a.Length);

                    if (a.Length != 0)
                    {
                        newA.SetValue(newA.GetValue(newA.Length - 2), newA.Length - 1);
                    }
                    else
                    {
                        newA.SetValue(Activator.CreateInstance(newA.GetType().GetElementType()), newA.Length - 1);
                    }
                    newValue = newA;
                }
                ImGui.EndDisabled();
                ImGui.SameLine();
                ImGui.BeginDisabled(a.Length == attrib.ArrayMinLength);
                if (ImGui.SmallButton("Remove"))
                {
                    var newA = Array.CreateInstance(a.GetType().GetElementType(), a.Length - 1);
                    Array.Copy(a, newA, newA.Length);
                    newValue = newA;
                }
                ImGui.EndDisabled();

                ImGui.TreePop();
            }
        }

        if (attrib.HelpTooltip is not null)
        {
            ImGui.SameLine();
            Utilities.ImGuiHelp(attrib.HelpTooltip);
        }
    }

    public virtual unsafe void SubmitUISelected(Editor editor, int componentIndex)
    {
        var data = Utilities.GetCopyOfInstance(this.GetNodeData()) as INodeDescriptionData;
        var props = data.GetType().GetProperties();

        foreach (var prop in props)
        {
            var attrib = prop.GetCustomAttribute<NodeDescriptionPropertyAttribute>();

            if (attrib is null)
                continue;

            var startValue = prop.GetValue(data);
            var displayName = $"{attrib.DisplayName}##{this.ID}";
            this.SubmitPropValue(editor, displayName, attrib, prop, startValue, out var newValue);
            //this.SubmitProp(editor, data, prop);

            if (newValue != startValue)
            {
                editor.Execute(new CModifyComponentDataProp(this.ID, prop, newValue), editor);
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