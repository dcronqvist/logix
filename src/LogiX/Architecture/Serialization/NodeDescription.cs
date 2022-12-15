using System.Reflection;
using LogiX.Content.Scripting;
using System.Text.Json;
using ImGuiNET;

namespace LogiX.Architecture.Serialization;

[AttributeUsage(AttributeTargets.Property)]
public class NodeDescriptionPropertyAttribute : Attribute
{
    public string DisplayName { get; set; }
    public string HelpTooltip { get; set; } = null;

    public int IntMinValue { get; set; } = int.MinValue;
    public int IntMaxValue { get; set; } = int.MaxValue;

    public uint StringMaxLength { get; set; } = ushort.MaxValue;
    public string StringHint { get; set; } = null;
    public bool StringMultiline { get; set; } = false;
    public ImGuiInputTextFlags StringFlags { get; set; } = ImGuiInputTextFlags.CallbackCharFilter;
    public string StringRegexFilter { get; set; } = null;

    public int ArrayMinLength { get; set; } = 0;
    public int ArrayMaxLength { get; set; } = int.MaxValue;

    public NodeDescriptionPropertyAttribute(string displayName)
    {
        this.DisplayName = displayName;
    }

    public static void Validate(INodeDescriptionData data)
    {
        // Loop over all properties
        foreach (var p in data.GetType().GetProperties())
        {
            // Check if property has NodeDescriptionPropertyAttribute
            if (p.GetCustomAttribute<NodeDescriptionPropertyAttribute>() is not null)
            {
                // Check if property is valid
                if (!IsPropertyValid(data, p.GetCustomAttribute<NodeDescriptionPropertyAttribute>(), p))
                {
                    // Fix the property by setting it to its default value
                    var defaultValue = data.GetDefault();
                    p.SetValue(data, p.GetValue(defaultValue));
                }
            }
        }
    }

    public static bool IsPropertyValid(INodeDescriptionData data, NodeDescriptionPropertyAttribute attrib, PropertyInfo prop)
    {
        // Get property value
        var value = prop.GetValue(data);

        if (value is null)
        {
            return false;
        }

        // Check if property is valid
        if (value is string str)
        {
            var lengthTest = str.Length <= attrib.StringMaxLength;
            var regexTest = attrib.StringRegexFilter is null || System.Text.RegularExpressions.Regex.IsMatch(str, attrib.StringRegexFilter);

            return lengthTest && regexTest;
        }
        else if (value is int i)
        {
            return i >= attrib.IntMinValue && i <= attrib.IntMaxValue;
        }
        else if (value is Array a)
        {
            return a.Length >= attrib.ArrayMinLength && a.Length <= attrib.ArrayMaxLength;
        }

        return true;
    }
}

public interface INodeDescriptionData
{
    public abstract INodeDescriptionData GetDefault();
}

public class NodeDescription
{
    public string NodeTypeID { get; set; }
    public INodeDescriptionData Data { get; set; }
    public Vector2i Position { get; set; }
    public int Rotation { get; set; }
    public Guid ID { get; set; }

    public NodeDescription(string nodeTypeID, Vector2i position, int rotation, Guid id, INodeDescriptionData data)
    {
        this.NodeTypeID = nodeTypeID;
        this.Data = data;
        this.Position = position;
        this.Rotation = rotation;
        this.ID = id;
    }

    private static Dictionary<string, ScriptType> _nodeTypes;
    public static void RegisterNodeTypes()
    {
        _nodeTypes = new();
        var types = ScriptManager.GetScriptTypes();
        foreach (var t in types)
        {
            if (t.Type.IsAssignableTo(typeof(Node)))
            {
                if (t.Type.GetCustomAttribute<NodeInfoAttribute>() is not null)
                {
                    _nodeTypes.Add(t.Identifier, t);
                }
            }
        }
    }

    public static NodeInfoAttribute GetNodeInfo(string identifier)
    {
        if (_nodeTypes.TryGetValue(identifier, out var type))
        {
            return type.Type.GetCustomAttribute<NodeInfoAttribute>();
        }
        else
        {
            return null;
        }
    }

