using System.Reflection;

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

    public void SubmitForProperty(PropertyInfo prop, Component comp)
    {
        Type propType = prop.PropertyType;

        if (this.Editable)
        {
            if (propType == typeof(int))
            {
                this.SubmitInt(prop, comp);
            }
            else if (propType.IsEnum)
            {
                this.SubmitEnum(prop, comp);
            }
            else if (propType == typeof(string))
            {
                this.SubmitString(prop, comp);
            }
        }
        else
        {
            ImGui.TextDisabled(prop.GetValue(comp).ToString() + ": " + this.Name);
        }
    }

    public void SubmitInt(PropertyInfo prop, Component comp)
    {
        int v = prop.GetValue(comp) as int? ?? 0;
        int oldVal = v;
        ImGui.SetNextItemWidth(this.ItemWidth);
        ImGui.InputInt(this.Name, ref v);

        v = Math.Max(v, this.IntMin);
        v = Math.Min(v, this.IntMax);

        if (oldVal != v)
            prop.SetValue(comp, v);
    }

    public void SubmitEnum(PropertyInfo prop, Component comp)
    {
        var values = Enum.GetValues(prop.PropertyType);

        ImGui.SetNextItemWidth(this.ItemWidth);
        if (ImGui.BeginCombo(this.Name, prop.GetValue(comp).ToString()))
        {
            foreach (var val in values)
            {
                if (ImGui.Selectable(val.ToString()))
                    prop.SetValue(comp, val);
            }
            ImGui.EndCombo();
        }
    }

    public void SubmitString(PropertyInfo prop, Component comp)
    {
        string s = prop.GetValue(comp) as string ?? "";
        ImGui.SetNextItemWidth(this.ItemWidth);
        ImGui.InputText(this.Name, ref s, 16);
        prop.SetValue(comp, s);
    }
}