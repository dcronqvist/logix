using System.Numerics;
using ImGuiNET;
using LogiX.Graphics;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace LogiX;

public class ImGuiMarkdownRenderer : RendererBase
{
    public Action<string> OnLinkClicked;
    private string _popupID = "##markdownPopup";

    public ImGuiMarkdownRenderer(Action<string> onLinkClicked)
    {
        this.OnLinkClicked = onLinkClicked;

        this.ObjectRenderers.Add(new HeadingRenderer());
        this.ObjectRenderers.Add(new ParagraphRenderer());
        this.ObjectRenderers.Add(new LinkRenderer());
        this.ObjectRenderers.Add(new LiteralInlineRenderer());
        this.ObjectRenderers.Add(new EmphasisInlineRenderer());
        this.ObjectRenderers.Add(new ListItemRenderer());
        this.ObjectRenderers.Add(new BlankLineRenderer());
        this.ObjectRenderers.Add(new TableRenderer());
    }

    public override object Render(MarkdownObject markdownObject)
    {
        Write(markdownObject);
        return null;
    }

    // Should wrap the words to the available width
    public static void RenderWrappedString(string s)
    {
        float avail = ImGui.GetContentRegionAvail().X;
        // List of all words and if there is a space after it
        List<(string, bool)> words = new();

        string currentWord = "";
        bool space = false;
        foreach (char c in s)
        {
            if (c == ' ')
            {
                space = true;
                words.Add((currentWord, space));
                currentWord = "";
            }
            else
            {
                currentWord += c;
                space = false;
            }
        }

        if (currentWord != "")
            words.Add((currentWord, space));

        string currentLine = "";
        int lines = 0;

        while (words.Count > 0)
        {
            (string word, bool addSpace) = words.First();
            avail = ImGui.GetContentRegionAvail().X;
            if (ImGui.CalcTextSize(currentLine + word).X > avail)
            {
                if (currentLine != "")
                {
                    ImGui.Text(currentLine);
                    lines++;
                }
                currentLine = "";
            }
            currentLine += word;
            if (addSpace)
                currentLine += " ";
            words = words.Skip(1).ToList();
        }

        currentLine.Trim();

        if (currentLine != "")
        {
            ImGui.Text(currentLine);
        }
        //ImGui.TextWrapped(s);
    }
}

public class HeadingRenderer : MarkdownObjectRenderer<ImGuiMarkdownRenderer, HeadingBlock>
{
    protected override void Write(ImGuiMarkdownRenderer renderer, HeadingBlock obj)
    {
        var size = obj.Level switch
        {
            1 => 30,
            2 => 28,
            3 => 26,
            4 => 24,
            5 => 22,
            6 => 20,
            _ => 16
        };

        Utilities.PushFontSize(size);
        Utilities.PushFontBold();

        foreach (var child in obj.Inline)
        {
            renderer.Render(child);
        }
        ImGui.NewLine();

        if (obj.Level < 2)
            ImGui.Separator();

        Utilities.PopFontStyle(2);
    }
}

public class ParagraphRenderer : MarkdownObjectRenderer<ImGuiMarkdownRenderer, ParagraphBlock>
{
    protected override void Write(ImGuiMarkdownRenderer renderer, ParagraphBlock obj)
    {
        foreach (var child in obj.Inline)
        {
            renderer.Write(child);
        }
        ImGui.NewLine();
        ImGui.Spacing();
        ImGui.Spacing();
    }
}

public class LiteralInlineRenderer : MarkdownObjectRenderer<ImGuiMarkdownRenderer, LiteralInline>
{
    protected override void Write(ImGuiMarkdownRenderer renderer, LiteralInline obj)
    {
        ImGuiMarkdownRenderer.RenderWrappedString(obj.ToString());
        ImGui.SameLine(0, 0);
    }
}

public class EmphasisInlineRenderer : MarkdownObjectRenderer<ImGuiMarkdownRenderer, EmphasisInline>
{
    protected override void Write(ImGuiMarkdownRenderer renderer, EmphasisInline obj)
    {
        if (obj.DelimiterCount == 2)
        {
            // BOLD FONT
            Utilities.PushFontBold();
            foreach (var child in obj)
            {
                renderer.Render(child);
            }
            Utilities.PopFontStyle();
        }
        else
        {
            // ITALIC FONT
            Utilities.PushFontItalic();
            foreach (var child in obj)
            {
                renderer.Render(child);
            }
            Utilities.PopFontStyle();
        }
    }
}

