namespace GoodGame.Content.Scripting;

[AttributeUsage(AttributeTargets.Class)]
public class ScriptTypeAttribute : Attribute
{
    public string Identifier { get; private set; }

    public ScriptTypeAttribute(string identifier)
    {
        Identifier = identifier;
    }
}

public class ScriptType
{
    public string Identifier { get; set; }
    public Type Type { get; set; }

    public ScriptType(string identifier, Type type)
    {
        Identifier = identifier;
        Type = type;
    }

    public T CreateInstance<T>()
    {
        return (T)Activator.CreateInstance(Type);
    }

    public T CreateInstance<T>(params object[] args)
    {
        return (T)Activator.CreateInstance(Type, args);
    }
}