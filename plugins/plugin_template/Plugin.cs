using LogiX;
using LogiX.Components;
using LogiX.SaveSystem;
using LogiX.Editor;
using System.Numerics;
using System.Text.Json;
using ImGuiNET;

// Below is an example of a plugin.
// You can create your own plugins by following the same structure.
// PluginMethods will be displayed in the editor as a list of methods that can be called in the main menu bar.
// CustomComponents will be displayed in the editor as a list of components in the sidebar that can be dragged out and placed.

public class ExampleMethod : PluginMethod
{
    // This is the name of this method, and it will be displayed in the editor.
    public override string Name => "Example Method";
    // This is the description of this method, it is currently not being displayed in the editor.
    public override string Description => "This example method does nothing.";
    // Used to determine wether or not the function can be run from the editor UI.
    public override Func<Editor, bool> CanRun => (editor) => true;
    // When clicking the method in the editor, this is the method that will be called.
    public override Execution OnRun => (Editor editor, out string? error) =>
    {
        // This is the code that will be run when the method is clicked.
        // The editor is passed in as a parameter, and a potential error string
        // can be returned. If an error has ocurred, you must specify this by returning false and 
        // setting the error string.
        editor.ModalError("You pressed a button!");

        error = null;
        return true;
    };
}

public class ExampleMethod2 : PluginMethod
{
    public override string Name => "Example Method 2";
    public override string Description => "This example method does nothing either.";
    public override Func<Editor, bool> CanRun => (editor) => true;
    public override Execution OnRun => (Editor editor, out string? error) =>
    {
        error = "An error has ocurred!";
        return false;
    };
}

public class CustomANDGate : CustomComponent
{
    public class CustomANDGateData : CustomComponentData
    {
        public int CustomData { get; set; }
    }

    public int CustomData { get; set; }

    public override bool HasContextMenu => true;

    // The constructor of ALL custom components must ALWAYS expect a Vector2 for position
    // and a JObject for your component's data. This data is saved in project files and
    // will be loaded when the project is loaded, to restore potential states of components.
    public CustomANDGate(Vector2 position, CustomANDGateData data) : base("example-plugin:custom-and", "Custom AND", Util.Listify(1, 1), Util.Listify(1), position)
    {
        // This is where you would load your data from the JObject, if you had any.
        // This is where you would set up your component's properties.
        this.CustomData = data.CustomData;
    }

    public override Dictionary<string, int> GetGateAmount()
    {
        // This is the amount of gates/components your component "has".
        // This is used to determine how many gates/components that are in the workspace.
        // It has no actual value, it is only for being able to display the amount of gates/components in the editor.
        return Util.GateAmount(("Custom AND Gate", 1));
    }

    public static CustomANDGateData GetDefaultComponentData()
    {
        // This static method MUST exist on all custom components.
        // It should return the initial data you would like a component to have.
        return new CustomANDGateData() { CustomData = 42 };
    }

    public override void PerformLogic()
    {
        // This is where you would perform your logic.
        // That usually means to read the inputs, perform some kind
        // of operations based on those inputs, and then set the outputs 
        // of the component to the result of that operation.
        // In this example, we simply mimic the behaviour of an AND gate.

        bool a = this.InputAt(0).Values[0] == LogicValue.HIGH ? true : false;
        bool b = this.InputAt(1).Values[0] == LogicValue.HIGH ? true : false;

        this.OutputAt(0).SetAllValues(a && b ? LogicValue.HIGH : LogicValue.LOW);
    }

    public override CustomDescription ToDescription()
    {
        // This is where you would convert your component to a description.
        // This is used to save the component's state in project files.
        // The supplied JsonDocument is used to store the data of the component, if you'd like to save it.
        // This data will be inputted to the created component in the constructor when creating this component again
        // from e.g. a project file.
        // The IODescriptions at the end should be the same as the ones you used in the constructor.
        // Util.Listify(1, 1), Util.Listify(1) -> Util.Listify(new IODescription(1), new IODescription(1)), Util.Listify(new IODescription(1))
        return new CustomDescription("example-plugin:custom-and", "Custom AND", GetDefaultComponentData(), this.Position, this.Rotation, Util.Listify(new IODescription(1), new IODescription(1)), Util.Listify(new IODescription(1)));
    }

    public override void SubmitContextPopup(Editor editor)
    {
        ImGui.Text("This text is displayed in the context menu.");
    }

    public override void OnSingleSelectedSubmitUI()
    {
        ImGui.Begin("Custom AND Gate");
        ImGui.Text("This text is displayed as a window when the component is selected.");
        ImGui.End();
    }
}
