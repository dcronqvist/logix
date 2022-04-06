using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using LogiX.Editor.Commands;

namespace LogiX.Components;

public class ComponentPropAttribute : Attribute
{
    public string Name { get; set; }
    public bool Editable { get; set; }

    public int IntMin { get; set; }
    public int IntMax { get; set; }

    public int ItemWidth { get; set; }

    public ComponentPropAttribute(string name)
    {
        this.Name = name;
        this.IntMin = int.MinValue;
        this.IntMax = int.MaxValue;
        this.Editable = true;
        this.ItemWidth = 100;
    }

    public bool SubmitForProperty(PropertyInfo prop, Component comp, [NotNullWhen(true)] out CommandComponentPropChanged? cmd)
    {
        Type propType = prop.PropertyType;

        if (this.Editable)
        {
            if (propType == typeof(int))
            {
                return this.SubmitInt(prop, comp, out cmd);
            }
            else if (propType.IsEnum)
            {
                return this.SubmitEnum(prop, comp, out cmd);
            }
            else if (propType == typeof(string))
            {
                return this.SubmitString(prop, comp, out cmd);
            }
        }
        else
        {
            ImGui.TextDisabled(prop.GetValue(comp).ToString() + ": " + this.Name);
        }

        cmd = null;
        return false;
    }

    public bool SubmitInt(PropertyInfo prop, Component comp, [NotNullWhen(true)] out CommandComponentPropChanged? cmd)
    {
        int v = prop.GetValue(comp) as int? ?? 0;
        int oldVal = v;
        ImGui.SetNextItemWidth(this.ItemWidth);
        ImGui.InputInt(this.Name, ref v);

        v = Math.Max(v, this.IntMin);
        v = Math.Min(v, this.IntMax);

        if (oldVal != v)
        {
            cmd = new CommandComponentPropChanged($"{this.Name} of {comp.DisplayText} changed", comp, prop, v);
            return true;
        }

        cmd = null;
        return false;
    }

    public bool SubmitEnum(PropertyInfo prop, Component comp, [NotNullWhen(true)] out CommandComponentPropChanged? cmd)
    {
        var values = Enum.GetValues(prop.PropertyType);

        ImGui.SetNextItemWidth(this.ItemWidth);
        if (ImGui.BeginCombo(this.Name, prop.GetValue(comp).ToString()))
        {
            foreach (var val in values)
            {
                if (ImGui.Selectable(val.ToString()))
                {
                    cmd = new CommandComponentPropChanged($"{this.Name} of {comp.DisplayText} changed", comp, prop, val);
                    return true;
                }
            }
            ImGui.EndCombo();
        }

        cmd = null;
        return false;
    }

    public bool SubmitString(PropertyInfo prop, Component comp, [NotNullWhen(true)] out CommandComponentPropChanged? cmd)
    {
        string s = prop.GetValue(comp) as string ?? "";
        string copy = s;
        ImGui.SetNextItemWidth(this.ItemWidth);
        ImGui.InputText(this.Name, ref s, 16);

        if (s != copy)
        {
            cmd = new CommandComponentPropChanged($"{this.Name} of {comp.DisplayText} changed", comp, prop, s);
            return true;
        }

        cmd = null;
        return false;
    }
}