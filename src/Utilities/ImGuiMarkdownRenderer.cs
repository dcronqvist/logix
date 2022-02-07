using Markdig;
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
        this.ObjectRenderers.Add(new ListItemRenderer());
        this.ObjectRenderers.Add(new BlankLineRenderer());
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
        Util.WithFont("opensans-bold-20", () =>
        {
            foreach (var child in obj.Inline)
            {
                ImGuiMarkdownRenderer.RenderWrappedString(child.ToString());
            }
            ImGui.Separator();
        });
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
        ImGui.PushStyleColor(ImGuiCol.Text, Color.LIME.ToVector4());
        renderer.Render(obj.FirstChild);
        if (ImGui.IsItemClicked())
        {
            Raylib.OpenURL(obj.Url);
        }
        ImGui.PopStyleColor();
        if (ImGui.IsItemHovered())
        {
            Util.Tooltip(obj.Url);
        }
        Underline(Color.LIME.ToVector4());
        ImGui.SameLine(0, 0);
    }

    public void DoImage(ImGuiMarkdownRenderer renderer, LinkInline obj)
    {
        if (!Util.AssetFileExists(obj.Url))
        {
            ImGui.Text("Image not found");
        }
        else
        {
            Texture2D tex = Util.GetAssetTexture(obj.Url);
            ImGui.Image(new IntPtr(tex.id), new Vector2(tex.width, tex.height));
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
