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
using D2Plugin;

namespace D2Plugin;

// [ScriptType("template_action_1")]
// public class TestPluginAction1 : IPluginAction
// {
//     public string Name => "Template Action 1";

//     public void Execute(Editor editor)
//     {
//         editor.OpenPopup("Template Action 1, popup!", (e) =>
//         {
//             ImGui.Text("This action just opens a popup! Super simple!");
//         });
//     }
// }

// [ScriptType("template_action_2")]
// public class TestPluginAction2 : IPluginAction
// {
//     public string Name => "Template Action 2";

//     public void Execute(Editor editor)
//     {
//         editor.SimulationRunning = !editor.SimulationRunning;
//         editor.SetMessage(editor.TimedMessages(("This action toggles the simulation!", 3000), ("", 0)));
//     }
// }

[ScriptType("ram_context_extension")]
public class RAMContextExtension : INodeContextExtension
{
    public string NodeType => "logix_builtin.script_type.RAM";
    public string MenuItemName => "Assemble from D2Assembly";

    public void Execute(Editor editor, Node node)
    {
        var fileDialog = new FileDialog(FileDialog.LastDirectory, "Select D2Assembly file", FileDialogType.SelectFile, (path) =>
        {
            var ramNode = node as RAM;
            var data = ramNode.GetNodeData() as RamData;

            using (StreamReader sr = new StreamReader(path))
            {
                var text = sr.ReadToEnd();
                var assembler = new D2Assembler();

                var bytes = assembler.Assemble(text);

                data.Memory = new WordAddressableMemory(bytes.ToArray());

                node.Initialize(data);
                editor.SetMessage(editor.TimedMessages(("Assembled D2Assembly file!", 3000), ("", 0)));
            }
        });

        editor.OpenPopup(fileDialog);
    }
}