using System.Numerics;
using ImGuiNET;
using LogiX.Architecture;
using LogiX.Architecture.Plugins;
using LogiX.Architecture.Serialization;
using LogiX.Content;

namespace LogiX.Graphics.UI;

public abstract class EditorWindow
{
    public abstract void SubmitUI(Editor editor);
}

public class DynamicEditorWindow : EditorWindow
{
    private Action<Editor, EditorWindow> _submit { get; }
    public DynamicEditorWindow(Action<Editor, EditorWindow> submitUI)
    {
        _submit = submitUI;
    }

    public override void SubmitUI(Editor editor)
    {
        _submit(editor, this);
    }
}

public class DocsTree
{
    public string Name { get; set; }
    public string Asset { get; set; }

    public List<DocsTree> Children { get; set; } = new List<DocsTree>();

    public DocsTree(string name, string asset)
    {
        this.Name = name;
        this.Asset = asset;
    }

    public void SubmitUI(string currentlySelectedAsset, string filter, Action<string> onSelected)
    {
        var treenodeFlags = ImGuiTreeNodeFlags.None;

        if (!string.IsNullOrWhiteSpace(filter))
        {
            treenodeFlags |= ImGuiTreeNodeFlags.DefaultOpen;
        }

        if (this.Children.Count == 0)
        {
            // Leaf node, can be selected
            if (ImGui.Selectable(this.Name, this.Asset == currentlySelectedAsset, ImGuiSelectableFlags.None))
            {
                onSelected(this.Asset);
            }
        }
        else
        {
            if (ImGui.TreeNodeEx(this.Name, treenodeFlags))
            {
                foreach (var child in this.Children)
                {
                    child.SubmitUI(currentlySelectedAsset, filter, onSelected);
                }

                ImGui.TreePop();
            }
        }
    }

    public static DocsTree Create(string name, string asset, params DocsTree[] children)
    {
        var tree = new DocsTree(name, asset);
        tree.Children.AddRange(children);
        return tree;
    }
}

public class DocumentationWindow : EditorWindow
{
    private List<string> _history = new List<string>();
    private int _historyIndex = -1;

    public DocumentationWindow()
    {
        this.GoTo("logix_core:docs/about.md");
    }

    public DocumentationWindow(string path)
    {
        this.GoTo(path);
    }

    private string _search = "";
    public override void SubmitUI(Editor editor)
    {
        bool open = true;
        ImGui.SetNextWindowSizeConstraints(new Vector2(300, 300), new Vector2(float.MaxValue, float.MaxValue));
        if (ImGui.Begin("Documentation", ref open, ImGuiWindowFlags.NoDocking))
        {
            ImGui.BeginGroup();
            ImGui.SetNextItemWidth(300);
            ImGui.BeginDisabled();
            ImGui.InputTextWithHint("##Search", "Search...", ref this._search, 100);
            ImGui.EndDisabled();

            ImGui.EndGroup();

            ImGui.SameLine();

            ImGui.BeginGroup();
            this.SubmitHistoryManager(editor);
            ImGui.EndGroup();

            var backgroundCol = ImGui.GetStyle().Colors[(int)ImGuiCol.ChildBg];
            ImGui.GetBackgroundDrawList().AddRectFilled(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), ImGui.GetColorU32(backgroundCol));

            float height = ImGui.GetContentRegionAvail().Y;
            ImGui.BeginChild("Sidebar", new Vector2(300, height), true, ImGuiWindowFlags.AlwaysVerticalScrollbar);
            this.SubmitSideBarContent(editor);
            ImGui.EndChild();

            ImGui.SameLine();

            float widthLeft = ImGui.GetContentRegionAvail().X;
            ImGui.BeginChild("Content", new Vector2(widthLeft, height), true, ImGuiWindowFlags.AlwaysVerticalScrollbar);
            this.SubmitContent(editor);
            ImGui.EndChild();

            ImGui.End();
        }

        if (!open)
        {
            editor.CloseEditorWindow(this);
        }
    }

    public void SubmitContent(Editor editor)
    {
        if (this._history.Count > 0)
        {
            var path = this.GetCurrent();

            if (Utilities.ContentManager.GetContentItem<MarkdownFile>(path) == null)
            {
                ImGui.Text("No documentation found for this component.");
                return;
            }

            var content = Utilities.ContentManager.GetContentItem<MarkdownFile>(path).Text;
            Utilities.RenderMarkdown(content, (link) =>
            {
                this.GotoLink(link);
            });
        }
    }

    private void GotoLink(string link)
    {
        if (link.StartsWith("http"))
        {
            Utilities.OpenURL(link);
        }
        else if (link.StartsWith("component://"))
        {
            var component = NodeDescription.GetNodeInfo(link.Substring("component://".Length));
            this.GoTo(component.DocumentationAsset);
        }
        else if (link.StartsWith("docs://"))
        {
            this.GoTo(link.Substring("docs://".Length));
        }
    }

    public void SubmitHistoryManager(Editor editor)
    {
        if (ImGui.ArrowButton("Back", ImGuiDir.Left))
        {
            this.Back();
        }

        ImGui.SameLine();

        if (ImGui.ArrowButton("Forward", ImGuiDir.Right))
        {
            this.Forward();
        }

        ImGui.SameLine();
    }

    public void GoTo(string docs)
    {
        // Remove all history after the current index
        while (this._history.Count > this._historyIndex + 1)
        {
            this._history.RemoveAt(this._historyIndex + 1);
        }

        // Add the new history
        this._history.Add(docs);

        // Increment the index
        this._historyIndex++;
    }

    private void Back()
    {
        if (this._historyIndex > 0)
        {
            this._historyIndex--;
        }
    }

    private void Forward()
    {
        if (this._historyIndex < this._history.Count - 1)
        {
            this._historyIndex++;
        }
    }

    private string GetCurrent()
    {
        return this._history[this._historyIndex];
    }

    public void SubmitSideBarContent(Editor editor)
    {
        var helpTree = DocsTree.Create("Help", null,
            DocsTree.Create("About", "logix_core:docs/about.md"),
            DocsTree.Create("Getting Started", "logix_core:docs/getting_started.md")
        );

        helpTree.SubmitUI(this.GetCurrent(), this._search, (selected) =>
        {
            this.GoTo(selected);
        });

        var componentCategories = NodeDescription.GetAllNodeCategories();
        var cats = componentCategories.Select(category =>
        {
            return DocsTree.Create(category.Key, null,
                category.Value.Select(component =>
                {
                    var cInfo = NodeDescription.GetNodeInfo(component);
                    return DocsTree.Create(cInfo.DisplayName, cInfo.DocumentationAsset);
                }).ToArray()
            );
        });

        var componentsTree = DocsTree.Create("Components", null, cats.ToArray());
        componentsTree.SubmitUI(this.GetCurrent(), this._search, (selected) =>
        {
            this.GoTo(selected);
        });
    }
}