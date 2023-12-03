using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using DotGLFW;
using ImGuiNET;
using LogiX.Content;
using LogiX.Model.Circuits;
using LogiX.Model.NodeModel;
using LogiX.Scripting;
using LogiX.UserInterface.Actions;
using NLua;
using Symphony;

namespace LogiX.Model.Projects;

[JsonDerivedType(typeof(VirtualFileTree<ICircuitDefinition>), "circuit-tree")]
public interface IVirtualFileTree<TKey, TValue>
{
    string Directory { get; }

    IEnumerable<IVirtualFileTree<TKey, TValue>> GetDirectories();
    IDictionary<TKey, TValue> GetFiles();

    IVirtualFileTree<TKey, TValue> AddDirectory(string directoryName);
    IVirtualFileTree<TKey, TValue> AddDirectory(string directoryName, IVirtualFileTree<TKey, TValue> directoryContents);
    IVirtualFileTree<TKey, TValue> AddFile(TKey fileName, TValue fileContents);

    TValue RecursivelyGetFileContents(TKey fileName);
    void RecursivelySetFileContents(TKey fileName, TValue fileContents);

    void RecursivelyDeleteFile(TKey fileName);
    void RecursivelyDeleteDirectory(string directoryName);
}

public class VirtualFileTree<TValue>(string directory) : IVirtualFileTree<string, TValue>
{
    public string Directory => directory;

    private readonly Dictionary<string, TValue> _files = [];
    public IReadOnlyDictionary<string, TValue> Files { get => _files; init => _files = value.ToDictionary(kvp => kvp.Key, kvp => kvp.Value); }

    private readonly Dictionary<string, IVirtualFileTree<string, TValue>> _directories = [];
    public IReadOnlyDictionary<string, IVirtualFileTree<string, TValue>> Directories { get => _directories; init => _directories = value.ToDictionary(kvp => kvp.Key, kvp => kvp.Value); }

    public IVirtualFileTree<string, TValue> AddDirectory(string directoryName)
    {
        var newDirectory = new VirtualFileTree<TValue>(directoryName);
        _directories.Add(directoryName, newDirectory);
        return newDirectory;
    }

    public IVirtualFileTree<string, TValue> AddDirectory(string directoryName, IVirtualFileTree<string, TValue> directoryContents)
    {
        _directories.Add(directoryName, directoryContents);
        return directoryContents;
    }

    public IVirtualFileTree<string, TValue> AddFile(string fileName, TValue fileContents)
    {
        _files.Add(fileName, fileContents);
        return this;
    }

    public IEnumerable<IVirtualFileTree<string, TValue>> GetDirectories() => _directories.Values;
    public IDictionary<string, TValue> GetFiles() => _files;

    public void RecursivelyDeleteDirectory(string directoryName)
    {
        if (!directoryName.Contains('/'))
        {
            _directories.Remove(directoryName);
            return;
        }

        string firstDirectoryName = directoryName.Split('/')[0];
        string directoryNameWithoutFirstDirectory = string.Join('/', directoryName.Split('/').Skip(1));

        _directories[firstDirectoryName].RecursivelyDeleteDirectory(directoryNameWithoutFirstDirectory);
    }

    public void RecursivelyDeleteFile(string fileName)
    {
        if (!fileName.Contains('/'))
        {
            _files.Remove(fileName);
            return;
        }

        string firstDirectoryName = fileName.Split('/')[0];
        string fileNameWithoutFirstDirectory = string.Join('/', fileName.Split('/').Skip(1));

        _directories[firstDirectoryName].RecursivelyDeleteFile(fileNameWithoutFirstDirectory);
    }

    public TValue RecursivelyGetFileContents(string fileName)
    {
        if (!fileName.Contains('/'))
        {
            return _files[fileName];
        }

        string firstDirectoryName = fileName.Split('/')[0];
        string fileNameWithoutFirstDirectory = string.Join('/', fileName.Split('/').Skip(1));

        return _directories[firstDirectoryName].RecursivelyGetFileContents(fileNameWithoutFirstDirectory);
    }

