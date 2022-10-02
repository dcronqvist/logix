using System.Reflection;
using LogiX.Content.Scripting;
using System.Text.Json;

namespace LogiX.Architecture.Serialization;

public interface IComponentDescriptionData
{
    public static abstract IComponentDescriptionData GetDefault();
    public static abstract IOMapping GetDefaultMapping(IComponentDescriptionData data);
}

public class ComponentDescription
{
    public string ComponentTypeID { get; set; }
    public IComponentDescriptionData Data { get; set; }
    public IOMapping Mapping { get; set; }
    public Vector2i Position { get; set; }

    public ComponentDescription(string componentTypeID, Vector2i position, IComponentDescriptionData data, IOMapping mapping)
    {
        this.ComponentTypeID = componentTypeID;
        this.Data = data;
        this.Mapping = mapping;
        this.Position = position;
    }

    private static Dictionary<string, ScriptType> _componentTypes;
    public static void RegisterComponentTypes()
    {
        if (_componentTypes is null)
        {
            _componentTypes = new();
            var types = ScriptManager.GetScriptTypes();
            foreach (var t in types)
            {
                if (t.Type.IsAssignableTo(typeof(Component)))
                {
                    _componentTypes.Add(t.Identifier, t);
                }
            }
        }
    }

    public static string GetComponentTypeID(Type type)
    {
        RegisterComponentTypes();
        foreach (var t in _componentTypes)
        {
            if (t.Value.Type == type)
            {
                return t.Key;
            }
        }
        return null;
    }

    public Component CreateComponent()
    {
        var type = _componentTypes[this.ComponentTypeID];
        var component = type.CreateInstance<Component>(Mapping);
        component.Initialize(this.Data);
        component.Position = this.Position;
        return component;
    }

    public static Component CreateDefaultComponent(string identifier)
    {
        var type = _componentTypes[identifier];

        if (Utilities.IsSubclassOfRawGeneric(typeof(Component<>), type.Type))
        {
            // Has data parameter
            var x = type.Type.BaseType.GetGenericArguments();
            var instance = x.First().GetMethod("GetDefault").Invoke(null, null);
            var component = type.CreateInstance<Component>(x.First().GetMethod("GetDefaultMapping").Invoke(null, Utilities.Arrayify(instance)));
            component.Initialize((IComponentDescriptionData)instance);
            return component;
        }
        else
        {
            throw new Exception("Component does not inherit from Component<TData>");
        }
    }

    public static ComponentDescription CreateDefaultComponentDescription(string identifier)
    {
        return CreateDefaultComponent(identifier).GetDescriptionOfInstance();
    }

    public static IComponentDescriptionData CreateDefaultComponentDescriptionData(string identifier)
    {
        var type = _componentTypes[identifier];

        if (Utilities.IsSubclassOfRawGeneric(typeof(Component<>), type.Type))
        {
            // Has data parameter
            var x = type.Type.BaseType.GetGenericArguments();
            var instance = x.First().GetMethod("GetDefault").Invoke(null, null);
            return (IComponentDescriptionData)instance;
        }
        else
        {
            throw new Exception("Component does not inherit from Component<TData>");
        }
    }

    public static string[] GetRegisteredComponentTypes()
    {
        return _componentTypes.Keys.ToArray();
    }

    public static ScriptType GetComponentScriptTypeFromIdentifier(string identifier)
    {
        return _componentTypes[identifier];
    }

    public void SaveToFile(string path)
    {
        var options = new JsonSerializerOptions()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new IComponentDescriptionDataConverter() }
        };

        string json = JsonSerializer.Serialize(this, options);

        using (var file = new StreamWriter(path))
        {
            file.Write(json);
        }
    }

    public static ComponentDescription LoadFromFile(string path)
    {
        using (var file = new StreamReader(path))
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new IComponentDescriptionDataConverter() }
            };

            return JsonSerializer.Deserialize<ComponentDescription>(file.ReadToEnd(), options);
        }
    }

    public static Type GetComponentTypesDataParameterType(string identifier)
    {
        var type = _componentTypes[identifier];

        if (Utilities.IsSubclassOfRawGeneric(typeof(Component<>), type.Type))
        {
            // Has data parameter
            var x = type.Type.BaseType.GetGenericArguments();
            return x.First();
        }
        else
        {
            throw new Exception("Component does not inherit from Component<TData>");
        }
    }
}

public class ComponentDescription<TData> : ComponentDescription where TData : IComponentDescriptionData
{
    public new TData Data { get; set; }

    public ComponentDescription(string componentTypeID, Vector2i position, TData data, IOMapping mapping) : base(componentTypeID, position, data, mapping)
    {
        Data = data;
    }
}