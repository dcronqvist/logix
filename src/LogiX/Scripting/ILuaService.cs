using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using LogiX.Content;
using LogiX.Debug.Logging;
using LogiX.Graphics;
using LogiX.Model;
using LogiX.Model.NodeModel;
using LogiX.Model.Simulation;
using NLua;
using Symphony;

namespace LogiX.Scripting;

public record LuaAPIFile(string FileName, string Content);

public interface ILuaService
{
    object GetGlobal(string name);
    T GetGlobal<T>(string name) => (T)GetGlobal(name);
    void SetGlobal(string name, object value);
    object[] Call(string name, params object[] args);
    IReadOnlyCollection<LuaDataEntry> GetAllDataEntries(Predicate<LuaDataEntry> predicate = null);
    IEnumerable<LuaAPIFile> GetEmmyLuaAPI();
}

public class LuaService(
    IFactory<Lua> luaFactory,
    IContentManager<ContentMeta> contentManager,
    ILog log) : ILuaService, IDisposable
{
    private const string ConstantsTable = "defs";

    private readonly IFactory<Lua> _luaFactory = luaFactory;
    private readonly IContentManager<ContentMeta> _contentManager = contentManager;
    private readonly ILog _log = log;

    private Lua _luaState;
    private List<LuaDataEntry> _dataEntries = null;

    private IEnumerable<ILuaFacingFunction> GetAvailableFunctions()
    {
        yield return new LuaFacingFunctionDeprecated(new LuaFacingFunctionDefinedInLua(
            luaFunctionName: "import",
            luaFunction:
            """
            import = function () end
            """,
            returnType: null,
            docs: "Do not use this function. It has been sandboxed away",
            parameters: [],
            genericTypes: []));

        yield return new LuaFacingFunctionDefinedInLua(
            luaFunctionName: "round",
            luaFunction:
            """
            function round(num, numDecimalPlaces)
                local mult = 10^(numDecimalPlaces or 0)
                return math.floor(num * mult + 0.5) / mult
            end
            """,
            returnType: "number",
            docs: "Rounds a number to a given number of decimal places",
            parameters: [("num", "number"), ("numDecimalPlaces", "integer")],
            genericTypes: []);

        yield return new LuaFacingFunctionDefinedInLua(
            luaFunctionName: "find",
            luaFunction:
            """
            function find(table, key, value)
                for _, v in pairs(table) do
                    if v[key] == value then
                        return v
                    end
                end

                return nil
            end
            """,
            returnType: "T",
            docs: "Finds a value in a table by a given key and value",
            parameters: [("table", "T[]"), ("key", "string"), ("value", "any")],
            genericTypes: ["T"]);

        yield return new LuaFacingFunctionDefinedInLua(
            luaFunctionName: "deepcopy",
            luaFunction:
            """
            function deepcopy(orig)
                local orig_type = type(orig)
                local copy
                if orig_type == 'table' then
                    copy = {}
                    for orig_key, orig_value in next, orig, nil do
                        copy[deepcopy(orig_key)] = deepcopy(orig_value)
                    end
                    setmetatable(copy, deepcopy(getmetatable(orig)))
                else -- number, string, boolean, etc
                    copy = orig
                end
                return copy
            end
            """,
            returnType: "T",
            docs: "Deep copies a value (including tables)",
            parameters: [("orig", "T")],
            genericTypes: ["T"]);

        yield return new LuaFacingFunctionDelegate(
            luaFunctionName: "require",
            docs: "Requires a script",
            delegateFactory: (string scriptSourceIdentifier) => (string file) =>
            {
                string requiredScriptSourceIdentifier = _contentManager.GetSourceIdentifierForContent(file);
                var scriptItem = _contentManager.GetContentItem(file);
                var script = _contentManager.GetContent<LuaScript>(file);
                if (script == null)
                {
                    _log.LogMessage(LogLevel.Error, $"Could not find script: {file}");
                }

                _log.LogMessage(LogLevel.Debug, $"Requiring script: {file}");

                object[] result = RunScript(script, scriptItem.Identifier, requiredScriptSourceIdentifier);

                if (result.Length != 0)
                    return result[0];

                return null;
            });

        yield return new LuaFacingFunctionDelegate(
            luaFunctionName: "table.extend",
            docs: "Extends a table with another table, in place",
            delegateFactory: (string scriptSourceIdentifier) => (LuaTable t1, LuaTable t2) => LuaServiceHelpers.ExtendData(t1, t2));

        yield return new LuaFacingFunctionDelegate(
            luaFunctionName: "table.concat",
            docs: "Extends a table with another table, in place",
            delegateFactory: (string scriptSourceIdentifier) => (LuaTable t1, LuaTable t2) =>
            {
                LuaServiceHelpers.ExtendData(t1, t2);
                return t1;
            });

        yield return new LuaFacingFunctionDelegate(
            luaFunctionName: "log",
            docs: "Logs a message",
            delegateFactory: (string scriptSourceIdentifier) => (LogLevel logLevel, string s) =>
            {
                Task.Run(() => _log.LogMessage(logLevel, s));
                return;
            });

        yield return new LuaFacingFunctionDelegate(
            luaFunctionName: "new_id",
            docs: "Creates a new identifier, with the given id as a suffix to the script source identifier",
            delegateFactory: (string scriptSourceIdentifier) => (string id) => $"{scriptSourceIdentifier}:{id}");

        yield return new LuaFacingFunctionDelegate(
            luaFunctionName: "color_rgba",
            docs: "Creates a new color",
            delegateFactory: (string scriptSourceIdentifier) => (float r, float g, float b, float a) => new ColorF(r, g, b, a));

        yield return new LuaFacingFunctionDelegate(
            luaFunctionName: "get_logic_value_color",
            docs: "Gets the color for a given logic value",
            delegateFactory: (string scriptSourceIdentifier) => (LogicValue logicValue) => logicValue.GetValueColor());

        yield return new LuaFacingFunctionDelegate(
            luaFunctionName: "part_rect",
            docs: "Returns a rectangle part",
            delegateFactory: (string scriptSourceIdentifier) => (LuaTable position, LuaTable size, ColorF color, bool renderSelected) =>
            {
                return new RectangleVisualNodePart(
                    position: position.ParseLuaTableAs<Vector2>(),
                    size: size.ParseLuaTableAs<Vector2>(),
                    color: color,
                    renderSelected: renderSelected);
            });

        yield return new LuaFacingFunctionDelegate(
            luaFunctionName: "part_rect_rightclickable",
            docs: "Returns a rectangle part that can be right clicked",
            delegateFactory: (string scriptSourceIdentifier) => (LuaTable position, LuaTable size, ColorF color, bool renderSelected, LuaFunction rightClick) =>
            {
                return new RectangleVisualNodePart(
                    position: position.ParseLuaTableAs<Vector2>(),
                    size: size.ParseLuaTableAs<Vector2>(),
                    color: color,
                    renderSelected: renderSelected,
                    onRightClicked: () => (rightClick.Call().First() as LuaTable).ParseLuaTableAsArrayOf<PinEvent>());
            });

        yield return new LuaFacingFunctionDelegate(
            luaFunctionName: "part_text",
            docs: "Returns a text part",
            delegateFactory: (string scriptSourceIdentifier) => (LuaTable position, string text, float scale) =>
            {
                return new TextVisualNodePart(
                    text: text,
                    position: position.ParseLuaTableAs<Vector2>(),
                    scale: scale);
            });

        yield return new LuaFacingFunctionDelegate(
            luaFunctionName: "list_files_in_dir",
            docs: "Lists all files in an asset directory",
            delegateFactory: (string scriptSourceIdentifier) => (string dir) =>
            {
                string directory = $"{scriptSourceIdentifier}:{dir}";

                string[] items = _contentManager
                    .GetContentItems()
                    .Where(x => x.Identifier.StartsWith(directory))
                    .Select(x => x.Identifier).ToArray();

                return items.ToLuaTable(_luaState);
            });
    }

    private static IEnumerable<ILuaFacingType> GetAvailableTypes()
    {
        yield return new LuaFacingType<LuaDataEntry>();
        yield return new LuaFacingType<PinConfig>();
        yield return new LuaFacingType<Vector2i>();
        yield return new LuaFacingType<PinEvent>();
        yield return new LuaFacingType<ColorF>();
    }

    private static IEnumerable<ILuaFacingConstant> GetAvailableConstants()
    {
        yield return new LuaFacingEnum<ScriptingDataType>(ConstantsTable);
        yield return new LuaFacingEnum<LogLevel>(ConstantsTable);
        yield return new LuaFacingEnum<LogicValue>(ConstantsTable);
        yield return new LuaFacingEnum<PinSide>(ConstantsTable);

        yield return new LuaFacingConstDict<ColorF>(ConstantsTable, "colors", new Dictionary<string, ColorF>() {
            { "black", ColorF.Black },
            { "white", ColorF.White }
        });
    }

    private void BeginStage() => _luaState ??= _luaFactory.Create();

    private object[] RunScript(LuaScript script, string scriptIdentifier, string scriptSourceIdentifier)
    {
        var availableFunctions = GetAvailableFunctions();
        foreach (var function in availableFunctions)
        {
            function.Register(_luaState, scriptSourceIdentifier);
        }

        _luaState.DoString($"{ConstantsTable} = {{}}");
        var availableConstants = GetAvailableConstants();
        foreach (var constant in availableConstants)
        {
            constant.Register(_luaState);
        }

        return _luaState.DoString(script.GetScriptContent(), scriptIdentifier);
    }

    private IReadOnlyCollection<(LuaScript, string, string)> GetScriptsFromContentManagerInCorrectLoadOrderFromPattern(string pattern)
    {
        var loadOrder = _contentManager.GetLoadedSourcesMetadata()
            .Select(x => x.Source)
            .ToList();

        var scripts = _contentManager.GetContentItemsOfType<LuaScript>()
            .Where(x => x.Identifier.EndsWith(pattern, StringComparison.Ordinal))
            .OrderBy(x => loadOrder.IndexOf(x.FinalSource))
            .Select(x => (x.Content as LuaScript, x.Identifier, _contentManager.GetSourceIdentifierForContent(x.Identifier)));

        return scripts.ToList();
    }

    private static List<LuaDataEntry> GetLuaDataEntries(LuaTable table)
    {
        var entries = new List<LuaDataEntry>();

        foreach (KeyValuePair<object, object> kvp in table)
        {
            // Assume all values to be LuaTables
            var subTable = (LuaTable)kvp.Value;
            entries.Add(new LuaDataEntry(subTable));
        }

        return entries;
    }

    private List<LuaDataEntry> LoadAllDataEntries()
    {
        BeginStage();

        _luaState.DoString("data = {}");

        var dataScripts = GetScriptsFromContentManagerInCorrectLoadOrderFromPattern(":data.lua");
        dataScripts.ToList().ForEach(x => RunScript(x.Item1, x.Item2, x.Item3));

        return GetLuaDataEntries(_luaState.GetTable("data"));
    }

    public object[] Call(string name, params object[] args) =>
        _luaState.GetFunction(name).Call(args);

    public object GetGlobal(string name) => _luaState[name];

    public void SetGlobal(string name, object value) => _luaState[name] = value;

    public void Dispose() => GC.SuppressFinalize(this);

    public IReadOnlyCollection<LuaDataEntry> GetAllDataEntries(Predicate<LuaDataEntry> predicate = null)
    {
        _dataEntries ??= LoadAllDataEntries();

        if (predicate == null)
            return _dataEntries;

        return _dataEntries.Where(x => predicate(x)).ToList();
    }

    public IEnumerable<LuaAPIFile> GetEmmyLuaAPI()
    {
        var sb = new StringBuilder();
        var availableFunctions = GetAvailableFunctions();
        foreach (var function in availableFunctions)
        {
            function.WriteEmmyLua(sb);
        }

        yield return new LuaAPIFile("functions.lua", sb.ToString());

        sb = new StringBuilder();
        var availableTypes = GetAvailableTypes();
        foreach (var type in availableTypes)
        {
            type.WriteEmmyLua(sb);
        }

        yield return new LuaAPIFile("types.lua", sb.ToString());

        sb = new StringBuilder();
        sb.AppendLine($"---@type lua_data_entry[]");
        sb.AppendLine($"data = {{}}");
        sb.AppendLine($"{ConstantsTable} = {{}}\n");
        var availableConstants = GetAvailableConstants();
        foreach (var constant in availableConstants)
        {
            constant.WriteEmmyLua(sb);
        }

        yield return new LuaAPIFile("constants.lua", sb.ToString());
    }
}
