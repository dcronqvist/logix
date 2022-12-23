using System.CommandLine;

namespace LogiX.Architecture.Plugins;

public interface IActionSequenceExtension
{
    RootCommand GetCommand(Simulation simulation);
}