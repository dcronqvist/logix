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
            for (int i = 0; i < this.Keys.Length - 1; i++)
            {
                if (!Input.IsKeyDown(this.Keys[i]))
                {
                    return;
                }
            }

            if (this.Condition(editor) && Input.IsKeyPressed(this.Keys.Last()))
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
}