using System.Collections;
using LogiX.Model.Circuits;
using LogiX.Model.Commands;
using LogiX.Model.Simulation;
using LogiX.UserInterface.Coroutines;

namespace LogiX.UserInterface.Views.EditorView;

public partial class EditorView
{
    public IEnumerator OpenCircuit(ICircuitDefinition circuitDefinition)
    {
        if (_invoker.CanUndo())
        {
            bool result = false;
            yield return CoroutineHelpers.ShowMessageBoxOKCancel("Unsaved changes", "You have unsaved changes. Do you want to\nsave them before opening a new circuit?\nCancel to do nothing.", (r) => result = r);
            if (!result)
                yield break;
        }

        _currentlySimulatedCircuitDefinition = new ThreadSafe<ICircuitDefinition>(circuitDefinition);
        var simulator = _currentlySimulatedCircuitDefinition.Locked(cd =>
        {
            var simulator = new Simulator(cd, _nodeUIHandlerConfigurer);
            cd.Subscribe(simulator);
            return simulator;
        });
        _simulator = new ThreadSafe<ISimulator>(simulator);
        _currentlySimulatedCircuitViewModel = new CircuitDefinitionViewModel(_currentlySimulatedCircuitDefinition, _simulator);

        _invoker = new Invoker();

        yield break;
    }
}
