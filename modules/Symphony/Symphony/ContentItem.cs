using System;

namespace Symphony;

public abstract class ContentItem
{
    public string Identifier { get; private set; }
    public IContentSource Source { get; private set; }
    public object Content { get; private set; }
    internal DateTime LastModified { get; private set; }

    public event EventHandler? ContentUpdated;

    public ContentItem(string identifier, IContentSource source, object content)
    {
        Identifier = identifier;
        Source = source;
        Content = content;
    }

    internal void UpdateContent(IContentSource source, object content)
    {
        this.OnContentUpdated(content);
        Content = content;
        Source = source;
        this.ContentUpdated?.Invoke(this, EventArgs.Empty);
    }

    internal void SetLastModified(DateTime lastModified)
    {
        LastModified = lastModified;
    }

    /// <summary>
    /// Called when the content of this item is updated.
    /// </summary>
    protected abstract void OnContentUpdated(object newContent);

    /// <summary>
    /// Called when the content of this item is supposed to be unloaded.
    /// </summary>
    public abstract void Unload();
}

// Can be used for content items, however, it is recommended that you inherit from this class instead and provid
public abstract class ContentItem<T> : ContentItem
{
    public ContentItem(string identifier, IContentSource source, T content)
        : base(identifier, source, content!)
    {
    }

    public new T? Content
    {
        get { return (T)base.Content; }
    }

    protected override void OnContentUpdated(object newContent)
    {
        this.OnContentUpdated((T)newContent);
    }

    protected abstract void OnContentUpdated(T newContent);
}