using Symphony;

namespace LogiX.Content;

public class MarkdownFile : ContentItem
{
    public string Text { get => this.Content as string; }

    public MarkdownFile(string identifier, IContentSource source, object content) : base(identifier, source, content)
    {

    }

    public override void Unload()
    {

    }

    protected override void OnContentUpdated(object newContent)
    {

    }
}
