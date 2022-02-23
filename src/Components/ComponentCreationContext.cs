using LogiX.Editor;
using System.Diagnostics.CodeAnalysis;

namespace LogiX.Components;

public abstract class ComponentCreationPopup
{
    public abstract bool Create(Editor.Editor editor, [NotNullWhen(true)] out Component? component);
}

public class ComponentCreationContext
{
    public Func<Component> DefaultComponent { get; set; }
    public ComponentCreationPopup? PopupCreator { get; set; }
    public string Category { get; set; }
    public string Name { get; set; }

    public ComponentCreationContext(string category, string name, Func<Component> defaultComponent, ComponentCreationPopup? popupCreator = null)
    {
        this.Category = category;
        this.Name = name;
        this.DefaultComponent = defaultComponent;
        this.PopupCreator = popupCreator;
    }
}

public class CCPUGate : ComponentCreationPopup
{
    int bits;
    bool multibit;
    IGateLogic logic;

    public CCPUGate(IGateLogic logic)
    {
        this.bits = 2;
        this.multibit = false;
        this.logic = logic;
    }

    public override bool Create(Editor.Editor editor, [NotNullWhen(true)] out Component? component)
    {
        ImGui.SetNextItemWidth(80);
        if (ImGui.InputInt("Bits", ref this.bits, 1, 1))
        {
            this.bits = Math.Max(2, this.bits);
        }

        ImGui.Button("Create");
        if (ImGui.IsItemClicked())
        {
            ImGui.CloseCurrentPopup();
            component = new LogicGate(editor.GetWorldMousePos(), this.bits, this.logic);
            return true;
        }

        component = null;
        return false;
    }
}