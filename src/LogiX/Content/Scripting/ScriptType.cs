using Symphony;

namespace LogiX.Content.Scripting;

[AttributeUsage(AttributeTargets.Class)]
public class ScriptTypeAttribute : Attribute
{
    public string Identifier { get; private set; }

    public ScriptTypeAttribute(string identifier)
    {
        Identifier = identifier;
    }
}

public class ScriptType : ContentItem<Type>
{
    public ScriptType(IContentSource source, Type type) : base(source, type) { }

    public T CreateInstance<T>()
    {
        return (T)Activator.CreateInstance(this.Content);
    }

    public T CreateInstance<T>(params object[] args)
    {
        return (T)Activator.CreateInstance(this.Content, args);
    }

    protected override void OnContentUpdated(Type newContent)
    {

    }

    public override void Unload()
    {

    }

    public void SetIdentifier(string identifier)
    {
        this.Identifier = identifier;
    }
}