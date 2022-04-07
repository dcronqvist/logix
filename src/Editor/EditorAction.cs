using LogiX.Editor.Commands;

namespace LogiX.Editor;

public class EditorAction
{
    public string Category { get; set; }
    public string Name { get; set; }
    public KeyboardKey[] Shortcut { get; set; }
    public Func<Editor, bool> Prerequisite { get; set; }
    public Action<Editor> Action { get; set; }

    public EditorAction(string category, string name, Action<Editor>? action, Func<Editor, bool>? prerequisite, params KeyboardKey[] shortcut)
    {
        Category = category;
        Name = name;
        Shortcut = shortcut;
        Action = action ?? (editor => { });
        Prerequisite = prerequisite ?? (editor => true);
    }

    public bool CanExecute(Editor editor)
    {
        return this.Prerequisite(editor);
    }

    public virtual void Execute(Editor editor)
    {
        this.Action(editor);
    }
}

public class EditorActionCommand : EditorAction
{
    public Command<Editor> Command { get; set; }

    public EditorActionCommand(string category, string name, Command<Editor> command, Func<Editor, bool>? prereq, params KeyboardKey[] shortcut) : base(category, name, null, prereq, shortcut)
    {
        Command = command;
        Prerequisite = prereq ?? ((e) => true);
    }

    public override void Execute(Editor editor)
    {
        editor.Execute(this.Command);
    }
}