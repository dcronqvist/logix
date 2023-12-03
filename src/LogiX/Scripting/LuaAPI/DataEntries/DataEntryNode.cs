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
    ILuaService luaService) : INode
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

    public void ConfigureUIHandlers(INodeState state, INodeUIHandlerConfigurer configurer)
    {

    }

    public INodeState CreateInitialState()
    {
        var luaState = GetDataEntryNode().CreateInitState.Call().First() as LuaTable;

        var state = new Dictionary<string, object>();
        foreach (object key in luaState.Keys)
        {
            state[key.ToString()] = luaState[key];
        }

        return new DataEntryNodeState(state);
    }

    public IEnumerable<PinEvent> Evaluate(INodeState state, IPinCollection pins)
    {
        yield break;
    }

    public Vector2 GetMiddleRelativeToOrigin(INodeState state)
    {
        return new Vector2(1, 1);
    }

    public string GetNodeName() => GetDataEntryNode().Name;

    public IEnumerable<INodePart> GetParts(INodeState state, IPinCollection pins)
    {
        yield return new RectangleVisualNodePart(new Vector2(0, 0), new Vector2(2, 2), ColorF.White, renderSelected: true);
    }

    public IEnumerable<PinConfig> GetPinConfigs(INodeState state)
    {
        return (GetDataEntryNode().GetPinConfigs.Call()[0] as LuaTable).Values.Cast<LuaTable>().Select(table => LuaDataEntry.ParseData(typeof(PinConfig), table) as PinConfig);
    }

    public IEnumerable<PinEvent> Initialize(INodeState state)
    {
        yield break;
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
}
