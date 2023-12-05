using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using LogiX.Graphics;
using LogiX.Model.NodeModel;
using LogiX.Model.Simulation;
using NLua;

namespace LogiX.Scripting;

public class DataEntryNodeState(Dictionary<string, object> state) : INodeState
{
    public Dictionary<string, object> State { get; set; } = state;

    public INodeState Clone() => new DataEntryNodeState(State);
    public string GetStateEditorGUIName() => "DataEntryNodeState";
    public bool HasEditorGUI() => true;
    public bool SubmitStateEditorGUI()
    {
        foreach (string key in State.Keys)
        {
            // Assume all values to be tostringable
            object value = State[key];
            string valueString = value.ToString();

            ImGui.Text($"{key}: {valueString}");
        }

        return false;
    }
}

public class LuaNode(
    string luaDataEntryIdentifier,
    ILuaService luaService) : INode<DataEntryNodeState>
{
    public string LuaDataEntryIdentifier => luaDataEntryIdentifier;

    private DataEntryNode _dataEntryNode;
    private DataEntryNode GetDataEntryNode()
    {
        _dataEntryNode ??= luaService.GetAllDataEntries()
            .FirstOrDefault(x => x.Identifier == luaDataEntryIdentifier)
            .GetEntryAs<DataEntryNode>();

        return _dataEntryNode;
    }

    public void ConfigureUIHandlers(DataEntryNodeState state, INodeUIHandlerConfigurer configurer)
    {

    }

    public DataEntryNodeState CreateInitialState()
    {
        var luaState = GetDataEntryNode().CreateInitState.Call().First() as LuaTable;

        var state = new Dictionary<string, object>();
        foreach (object key in luaState.Keys)
        {
            state[key.ToString()] = luaState[key];
        }

        return new DataEntryNodeState(state);
    }

    public IEnumerable<PinEvent> Evaluate(DataEntryNodeState state, IPinCollection pins)
    {
        var evaluation = GetDataEntryNode()
            .Evaluate.Call(state, pins).First() as LuaTable;

        var pinEvents = evaluation.ParseLuaTableAsArrayOf<PinEvent>();

        return pinEvents;
    }

    public Vector2 GetMiddleRelativeToOrigin(DataEntryNodeState state)
    {
        return new Vector2(1, 1);
    }

    public string GetNodeName() => GetDataEntryNode().Name;

    public IEnumerable<INodePart> GetParts(DataEntryNodeState state, IPinCollection pins)
    {
        var parts = GetDataEntryNode()
            .GetParts.Call(state.State, pins).First() as LuaTable;

        return parts.Values.Cast<INodePart>();
    }

    public IEnumerable<PinConfig> GetPinConfigs(DataEntryNodeState state)
    {
        var nodePinConfigs = GetDataEntryNode()
            .GetPinConfigs.Call(state.State).First() as LuaTable;

        return nodePinConfigs.ParseLuaTableAsArrayOf<PinConfig>();
    }

    public IEnumerable<PinEvent> Initialize(DataEntryNodeState state)
    {
        var init = GetDataEntryNode()
            .Initialize.Call(state.State).First() as LuaTable;

        return init.ParseLuaTableAsArrayOf<PinEvent>();
    }
}

public class DataEntryNode
{
    [LuaMember(Name = "id")]
    public string Identifier { get; set; }

    [LuaMember(Name = "name")]
    public string Name { get; set; }

    [LuaMember(Name = "create_init_state")]
    public LuaFunction CreateInitState { get; set; }

    [LuaMember(Name = "get_pin_configs")]
    public LuaFunction GetPinConfigs { get; set; }

    [LuaMember(Name = "evaluate")]
    public LuaFunction Evaluate { get; set; }

    [LuaMember(Name = "get_parts")]
    [LuaTypeHint(TypeHint = "fun(state: table, pins: any): node_part[]")]
    public LuaFunction GetParts { get; set; }

    [LuaMember(Name = "initialize")]
    public LuaFunction Initialize { get; set; }
}