public class LinkRenderer : MarkdownObjectRenderer<ImGuiMarkdownRenderer, LinkInline>
{
    public void Underline(Vector4 color)
    {
        Vector2 vmin = ImGui.GetItemRectMin();
        Vector2 vmax = ImGui.GetItemRectMax();
        vmin.Y = vmax.Y;

        if (ImGui.IsItemHovered())
        {
            ImGui.GetWindowDrawList().AddLine(vmin, vmax, ImGui.GetColorU32(color), 1);
        }
    }

    public void DoHyperlink(ImGuiMarkdownRenderer renderer, LinkInline obj)
    {
        var popupID = $"Link Warning##{obj.Url}";
        var color = ColorF.LightSkyBlue;
        ImGui.PushStyleColor(ImGuiCol.Text, color.ToVector4());
        renderer.Render(obj.FirstChild);
        Underline(color.ToVector4());
        if (ImGui.IsItemClicked())
        {
            var showWarning = Settings.GetSetting<bool>(Settings.SHOW_URL_WARNING);

            if (!showWarning)
            {
                renderer.OnLinkClicked?.Invoke(obj.Url);
            }
            else
            {
                ImGui.OpenPopup(popupID);
            }
        }
        ImGui.PopStyleColor();
        ImGui.SameLine(0, 0);

        bool open = true;
        if (ImGui.BeginPopupModal(popupID, ref open, ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text("You are about to open a link to an external website!\nThis may be unsafe. Pressing \"Open link\" will\nopen the link in your default browser.");

            ImGui.Spacing();

            ImGui.Text($"Link: {obj.Url}");

            ImGui.Spacing();

            var showWarning = Settings.GetSetting<bool>(Settings.SHOW_URL_WARNING);
            var disableWarning = !showWarning;
            ImGui.Checkbox("Don't show this warning again", ref disableWarning);
            Settings.SetSetting(Settings.SHOW_URL_WARNING, !disableWarning);

            ImGui.Spacing();

            if (ImGui.Button("Open link"))
            {
                renderer.OnLinkClicked?.Invoke(obj.Url);
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }

        if (!open)
        {
            ImGui.CloseCurrentPopup();
        }
    }

    public void DoImage(ImGuiMarkdownRenderer renderer, LinkInline obj)
    {
        if (LogiXWindow.ContentManager.GetContentItem(obj.Url) is null)
        {
            ImGui.Text("Image not found");
        }
        else
        {
            var tex = LogiXWindow.ContentManager.GetContentItem<Texture2D>(obj.Url);
            ImGui.Image(new IntPtr(tex.GLID), new Vector2(tex.Width, tex.Height));
            ImGui.SameLine(0, 2);
        }
    }

    protected override void Write(ImGuiMarkdownRenderer renderer, LinkInline obj)
    {
        if (!obj.IsImage)
        {
            DoHyperlink(renderer, obj);
        }
        else
        {
            DoImage(renderer, obj);
        }
    }
}

public class ListItemRenderer : MarkdownObjectRenderer<ImGuiMarkdownRenderer, ListItemBlock>
{
    protected override void Write(ImGuiMarkdownRenderer renderer, ListItemBlock obj)
    {
        foreach (var i in obj)
        {
            ImGui.Bullet();
            ImGui.SameLine();
            renderer.Render(i);
        }
    }
}

public class BlankLineRenderer : MarkdownObjectRenderer<ImGuiMarkdownRenderer, BlankLineBlock>
{
    protected override void Write(ImGuiMarkdownRenderer renderer, BlankLineBlock obj)
    {
        ImGui.NewLine();
    }
}

public class TableRenderer : MarkdownObjectRenderer<ImGuiMarkdownRenderer, Markdig.Extensions.Tables.Table>
{
    protected override void Write(ImGuiMarkdownRenderer renderer, Table obj)
    {
        if (ImGui.BeginTable(obj.GetHashCode().ToString(), obj.ColumnDefinitions.Count - 1, ImGuiTableFlags.Borders))
        {
            foreach (var rowObj in obj)
            {
                var row = (TableRow)rowObj;

                foreach (var cell in row)
                {
                    var tableCell = (TableCell)cell;
                    if (row.IsHeader)
                    {
                        ImGui.TableSetupColumn(((ParagraphBlock)tableCell.LastChild).Inline.First().ToString());
                    }
                    else
                    {
                        ImGui.TableNextColumn();
                        renderer.Render(tableCell);
                    }
                }

                if (row.IsHeader)
                {
                    Utilities.PushFontBold();
                    ImGui.TableHeadersRow();
                    Utilities.PopFontStyle();
                }

                if (rowObj != obj.Last())
                    ImGui.TableNextRow();
            }

            ImGui.EndTable();
        }
    }
}