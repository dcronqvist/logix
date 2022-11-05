using System.Reflection;
using LogiX.Content.Scripting;
using System.Text.Json;

namespace LogiX.Architecture.Serialization;

public interface IComponentDescriptionData
{
    public static abstract IComponentDescriptionData GetDefault();
}

public class ComponentDescription
{
    public string ComponentTypeID { get; set; }
    public IComponentDescriptionData Data { get; set; }
    public Vector2i Position { get; set; }
    public int Rotation { get; set; }

    public ComponentDescription(string componentTypeID, Vector2i position, int rotation, IComponentDescriptionData data)
    {
        this.ComponentTypeID = componentTypeID;
        this.Data = data;
        this.Position = position;
        this.Rotation = rotation;
    }

    private static Dictionary<string, ScriptType> _componentTypes;
    public static void RegisterComponentTypes()
    {
        _componentTypes = new();
        var types = ScriptManager.GetScriptTypes();
        foreach (var t in types)
        {
            if (t.Type.IsAssignableTo(typeof(Component)))
            {
                if (t.Type.GetCustomAttribute<ComponentInfoAttribute>() is not null)
                {
                    _componentTypes.Add(t.Identifier, t);
                }
            }
        }
    }

    public static ComponentInfoAttribute GetComponentInfo(string identifier)
    {
        if (_componentTypes.TryGetValue(identifier, out var type))
        {
            return type.Type.GetCustomAttribute<ComponentInfoAttribute>();
        }
        else
        {
            return null;
        }
    }

    public static string GetComponentTypeID(Type type)
    {
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
        var component = type.CreateInstance<Component>();
        component.Initialize(this.Data);
        component.Position = this.Position;
        component.Rotation = this.Rotation;
        return component;
    }

    public static Component CreateDefaultComponent(string identifier)
    {
        var type = _componentTypes[identifier];

        if (Utilities.IsSubclassOfRawGeneric(typeof(Component<>), type.Type))
        {
            // Has data parameter
            var h = Utilities.RecursivelyCheckBaseclassUntilRawGeneric(typeof(Component<>), type.Type);

            var x = type.Type.BaseType.GetGenericArguments();
            var instance = x.First().GetMethod("GetDefault").Invoke(null, null);
            var component = type.CreateInstance<Component>();
            component.Initialize((IComponentDescriptionData)instance);
            return component;
        }
        else
        {
            var component = type.CreateInstance<Component>();
            component.Initialize(null);
            return component;
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

    public static SortedDictionary<string, string[]> GetAllComponentCategories()
    {
        var categories = new Dictionary<string, List<string>>();
        foreach (var t in _componentTypes)
        {
            var info = t.Value.Type.GetCustomAttribute<ComponentInfoAttribute>();
            if (info is not null)
            {
                if (info.Hidden)
                    continue;

                if (!categories.ContainsKey(info.Category))
                {
                    categories.Add(info.Category, new List<string>());
                }
                categories[info.Category].Add(t.Key);
            }

            categories[info.Category].Sort();
        }

        var result = new SortedDictionary<string, string[]>();
        foreach (var c in categories)
        {
            result.Add(c.Key, c.Value.ToArray());
        }
        return result;
    }

    public static LogiXProject CurrentProject { get; set; }

    public static Circuit GetIntegratedProjectCircuitByID(Guid id)
    {
        return CurrentProject.GetCircuit(id);
    }
}

public class ComponentDescription<TData> : ComponentDescription where TData : IComponentDescriptionData
{
    public new TData Data { get; set; }

    public ComponentDescription(string componentTypeID, Vector2i position, int rotation, TData data) : base(componentTypeID, position, rotation, data)
    {
        Data = data;
    }
}