    public void RecursivelySetFileContents(string fileName, TValue fileContents)
    {
        if (!fileName.Contains('/'))
        {
            _files[fileName] = fileContents;
            return;
        }

        string firstDirectoryName = fileName.Split('/')[0];
        string fileNameWithoutFirstDirectory = string.Join('/', fileName.Split('/').Skip(1));

        _directories[firstDirectoryName].RecursivelySetFileContents(fileNameWithoutFirstDirectory, fileContents);
    }

    public static IVirtualFileTree<string, TValue> Merge(IVirtualFileTree<string, TValue> tree1, IVirtualFileTree<string, TValue> tree2)
    {
        // Perform a recursive merge of the two trees
        // If a directory exists in both trees, merge the two directories
        // If a file exists in both trees, use the file from tree1

        var mergedTree = new VirtualFileTree<TValue>(tree1.Directory);

        foreach (var directory in tree1.GetDirectories())
        {
            if (tree2.GetDirectories().Any(d => d.Directory == directory.Directory))
            {
                mergedTree.AddDirectory(directory.Directory, Merge(directory, tree2.GetDirectories().First(d => d.Directory == directory.Directory)));
            }
            else
            {
                mergedTree.AddDirectory(directory.Directory, directory);
            }
        }

        foreach (var file in tree1.GetFiles())
        {
            mergedTree.AddFile(file.Key, file.Value);
        }

        foreach (var file in tree2.GetFiles())
        {
            if (!mergedTree.GetFiles().ContainsKey(file.Key))
            {
                mergedTree.AddFile(file.Key, file.Value);
            }
        }

        return mergedTree;
    }
}

public record ProjectMetadata
{
    [LuaMember(Name = "name")]
    public string Name { get; init; }
}

public interface IProject
{
    [LuaMember(Name = "get_project_metadata")]
    ProjectMetadata GetProjectMetadata();

    IVirtualFileTree<string, ICircuitDefinition> GetProjectCircuitTree();
    IVirtualFileTree<string, INode> GetAvailableNodesTree();
    void SetCircuit(string circuitName, ICircuitDefinition circuitDefinition);

    IVirtualFileTree<string, IEditorAction> GetAvailableEditorActionsTree();
    void SubmitMainMenubarMenu();
}

