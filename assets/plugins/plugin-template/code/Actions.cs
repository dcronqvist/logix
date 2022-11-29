using ImGuiNET;
using LogiX.Architecture;
using LogiX.Architecture.Plugins;
using LogiX.Content.Scripting;

namespace TemplatePlugin;

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