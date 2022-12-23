using System;
using System.CommandLine;
using System.IO;
using ImGuiNET;
using LogiX;
using LogiX.Architecture;
using LogiX.Architecture.BuiltinComponents;
using LogiX.Architecture.Plugins;
using LogiX.Content.Scripting;
using LogiX.Graphics.UI;
using System.Linq;

namespace FlispPlugin;

[ScriptType("template_action_1")]
public class TestPluginAction1 : IPluginAction
{
    public string Name => "Template Action 1";

    public void Execute(Editor editor)
    {
        editor.OpenPopup("Template Action 1, popup!", (e) =>
        {
            ImGui.Text("This action just opens a popup! Super simple!");
        });
    }
}

[ScriptType("template_action_2")]
public class TestPluginAction2 : IPluginAction
{
    public string Name => "Template Action 2";

    public void Execute(Editor editor)
    {
        editor.SimulationRunning = !editor.SimulationRunning;
        editor.SetMessage(editor.TimedMessages(("This action toggles the simulation!", 3000), ("", 0)));
    }
}

[ScriptType("flisp_ram_extension_assemble")]
public class Extension : INodeContextExtension
{
    public string NodeType => "logix_builtin.script_type.RAM";
    public string MenuItemName => "Assemble from file...";

    public void Execute(Editor editor, Node node)
    {
        var fileDialog = new FileDialog(".", "Select file with FLISP assembly", FileDialogType.SelectFile, (path) =>
        {
            var ramNode = node as RAM;
            var data = ramNode.GetNodeData() as RamData;
            var memory = data.Memory;

            using (StreamReader sr = new(path))
            {
                var s = sr.ReadToEnd();
                Assembler a = new();
                var d = a.Assemble(s);
                memory = new WordAddressableMemory(d);
                data.Memory = memory;

                ramNode.Initialize(data);
            }

            editor.SetMessage(editor.TimedMessages(("Assembled from file!", 3000), ("", 0)));
        });

        editor.OpenPopup(fileDialog);
    }
}

[ScriptType("flisp_ram_extension_compile")]
public class CompileExtension : INodeContextExtension
{
    public string NodeType => "logix_builtin.script_type.RAM";
    public string MenuItemName => "Compile from FLISPC file...";

    public void Execute(Editor editor, Node node)
    {
        var fileDialog = new FileDialog(".", "Select file with FLISPC code", FileDialogType.SelectFile, (path) =>
        {
            var ramNode = node as RAM;
            var data = ramNode.GetNodeData() as RamData;
            var memory = data.Memory;

            using (StreamReader sr = new(path))
            {
                var s = sr.ReadToEnd();
                var compiler = new FLISPCCompiler();

                var ass = compiler.Compile(s);

                Console.WriteLine("------ ASSEMBLY ------");
                Console.Write(ass);

                Assembler a = new();
                var d = a.Assemble(ass);
                memory = new WordAddressableMemory(d);
                data.Memory = memory;

                ramNode.Initialize(data);
            }
        });

        editor.OpenPopup(fileDialog);
    }
}

[ScriptType("flispc_compile")]
public class ASCompileExtension : IActionSequenceExtension
{
    public RootCommand GetCommand(Simulation simulation)
    {
        var command = new RootCommand("Compile FLISPC code");

        var ram = new Argument<string>("ram", "The name of the RAM node to compile to");
        var file = new Argument<FileInfo>("file", "The FLISPC file to compile");

        command.AddArgument(ram);
        command.AddArgument(file);

        command.SetHandler((r, f) =>
        {
            if (!f.Exists)
            {
                throw new FileNotFoundException("File not found", f.FullName);
            }

            var ramNode = simulation.GetNodesOfType<RAM>().FirstOrDefault(x => (x.GetNodeData() as RamData).Label == r);

            if (ramNode == null)
            {
                throw new Exception("RAM node not found");
            }

            var data = ramNode.GetNodeData() as RamData;

            using (StreamReader sr = new(f.FullName))
            {
                var s = sr.ReadToEnd();
                var compiler = new FLISPCCompiler();

                var ass = compiler.Compile(s);

                var a = new Assembler();
                var d = a.Assemble(ass);

                data.Memory = new WordAddressableMemory(d);

                ramNode.Initialize(data);
            }

        }, ram, file);

        return command;
    }
}