    public static string GetNodeTypeID(Type type)
    {
        foreach (var t in _nodeTypes)
        {
            if (t.Value.Type == type)
            {
                return t.Key;
            }
        }
        return null;
    }

    public Node CreateNode()
    {
        var type = _nodeTypes[this.NodeTypeID];
        var node = type.CreateInstance<Node>();
        node.Initialize(this.Data);
        node.Position = this.Position;
        node.Rotation = this.Rotation;
        node.ID = this.ID;
        return node;
    }

    public static Node CreateDefaultNode(string identifier)
    {
        var type = _nodeTypes[identifier];

        if (Utilities.IsSubclassOfRawGeneric(typeof(Node<>), type.Type))
        {
            // Has data parameter
            var h = Utilities.RecursivelyCheckBaseclassUntilRawGeneric(typeof(Node<>), type.Type);
            var x = type.Type.BaseType.GetGenericArguments();
            var instanceOfType = Activator.CreateInstance(x.First());
            var instance = x.First().GetMethod("GetDefault").Invoke(instanceOfType, null);
            var component = type.CreateInstance<Node>();
            component.Initialize((INodeDescriptionData)instance);
            return component;
        }
        else
        {
            var component = type.CreateInstance<Node>();
            component.Initialize(null);
            return component;
        }
    }

    public static NodeDescription CreateDefaultNodeDescription(string identifier)
    {
        return CreateDefaultNode(identifier).GetDescriptionOfInstance();
    }

    public static INodeDescriptionData CreateDefaultNodeDescriptionData(string identifier)
    {
        var type = _nodeTypes[identifier];

        if (Utilities.IsSubclassOfRawGeneric(typeof(Node<>), type.Type))
        {
            // Has data parameter
            var x = type.Type.BaseType.GetGenericArguments();
            var instanceOfType = Activator.CreateInstance(x.First());
            var instance = x.First().GetMethod("GetDefault").Invoke(instanceOfType, null);
            return (INodeDescriptionData)instance;
        }
        else
        {
            throw new Exception("Node does not inherit from Node<T>");
        }
    }

    public static string[] GetRegisteredNodeTypes()
    {
        return _nodeTypes.Keys.ToArray();
    }

    public static ScriptType GetNodeScriptTypeFromIdentifier(string identifier)
    {
        return _nodeTypes[identifier];
    }

    public void SaveToFile(string path)
    {
        var options = new JsonSerializerOptions()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new INodeDescriptionDataConverter() }
        };

        string json = JsonSerializer.Serialize(this, options);

        using (var file = new StreamWriter(path))
        {
            file.Write(json);
        }
    }

    public static NodeDescription LoadFromFile(string path)
    {
        using (var file = new StreamReader(path))
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new INodeDescriptionDataConverter() }
            };

            return JsonSerializer.Deserialize<NodeDescription>(file.ReadToEnd(), options);
        }
    }

    public static Type GetNodeTypesDataParameterType(string identifier)
    {
        var type = _nodeTypes[identifier];

        if (Utilities.IsSubclassOfRawGeneric(typeof(Node<>), type.Type))
        {
            // Has data parameter
            var x = type.Type.BaseType.GetGenericArguments();
            return x.First();
        }
        else
        {
            throw new Exception("Node does not inherit from Node<T>");
        }
    }

    public static SortedDictionary<string, string[]> GetAllNodeCategories()
    {
        var categories = new Dictionary<string, List<string>>();
        foreach (var t in _nodeTypes)
        {
            var info = t.Value.Type.GetCustomAttribute<NodeInfoAttribute>();
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

public class ComponentDescription<TData> : NodeDescription where TData : INodeDescriptionData
{
    public new TData Data { get; set; }

    public ComponentDescription(string componentTypeID, Vector2i position, int rotation, Guid id, TData data) : base(componentTypeID, position, rotation, id, data)
    {
        Data = data;
    }
}