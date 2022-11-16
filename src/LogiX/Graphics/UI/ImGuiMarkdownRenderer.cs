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
    public ImGuiMarkdownRenderer()
    {
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
        string[] words = s.Split(' ').Where(x => x != "").ToArray();
        string currentLine = "";

        while (words.Length > 0)
        {
            string word = words[0];
            avail = ImGui.GetContentRegionAvail().X;
            if (ImGui.CalcTextSize(currentLine + word).X > avail)
            {
                if (currentLine != "")
                    ImGui.Text(currentLine);
                currentLine = "";
            }
            currentLine += word + " ";
            words = words.Skip(1).ToArray();
        }

        currentLine.Trim();

        if (currentLine != "")
        {
            ImGui.Text(currentLine);
        }
    }
}

public class HeadingRenderer : MarkdownObjectRenderer<ImGuiMarkdownRenderer, HeadingBlock>
{
    protected override void Write(ImGuiMarkdownRenderer renderer, HeadingBlock obj)
    {
        var size = obj.Level switch
        {
            1 => 32,
            2 => 28,
            3 => 24,
            4 => 20,
            5 => 16,
            6 => 16,
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
        var color = ColorF.LightSkyBlue;
        ImGui.PushStyleColor(ImGuiCol.Text, color.ToVector4());
        renderer.Render(obj.FirstChild);
        if (ImGui.IsItemClicked())
        {
            Utilities.OpenURL(obj.Url);
        }
        ImGui.PopStyleColor();
        if (ImGui.IsItemHovered())
        {
            Utilities.MouseToolTip(obj.Url);
        }
        Underline(color.ToVector4());
        ImGui.SameLine(0, 0);
    }

    public void DoImage(ImGuiMarkdownRenderer renderer, LinkInline obj)
    {
        if (LogiX.ContentManager.GetContentItem(obj.Url) is null)
        {
            ImGui.Text("Image not found");
        }
        else
        {
            var tex = LogiX.ContentManager.GetContentItem<Texture2D>(obj.Url);
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