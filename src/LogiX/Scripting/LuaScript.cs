using Symphony;

namespace LogiX.Scripting;

public class LuaScript : Content<LuaScript>
{
    private string _scriptContent;

    public LuaScript(string scriptContent) => _scriptContent = scriptContent;

    protected override void OnContentUpdated(LuaScript newContent) => _scriptContent = newContent.GetScriptContent();

    public override void Unload()
    {

    }

    public string GetScriptContent() => _scriptContent;
}