public class Project(
    ProjectMetadata metadata,
    IContentManager<ContentMeta> contentManager,
    IProjectService projectService,
    ILuaService luaService,
    IVirtualFileTree<string, ICircuitDefinition> initialCircuitFileTree) : IProject
{
    private readonly IVirtualFileTree<string, ICircuitDefinition> _circuitFileTree = initialCircuitFileTree;

    [LuaMember(Name = "get_project_metadata")]
    public ProjectMetadata GetProjectMetadata() => metadata;

    public IVirtualFileTree<string, ICircuitDefinition> GetProjectCircuitTree() => _circuitFileTree;

    public IVirtualFileTree<string, INode> GetAvailableNodesTree()
    {
        var root = new VirtualFileTree<INode>("root");

        root.AddDirectory("Gates")
                .AddFile(new NorNode().GetNodeName(), new NorNode());

        root.AddDirectory("Common")
                .AddFile(new PinNode().GetNodeName(), new PinNode());

        var luaDir = root.AddDirectory("Lua");

        var nodeEntries = luaService.GetAllDataEntries(entry => entry.DataType == ScriptingDataType.Node);
        var nodeEntriesCasted = nodeEntries.Select(entry => entry.GetEntryAs<DataEntryNode>());

        foreach (var nodeEntry in nodeEntriesCasted)
        {
            luaDir.AddFile(nodeEntry.Name, new LuaNode(nodeEntry.Identifier, luaService));
        }

        return root;
    }

    public void SetCircuit(string circuitName, ICircuitDefinition circuitDefinition) => _circuitFileTree.RecursivelySetFileContents(circuitName, circuitDefinition);

    public IVirtualFileTree<string, IEditorAction> GetAvailableEditorActionsTree()
    {
        var root = new VirtualFileTree<IEditorAction>("root");

        root.AddDirectory("File")
                .AddFile("New", EditorAction.Empty)
                .AddFile("Open", new EditorAction(
                    canExecute: (inv, vm) => true,
                    isSelected: (inv, vm) => false,
                    execute: (inv, vm) => projectService.LoadProjectFromDisk("test.json"),
                    Keys.LeftControl, Keys.O
                ))
                .AddFile("Save", new EditorAction(
                    canExecute: (inv, vm) => true,
                    isSelected: (inv, vm) => false,
                    execute: (inv, vm) => projectService.SaveProjectToDisk(this, "test.json"),
                    Keys.LeftControl, Keys.S
                ))
                .AddFile("Save As", EditorAction.Empty);

        root.AddDirectory("Edit")
                .AddFile("Undo", new EditorAction(
                    canExecute: (inv, vm) => inv.CanUndo(),
                    isSelected: (inv, vm) => false,
                    execute: (inv, vm) => inv.Undo(),
                    Keys.LeftControl, Keys.Z))

                .AddFile("Redo", new EditorAction(
                    canExecute: (inv, vm) => inv.CanRedo(),
                    isSelected: (inv, vm) => false,
                    execute: (inv, vm) => inv.Redo(),
                    Keys.LeftControl, Keys.Y));

        var root2 = new VirtualFileTree<IEditorAction>("root");

        root2.AddDirectory("File")
                .AddFile("Yes", EditorAction.Empty);

        return VirtualFileTree<IEditorAction>.Merge(root, root2);
    }

    public void SubmitMainMenubarMenu()
    {
        var contentItems = contentManager.GetContentItems().ToArray();
        var contentSourceMetas = contentManager.GetLoadedSourcesMetadata();

        foreach (var contentSourceMeta in contentSourceMetas)
        {
            if (ImGui.BeginMenu($"{contentSourceMeta.Metadata.Identifier} (v{contentSourceMeta.Metadata.Version})"))
            {
                var itemsFromSource = contentItems.Where(c => c.SourceFirstLoadedIn == contentSourceMeta.Source).ToArray();
                var groupedItems = GroupContentItemsByDirectory(itemsFromSource);
                ImGui.BeginDisabled();
                ImGui.Text("Introduced by this source:");
                ImGui.EndDisabled();

                if (itemsFromSource.Length == 0)
                {
                    ImGui.TextDisabled("(None)");
                }
                else
                {
                    foreach (var (key, items) in groupedItems)
                    {
                        if (ImGui.BeginMenu(key))
                        {
                            foreach (var contentItem in items)
                            {
                                ImGui.MenuItem(contentItem.Identifier);
                            }

                            ImGui.EndMenu();
                        }
                    }
                }

                ImGui.Separator();

                var itemsOverwrittenByThisSource = contentItems.Where(c => c.FinalSource == contentSourceMeta.Source && c.SourceFirstLoadedIn != c.FinalSource).ToArray();
                var groupedOverwrittenItems = GroupContentItemsByDirectory(itemsOverwrittenByThisSource);
                ImGui.BeginDisabled();
                ImGui.Text("Overwrites these items:");
                ImGui.EndDisabled();

                if (itemsOverwrittenByThisSource.Length == 0)
                {
                    ImGui.TextDisabled("(None)");
                }
                else
                {
                    foreach (var (key, items) in groupedOverwrittenItems)
                    {
                        if (ImGui.BeginMenu(key))
                        {
                            foreach (var contentItem in items)
                            {
                                ImGui.MenuItem(contentItem.Identifier);
                            }

                            ImGui.EndMenu();
                        }
                    }
                }
                ImGui.EndMenu();
            }
        }
    }

    private IDictionary<string, List<ContentItem>> GroupContentItemsByDirectory(IEnumerable<ContentItem> contentItems)
    {
        // Identifier are always "namespace:path/to/file.extension"
        // namespace will be same for all content items, so we can just group by the directory path

        Dictionary<string, List<ContentItem>> itemsByDirectory = [];

        foreach (var contentItem in contentItems)
        {
            string directory = string.Join('/', contentItem.Identifier.Split(':')[1].Split('/').SkipLast(1));
            if (!itemsByDirectory.TryGetValue(directory, out var value))
            {
                value = [];
                itemsByDirectory.Add(directory, value);
            }

            value.Add(contentItem);
        }

        return itemsByDirectory;
    }
}
