using ImGuiNET;
using LogiX.GLFW;

namespace LogiX.Architecture;

public class EditorAction
{
    public Func<Editor, bool> Condition { get; set; }
    public Func<Editor, bool> Selected { get; set; }

    public Action<Editor> Execute { get; set; }
    public Keys[] Keys { get; set; }

    public EditorAction(Func<Editor, bool> condition, Func<Editor, bool> selected, Action<Editor> execute, params Keys[] keys)
    {
        Condition = condition;
        this.Selected = selected;
        Execute = execute;
        Keys = keys;
    }

    public bool HasKeys()
    {
        return this.Keys.Length > 0;
    }

    public void Update(Editor editor)
    {
        if (this.Keys.Length > 0)
        {
            if (Input.IsKeyComboPressed(this.Keys) && !ImGui.GetIO().WantCaptureKeyboard && !ImGui.GetIO().WantCaptureMouse)
            {
                if (this.Condition(editor))
                {
                    try
                    {
                        this.Execute(editor);
                    }
                    catch (Exception e)
                    {
                        editor.OpenErrorPopup("Error", true, () => { ImGui.Text(e.Message); ImGui.Button("OK"); });
                    }
                }
            }
        }
    }

    public string GetShortcutString()
    {
        string s = "";
        for (int i = 0; i < this.Keys.Length; i++)
        {
            s += this.Keys[i].PrettifyKey();
            if (this.Keys[i] != this.Keys.Last())
            {
                s += "+";
            }
        }
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

    public NestedEditorAction(params (string, EditorAction)[] actions) : base((e) => true, (e) => false, (e) => { }, new Keys[0])
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