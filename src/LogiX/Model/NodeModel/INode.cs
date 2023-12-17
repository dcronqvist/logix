using ImGuiNET;
using LogiX.Graphics;
using LogiX.Model.Simulation;
using System.Collections.Generic;
using System.Numerics;

namespace LogiX.Model.NodeModel;

public record PinConfig
{
    public PinConfig() { }

    public PinConfig(string id, int bitWidth, bool updateCausesEvaluation, Vector2i position, PinSide side, bool displayPinName)
    {
        ID = id;
        BitWidth = bitWidth;
        UpdateCausesEvaluation = updateCausesEvaluation;
        Position = position;
        Side = side;
        DisplayPinName = displayPinName;
    }

    public string ID { get; init; }

    public int BitWidth { get; init; }

    public bool UpdateCausesEvaluation { get; init; }

    public Vector2i Position { get; init; }

    public PinSide Side { get; init; }

    public bool DisplayPinName { get; init; }
}

public interface INodeState
{
    INodeState Clone();

    bool HasEditorGUI();
    bool SubmitStateEditorGUI();
}

public class EmptyState : INodeState
{
    public INodeState Clone() => new EmptyState();
    public bool HasEditorGUI() => false;
    public bool SubmitStateEditorGUI() => false;
}

public interface INode
{
    INodeState CreateInitialState();
    void ConfigureUIHandlers(INodeState state, INodeUIHandlerConfigurer configurer);
    IEnumerable<INodePart> GetParts(INodeState state, IPinCollection pins);
    IEnumerable<PinConfig> GetPinConfigs(INodeState state);
    IEnumerable<PinEvent> Evaluate(INodeState state, IPinCollection pins);
    IEnumerable<PinEvent> Initialize(INodeState state);
    Vector2 GetMiddleRelativeToOrigin(INodeState state);
    string GetNodeName();
}

public interface INode<TState> : INode where TState : INodeState
{
    new TState CreateInitialState();
    INodeState INode.CreateInitialState() => CreateInitialState();

    void ConfigureUIHandlers(TState state, INodeUIHandlerConfigurer configurer);
    void INode.ConfigureUIHandlers(INodeState state, INodeUIHandlerConfigurer configurer) => ConfigureUIHandlers((TState)state, configurer);

    IEnumerable<INodePart> GetParts(TState state, IPinCollection pins);
    IEnumerable<INodePart> INode.GetParts(INodeState state, IPinCollection pins) => GetParts((TState)state, pins);

    IEnumerable<PinConfig> GetPinConfigs(TState state);
    IEnumerable<PinConfig> INode.GetPinConfigs(INodeState state) => GetPinConfigs((TState)state);

    IEnumerable<PinEvent> Evaluate(TState state, IPinCollection pins);
    IEnumerable<PinEvent> INode.Evaluate(INodeState state, IPinCollection pins) => Evaluate((TState)state, pins);

    IEnumerable<PinEvent> Initialize(TState state);
    IEnumerable<PinEvent> INode.Initialize(INodeState state) => Initialize((TState)state);

    Vector2 GetMiddleRelativeToOrigin(TState state);
    Vector2 INode.GetMiddleRelativeToOrigin(INodeState state) => GetMiddleRelativeToOrigin((TState)state);
}

public class NorNode : INode<EmptyState>
{
    public IEnumerable<PinConfig> GetPinConfigs(EmptyState state)
    {
        yield return new PinConfig("A", 1, true, new Vector2i(0, 0), PinSide.Left, false);
        yield return new PinConfig("B", 1, true, new Vector2i(0, 2), PinSide.Left, false);
        yield return new PinConfig("Y", 1, false, new Vector2i(2, 1), PinSide.Right, false);
    }

