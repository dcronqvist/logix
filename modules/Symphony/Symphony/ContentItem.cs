using System;

namespace Symphony;

public interface IContent
{
    public void OnContentUpdated(object newContent);
    public void Unload();
}

public abstract class Content<T> : IContent
{
    protected abstract void OnContentUpdated(T newContent);

    public void OnContentUpdated(object newContent)
    {
        OnContentUpdated((T)newContent);
    }

    public abstract void Unload();
}

public class ContentItem
{
    public string Identifier { get; }
    public IContentSource SourceFirstLoadedIn { get; }
    public IContentSource FinalSource { get; }
    public IContent Content { get; }

    public event EventHandler? ContentUpdated;

    public ContentItem(string identifier, IContentSource sourceFirstLoadedIn, IContentSource finalSource, IContent content)
    {
        Identifier = identifier;
        SourceFirstLoadedIn = sourceFirstLoadedIn;
        FinalSource = finalSource;
        Content = content;
    }

    internal void UpdateContent(object newContent)
    {
        Content.OnContentUpdated(newContent);
        this.ContentUpdated?.Invoke(this, EventArgs.Empty);
    }

    public override string ToString()
    {
        return Identifier;
    }
}
