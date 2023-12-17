using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using LogiX.Addons;
using LogiX.Content;
using LogiX.Model.Circuits;
using LogiX.Model.NodeModel;
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
    IAddonService addonService
) : IProjectService
{
    private IProject _currentProject;

    public IProject CreateNewProject(ProjectMetadata projectMetadata)
    {
        var circuitTree = new VirtualFileTree<ICircuitDefinition>("root");

        circuitTree.AddFile("main", new CircuitDefinition());

        circuitTree.AddDirectory("latches")
            .AddFile("sr-latch", new CircuitDefinition());

        return new Project(projectMetadata, contentManager, this, addonService, circuitTree);
    }

    public void SetProject(IProject project) => _currentProject = project;

    public void ClearProject() => _currentProject = null;

    public bool HasProject() => _currentProject != null;
    public IProject GetCurrentProject() => _currentProject;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = {
            new JsonStringEnumConverter(),
            new NodeConverter(addonService),
            new NodeStateConverter(addonService),
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
            addonService,
            projectJsonRepresentation.CircuitDefinitions);
        return project;
    }
}

public class NodeStateConverter(IAddonService addonService) : JsonConverter<INodeState>
{
    private readonly string _nodeTypePropertyName = "$node-type";

    private static IReadOnlyDictionary<string, Type> GetNodeTypeIdentifiersAndStateTypeFromAddon(IAddon addon)
    {
        var nodeTree = addon.GetAddonNodeTree();
        var dict = new Dictionary<string, Type>();
        VirtualFileTree<INode>.Traverse(nodeTree, (path, node) => dict.Add(path, node.CreateInitialState().GetType()));
        return dict;
    }

    public override INodeState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var addons = addonService.GetAddons();
        var nodeTypesIdentifiersAndStateTypes = addons.SelectMany(GetNodeTypeIdentifiersAndStateTypeFromAddon).ToDictionary(x => x.Key, x => x.Value);

        var document = JsonDocument.ParseValue(ref reader);
        document.RootElement.TryGetProperty(_nodeTypePropertyName, out var nodeType);
        string nodeTypeString = nodeType.GetString();

        if (nodeTypesIdentifiersAndStateTypes.TryGetValue(nodeTypeString, out var value))
        {
            object nodeState = JsonSerializer.Deserialize(document.RootElement.GetProperty("state"), value, options);
            return (INodeState)nodeState;
        }
        else
        {
            throw new ArgumentException($"Unknown node type: {nodeTypeString}");
        }
    }

    public override void Write(Utf8JsonWriter writer, INodeState value, JsonSerializerOptions options)
    {
        var addons = addonService.GetAddons();
        var nodeTypesIdentifiersAndStateTypes = addons.SelectMany(GetNodeTypeIdentifiersAndStateTypeFromAddon).ToDictionary(x => x.Key, x => x.Value);

#pragma warning disable CA1869 // Cache and reuse 'JsonSerializerOptions' instances
        var newOptionsWithoutConverter = new JsonSerializerOptions(options);
#pragma warning restore CA1869 // Cache and reuse 'JsonSerializerOptions' instances
        newOptionsWithoutConverter.Converters.Remove(this);

        writer.WriteStartObject();
        writer.WriteString(_nodeTypePropertyName, nodeTypesIdentifiersAndStateTypes.First(x => x.Value == value.GetType()).Key);
        writer.WritePropertyName("state");
        JsonSerializer.Serialize(writer, value, value.GetType(), newOptionsWithoutConverter);
        writer.WriteEndObject();
    }
}

public class NodeConverter(IAddonService addonService) : JsonConverter<INode>
{
    private readonly string _nodeTypePropertyName = "$node-type";

    private static IReadOnlyDictionary<string, Type> GetNodeTypeIdentifiersAndTypeFromAddon(IAddon addon)
    {
        var nodeTree = addon.GetAddonNodeTree();
        var dict = new Dictionary<string, Type>();
        VirtualFileTree<INode>.Traverse(nodeTree, (path, node) => dict.Add(path, node.GetType()));
        return dict;
    }

    public override INode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var addons = addonService.GetAddons();
        var nodeTypesIdentifiersAndTypes = addons.SelectMany(GetNodeTypeIdentifiersAndTypeFromAddon).ToDictionary(x => x.Key, x => x.Value);

        var document = JsonDocument.ParseValue(ref reader);
        document.RootElement.TryGetProperty(_nodeTypePropertyName, out var nodeType);
        string nodeTypeString = nodeType.GetString();

        if (nodeTypesIdentifiersAndTypes.TryGetValue(nodeTypeString, out var value))
        {
            var node = (INode)Activator.CreateInstance(value);
            return node;
        }
        else
        {
            throw new ArgumentException($"Unknown node type: {nodeTypeString}");
        }
    }

    public override void Write(Utf8JsonWriter writer, INode value, JsonSerializerOptions options)
    {
        var addons = addonService.GetAddons();
        var nodeTypesIdentifiersAndTypes = addons.SelectMany(GetNodeTypeIdentifiersAndTypeFromAddon).ToDictionary(x => x.Key, x => x.Value);

        writer.WriteStartObject();
        writer.WriteString(_nodeTypePropertyName, nodeTypesIdentifiersAndTypes.First(x => x.Value == value.GetType()).Key);
        writer.WriteEndObject();
    }
}