    public IEnumerable<PinEvent> Evaluate(EmptyState state, IPinCollection pins)
    {
        var a = pins.Read("A", 0);
        var b = pins.Read("B", 0);

        if (a == LogicValue.HIGH || b == LogicValue.HIGH)
        {
            yield return new PinEvent("Y", 0, LogicValue.LOW);
            yield break;
        }

        if (a == LogicValue.UNDEFINED || b == LogicValue.UNDEFINED)
        {
            yield return new PinEvent("Y", 0, LogicValue.UNDEFINED);
            yield break;
        }

        yield return new PinEvent("Y", 0, LogicValue.HIGH);
    }

    public IEnumerable<INodePart> GetParts(EmptyState state, IPinCollection pins)
    {
        yield return new RectangleVisualNodePart(Vector2.Zero, new Vector2(2, 2), ColorF.Black, renderSelected: true);
        yield return new RectangleVisualNodePart(new Vector2(0.1f), new Vector2(1.8f, 1.8f), ColorF.White);
        yield return new TextVisualNodePart("NOR", new Vector2(1, 1), 13f);
    }

    public EmptyState CreateInitialState() => new();

    public Vector2 GetMiddleRelativeToOrigin(EmptyState state) => new(1, 1);

    public void ConfigureUIHandlers(EmptyState state, INodeUIHandlerConfigurer configurer) { }

    public IEnumerable<PinEvent> Initialize(EmptyState state) { yield break; }

    public string GetNodeName() => "NOR Gate";
}

public class PinState : INodeState
{
    public bool IsInput { get; set; }
    public LogicValue CurrentValue { get; set; }

    public INodeState Clone() => new PinState() { IsInput = IsInput, CurrentValue = CurrentValue };
    public bool HasEditorGUI() => true;

    public bool SubmitStateEditorGUI()
    {
        bool resetSimulation = false;

        bool isInput = IsInput;
        ImGui.Checkbox("Is Input", ref isInput);
        resetSimulation |= isInput != IsInput;
        IsInput = isInput;

        ImGui.Separator();

        ImGui.Text($"Current Value: {CurrentValue}");

        return resetSimulation;
    }
}

public class PinNode : INode<PinState>
{
    public IEnumerable<PinConfig> GetPinConfigs(PinState state)
    {
        yield return new PinConfig("A", 1, true, new Vector2i(0, 1), PinSide.Left, false);
    }

    public IEnumerable<PinEvent> Evaluate(PinState state, IPinCollection pins)
    {
        yield break;
    }

    public IEnumerable<INodePart> GetParts(PinState state, IPinCollection pins)
    {
        yield return new RectangleVisualNodePart(Vector2.Zero, new Vector2(2, 2), ColorF.Black, renderSelected: true);
        yield return new RectangleVisualNodePart(new Vector2(0.1f), new Vector2(1.8f, 1.8f), ColorF.White);
        yield return new RectangleVisualNodePart(new Vector2(0.3f), new Vector2(1.4f, 1.4f), ColorF.Black);

        IEnumerable<PinEvent> middleRightClicked()
        {
            if (!state.IsInput)
                yield break;

            state.CurrentValue = state.CurrentValue == LogicValue.HIGH ? LogicValue.LOW : LogicValue.HIGH;
            yield return new PinEvent("A", 0, state.CurrentValue);
        }

        var color = state.IsInput ? state.CurrentValue.GetValueColor() : pins.Read("A", 0).GetValueColor();
        yield return new RectangleVisualNodePart(new Vector2(0.35f), new Vector2(1.3f, 1.3f), color, middleRightClicked);
    }

    public Vector2 GetMiddleRelativeToOrigin(PinState state) => new(1, 1);

    public PinState CreateInitialState() => new() { CurrentValue = LogicValue.LOW, IsInput = true };

    public void ConfigureUIHandlers(PinState state, INodeUIHandlerConfigurer configurer) { }

    public IEnumerable<PinEvent> Initialize(PinState state)
    {
        if (!state.IsInput)
            yield break;

        yield return new PinEvent("A", 0, state.CurrentValue);
    }

    public string GetNodeName() => "Pin";
}
