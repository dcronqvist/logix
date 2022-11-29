using LogiX.Content.Scripting;

namespace LogiX.Architecture.Plugins;

public interface IPluginAction
{
    string Name { get; }
    void Execute(Editor editor);
}