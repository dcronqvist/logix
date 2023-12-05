using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using LogiX.Content;
using LogiX.Model.Circuits;
using LogiX.Model.NodeModel;
using LogiX.Scripting;
using LogiX.UserInterfaceContext;
using Symphony;

namespace LogiX.Model.Projects;

public interface IProjectService
{
    IProject CreateNewProject(ProjectMetadata projectMetadata);
    void SetProject(IProject project);
    void ClearProject();

    bool HasProject();
    IProject GetCurrentProject();

    void SaveProjectToDisk(IProject project, string path);
    IProject LoadProjectFromDisk(string path);
}

public class ProjectService(
    IContentManager<ContentMeta> contentManager,
    IFileSystemProvider fileSystemProvider,
    ILuaService luaService
) : IProjectService
{
    private IProject _currentProject;

    public IProject CreateNewProject(ProjectMetadata projectMetadata)
    {
        var circuitTree = new VirtualFileTree<ICircuitDefinition>("root");

        circuitTree.AddFile("main", new CircuitDefinition());

        circuitTree.AddDirectory("latches")
            .AddFile("sr-latch", new CircuitDefinition());

        return new Project(projectMetadata, contentManager, this, luaService, circuitTree);
    }

    public void SetProject(IProject project) => _currentProject = project;

    public void ClearProject() => _currentProject = null;

    public bool HasProject() => _currentProject != null;
    public IProject GetCurrentProject() => _currentProject;

    private static void ResolveNodeTypes(JsonTypeInfo typeInfo, ILuaService luaService)
    {
        if (typeInfo.Type != typeof(INodeState))
            return;

        var builtinNodes = new Dictionary<string, (Type nodeType, Type stateType)>
        {
            { "pin-node", (typeof(PinNode), typeof(PinState))},
            { "nor-gate", (typeof(NorNode), typeof(EmptyState))}
        };

        var luaNodeEntries = luaService.GetAllDataEntries(entry => entry.DataType == ScriptingDataType.Node);
        var luaNodes = luaNodeEntries.Select(entry => (entry.Identifier, (typeof(LuaNode), typeof(DataEntryNodeState)))).ToDictionary(x => x.Identifier, x => x.Item2);

        var allNodes = builtinNodes.Concat(luaNodes);

        var polymorphismDerivedTypesNodes = allNodes.Select(x => new JsonDerivedType(x.Value.Item1, x.Key));
        var polymorphismDerivedTypesStates = allNodes.Select(x => new JsonDerivedType(x.Value.Item2, x.Key));
        typeInfo.PolymorphismOptions = new JsonPolymorphismOptions()
        {
            TypeDiscriminatorPropertyName = "$node-type",
            IgnoreUnrecognizedTypeDiscriminators = true,
            UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
        };

        polymorphismDerivedTypesStates.ToList().ForEach(typeInfo.PolymorphismOptions.DerivedTypes.Add);
    }

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers = {
                (jti) => ResolveNodeTypes(jti, luaService)
            }
        },
        Converters = {
            new JsonStringEnumConverter(),
            new NodeConverter(luaService)
        }
    };

    private static Stream GetStringStream(string text)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(text);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    private static string GetStringFromStream(Stream stream)
    {
        stream.Position = 0;
        var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public void SaveProjectToDisk(IProject project, string path)
    {
        string jsonText = JsonSerializer.Serialize(new ProjectJsonRepresentation
        {
            Metadata = project.GetProjectMetadata(),
            CircuitDefinitions = project.GetProjectCircuitTree(),
        }, _jsonSerializerOptions);
        var jsonStream = GetStringStream(jsonText);

        fileSystemProvider.WriteFile(path, jsonStream);
    }

    public IProject LoadProjectFromDisk(string path)
    {
        if (!fileSystemProvider.FileExists(path))
            return CreateNewProject(new ProjectMetadata() { Name = "Untitled project" });

        var jsonStream = fileSystemProvider.ReadFile(path);
        string jsonText = GetStringFromStream(jsonStream);

        var projectJsonRepresentation = JsonSerializer.Deserialize<ProjectJsonRepresentation>(jsonText, _jsonSerializerOptions);

        var project = new Project(
            projectJsonRepresentation.Metadata,
            contentManager,
            this,
            luaService,
            projectJsonRepresentation.CircuitDefinitions);
        return project;
    }
}

public class NodeConverter(ILuaService luaService) : JsonConverter<INode>
{
    private Dictionary<string, (Type nodeType, Type stateType)> builtinNodes = new Dictionary<string, (Type nodeType, Type stateType)>
    {
        { "pin-node", (typeof(PinNode), typeof(PinState))},
        { "nor-gate", (typeof(NorNode), typeof(EmptyState))}
    };

    public override INode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var document = JsonDocument.ParseValue(ref reader);
        document.RootElement.TryGetProperty("$node-type", out var nodeType);
        string nodeTypeString = nodeType.GetString();

        if (luaService.GetAllDataEntries().Any(entry => entry.Identifier == nodeTypeString))
        {
            return new LuaNode(nodeTypeString, luaService);
        }
        else
        {
            var (builtinNodeType, _) = builtinNodes[nodeTypeString];
            var node = (INode)Activator.CreateInstance(builtinNodeType);
            return node;
        }
    }

    public override void Write(Utf8JsonWriter writer, INode value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        if (value is LuaNode luaNode)
            writer.WriteString("$node-type", luaNode.LuaDataEntryIdentifier);
        else
            writer.WriteString("$node-type", builtinNodes.First(x => x.Value.nodeType == value.GetType()).Key);
        writer.WriteEndObject();
    }
}
