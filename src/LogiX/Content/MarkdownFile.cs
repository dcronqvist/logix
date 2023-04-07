using Symphony;

namespace LogiX.Content;

public class MarkdownFile : ContentItem
{
    public string Text { get => this.Content as string; }

    public MarkdownFile(IContentSource source, object content) : base(source, content)
    {

    }

    public override void Unload()
    {

    }

    protected override void OnContentUpdated(object newContent)
    {

    }
}
