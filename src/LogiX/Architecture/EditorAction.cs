using ImGuiNET;
using LogiX.GLFW;

namespace LogiX.Architecture;

public class EditorAction
{
    public Func<Editor, bool> Condition { get; set; }
    public Func<Editor, bool> Selected { get; set; }

    public Action<Editor> Execute { get; set; }
    public ModifierKeys Modifiers { get; set; }
    public Keys Key { get; set; }

    public EditorAction(Func<Editor, bool> condition, Func<Editor, bool> selected, Action<Editor> execute, ModifierKeys mods, Keys key)
    {
        Condition = condition;
        this.Selected = selected;
        Execute = execute;
        Modifiers = mods;
        Key = key;
    }

    // public void Update(Editor editor)
    // {
    //     if (this.Keys.Length > 0)
    //     {
    //         if (Input.IsKeyComboPressed(this.Keys) && !ImGui.GetIO().WantCaptureKeyboard && !ImGui.GetIO().WantCaptureMouse)
    //         {
    //             if (this.Condition(editor))
    //             {
    //                 try
    //                 {
    //                     this.Execute(editor);
    //                 }
    //                 catch (Exception e)
    //                 {
    //                     editor.OpenErrorPopup("Error", true, () => { ImGui.Text(e.Message); ImGui.Button("OK"); });
    //                 }
    //             }
    //         }
    //     }
    // }

    public ModifierKeys[] GetAllModifiers()
    {
        var mods = new List<ModifierKeys>();
        if (this.Modifiers.HasFlag(ModifierKeys.Alt))
        {
            mods.Add(ModifierKeys.Alt);
        }
        if (this.Modifiers.HasFlag(ModifierKeys.Control))
        {
            mods.Add(ModifierKeys.Control);
        }
        if (this.Modifiers.HasFlag(ModifierKeys.Shift))
        {
            mods.Add(ModifierKeys.Shift);
        }
        if (this.Modifiers.HasFlag(ModifierKeys.Super))
        {
            mods.Add(ModifierKeys.Super);
        }
        return mods.ToArray();
    }

    public string GetShortcutString()
    {
        string s = "";
        var modifiers = this.GetAllModifiers();

        for (int i = 0; i < modifiers.Length; i++)
        {
            s += modifiers[i].ToString();
            if (i < modifiers.Length - 1)
            {
                s += "+";
            }
        }

        s += this.Key.PrettifyKey();
        return s;
    }

    public virtual void SubmitGUI(Editor editor, string actionName)
    {
        if (ImGui.MenuItem(actionName, GetShortcutString(), Selected.Invoke(editor), Condition.Invoke(editor)))
        {
            Execute.Invoke(editor);
        }
    }
}

public class NestedEditorAction : EditorAction
{
    public (string, EditorAction)[] Actions { get; set; }

    public NestedEditorAction(params (string, EditorAction)[] actions) : base((e) => true, (e) => false, (e) => { }, 0, Keys.Unknown)
    {
        this.Actions = actions;
    }

    public override void SubmitGUI(Editor editor, string actionName)
    {
        if (ImGui.BeginMenu(actionName))
        {
            foreach (var (nestName, action) in this.Actions)
            {
                action.SubmitGUI(editor, nestName);
            }
            ImGui.EndMenu();
        }
    }
}