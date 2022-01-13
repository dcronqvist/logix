namespace LogiX.Editor;

public class EditorAction
{
    public Func<Editor, bool> Condition { get; set; }
    public Func<Editor, bool> Selected { get; set; }

    public delegate bool Execution(Editor editor, out string error);

    public Execution Execute { get; set; }
    public KeyboardKey[] Keys { get; set; }

    public EditorAction(Func<Editor, bool> condition, Func<Editor, bool> selected, Execution execute, params KeyboardKey[] keys)
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
                if (!Raylib.IsKeyDown(this.Keys[i]))
                {
                    return;
                }
            }

            if (this.Condition(editor) && Raylib.IsKeyPressed(this.Keys.Last()))
            {
                if (!this.Execute(editor, out string error))
                {
                    // An error occured.
                    editor.ModalError(error);
                }
            }
        }
    }

    public string GetShortcutString()
    {
        string s = "";
        for (int i = 0; i < this.Keys.Length; i++)
        {
            s += this.Keys[i].Pretty();
            if (this.Keys[i] != this.Keys.Last())
            {
                s += "+";
            }
        }
        return s;
    }
}