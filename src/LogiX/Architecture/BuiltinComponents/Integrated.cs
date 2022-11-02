using System.Numerics;
using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Graphics;
using LogiX.Graphics.UI;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public class IntegratedData : IComponentDescriptionData
{
    public Guid CircuitID { get; set; }

    public static IComponentDescriptionData GetDefault()
    {
        return new IntegratedData()
        {
            CircuitID = Guid.Empty
        };
    }
}

public class IntegratedError : SimulationError
{
    public SimulationError Error { get; set; }
    public Integrated Comp { get; set; }

    public IntegratedError(SimulationError internalError, Integrated comp) : base($"INTERNAL ERROR: {internalError.Message}")
    {
        this.Error = internalError;
        this.Comp = comp;
    }

    public override void Render(Camera2D cam)
    {
        var pos = this.Comp.Position;
        var size = this.Comp.GetBoundingBox(out _).GetSize().ToVector2i(Constants.GRIDSIZE);

        var middle = new Vector2i(pos.X + size.X / 2, pos.Y + size.Y / 2).ToVector2(Constants.GRIDSIZE);
        var tShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.text");
        var font = LogiX.ContentManager.GetContentItem<Font>("content_1.font.default");

        TextRenderer.RenderText(tShader, font, this.Message, middle, 0.5f, ColorF.Red, cam);
    }
}

[ScriptType("INTEGRATED"), ComponentInfo("INTEGRATED", "BuiltIn", true)]
public class Integrated : Component<IntegratedData>
{
    public override string Name => this._circuit.Name;
    public override bool DisplayIOGroupIdentifiers => true;
    public override bool ShowPropertyWindow => true;

    private IntegratedData _data;
    private Simulation _simulation;
    private Dictionary<string, Vector2i> _switchPositions;
    private Circuit _circuit;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return _data;
    }

    public static ComponentDescription CreateDescriptionFromCircuit(string circuitName, Circuit circuit)
    {
        var data = new IntegratedData()
        {
            CircuitID = circuit.ID
        };
        var integrated = new Integrated();
        integrated.Initialize(data);

        return integrated.GetDescriptionOfInstance();
    }

    public override void Initialize(IntegratedData data)
    {
        this._data = data;
        this._switchPositions = new();
        this._circuit = ComponentDescription.GetIntegratedProjectCircuitByID(this._data.CircuitID);
        this._simulation = Simulation.FromCircuit(this._circuit, "logix_builtin.script_type.PIN");
        var switches = this._circuit.GetAllSwitches();

        int i = 0;
        foreach (var s in switches)
        {
            var sData = s.Data as PinData;
            var label = sData.Label == "" || sData.Label is null ? $"{i++}" : sData.Label;
            this.RegisterIO(label, sData.Bits, sData.Side);
            this._switchPositions.Add(label, s.CreateComponent().GetPositionForIO(0, out _));
        }
    }

    private Simulation integSim;
    public override void Update(Simulation simulation, bool performLogic = true)
    {
        this.integSim = simulation;
        var ios = this.IOs;
        var amountOfIOs = ios.Length;

        this._simulation.PreviousErrors = this._simulation.Errors;
        this._simulation.Errors.Clear();
        this._simulation.NewValues = new();

        var inputs = new List<IO>();
        var outputs = new List<IO>();

        for (int i = 0; i < amountOfIOs; i++)
        {
            var io = ios[i];
            var ioPos = this.GetPositionForIO(io, out _);
            if (simulation.TryGetLogicValuesAtPosition(ioPos, io.Bits, out var values, out var status, out var fromIO, out var fromComp) && fromIO != io)
            {
                // If there are values that can be read from the parent sim,
                // then we will assume this IO to be an input and read from them
                inputs.Add(io);
            }
            else
            {
                // If there are no values that can be read from the parent sim,
                // then we will assume this IO to be an output and write to them
                outputs.Add(io);
            }
        }

        foreach (var ioInput in inputs)
        {
            var ioPos = this.GetPositionForIO(ioInput, out _);
            var internalPos = this._switchPositions[ioInput.Identifier];

            if (simulation.TryGetLogicValuesAtPosition(ioPos, ioInput.Bits, out var values, out var status, out var fromIO, out var fromComp))
            {
                ioInput.SetValues(values);
                this._simulation.PushValuesAt(internalPos, ioInput, this, values);
            }
        }

        foreach (Component component in this._simulation.Components)
        {
            component.Update(this._simulation);
        }

        foreach (var ioOutput in outputs)
        {
            var ioPos = this.GetPositionForIO(ioOutput, out _);
            var internalPos = this._switchPositions[ioOutput.Identifier];

            if (this._simulation.TryGetLogicValuesAtPosition(internalPos, ioOutput.Bits, out var values, out var status, out var fromIO, out var fromComp))
            {
                ioOutput.SetValues(values);
                simulation.PushValuesAt(ioPos, ioOutput, this, values);
            }
            else
            {
                ioOutput.SetValues(Enumerable.Repeat(LogicValue.UNDEFINED, ioOutput.Bits).ToArray());
            }
        }


        this._simulation.CurrentValues = this._simulation.NewValues;

        if (this._simulation.Errors.Count > 0)
        {
            foreach (var error in this._simulation.Errors)
            {
                simulation.AddError(new IntegratedError(error, this));
            }
        }
    }

    public override void PerformLogic()
    {
        var ios = this.IOs;
        this._simulation.Tick(typeof(Pin));
    }

    public override void Render(Camera2D camera)
    {
        base.Render(camera);
    }

    public override void SubmitUISelected(Editor editor, int componentIndex)
    {
        ImGui.Text($"ID: {this._circuit.ID}");
        ImGui.Text($"ITERATION: {this._circuit.IterationID}");
        ImGui.Text($"NAME: {this._circuit.Name}");
        ImGui.Text($"INDEX: {componentIndex}");
    }
}