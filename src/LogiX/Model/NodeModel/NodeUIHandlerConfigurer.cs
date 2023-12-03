using DotGLFW;
using LogiX.Input;
using LogiX.Model.Simulation;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks.Dataflow;

namespace LogiX.Model.NodeModel;

public class NodeUIHandlerConfigurer(
    IKeyboard<char, Keys, ModifierKeys> keyboard,
    IMouse<MouseButton> mouse,
    Func<Vector2> getMousePositionInWorkspace,
    Func<bool> conditionFunc = null) : INodeUIHandlerConfigurer
{
    private readonly BufferBlock<(Guid, PinEvent)> _pinEventBuffer = new();
    private readonly List<INodeUIHandlerConfigurer> _childConfigurers = [];

    public void ClearAllHandlers()
    {
        _onCharacterTypedSubscribers.ForEach(keyboard.CharacterTypedEventProvider.Unsubscribe);
        _onCharacterTypedSubscribers.Clear();

        _onKeyPressedSubscribers.ForEach(keyboard.KeyPressedEventProvider.Unsubscribe);
        _onKeyPressedSubscribers.Clear();

        _childConfigurers.ForEach(x => x.ClearAllHandlers());
    }

    private static void StoreSubscription(ref List<Guid> list, Guid subscriptionID) => list.Add(subscriptionID);

    private void CollectPinEventsFrom(IEnumerable<(Guid, PinEvent)> pinEvents)
    {
        foreach (var pinEvent in pinEvents)
        {
            _pinEventBuffer.Post(pinEvent);
        }
    }

    public INodeUIHandlerConfigurer IfCondition(Func<bool> condition)
    {
        var newConditionalHandler = new NodeUIHandlerConfigurer(
            keyboard,
            mouse,
            getMousePositionInWorkspace,
            condition);

        _childConfigurers.Add(newConditionalHandler);

        newConditionalHandler.SetCurrentConfiguringNode(_currentConfiguringNode);
        return newConditionalHandler;
    }

    private List<Guid> _onCharacterTypedSubscribers = [];
    public INodeUIEventProvider<char> OnCharacterTyped()
    {
        var eventProvider = new NodeUIEventProvider<char>(_currentConfiguringNode);
        StoreSubscription(
            ref _onCharacterTypedSubscribers,
            keyboard.CharacterTypedEventProvider.Subscribe((characterTyped) =>
            {
                if (conditionFunc is not null && !conditionFunc())
                    return;

                CollectPinEventsFrom(eventProvider.NotifySubscribers(characterTyped));
            })
        );
        return eventProvider;
    }

    private Guid _currentConfiguringNode;
    public void SetCurrentConfiguringNode(Guid nodeID) => _currentConfiguringNode = nodeID;

    public bool TryGetNextPinEventFrom(out Guid nodeID, out PinEvent pinEvent)
    {
        if (_pinEventBuffer.TryReceive(out var pinEventTuple))
        {
            nodeID = pinEventTuple.Item1;
            pinEvent = pinEventTuple.Item2;
            return true;
        }

        foreach (var child in _childConfigurers)
        {
            if (child.TryGetNextPinEventFrom(out nodeID, out pinEvent))
                return true;
        }

        nodeID = default;
        pinEvent = default;
        return false;
    }

    private List<Guid> _onKeyPressedSubscribers = [];
    public INodeUIEventProvider<Keys> OnKeyPressed()
    {
        var eventProvider = new NodeUIEventProvider<Keys>(_currentConfiguringNode);
        StoreSubscription(
            ref _onKeyPressedSubscribers,
            keyboard.KeyPressedEventProvider.Subscribe((keyPressed) =>
            {
                if (conditionFunc is not null && !conditionFunc())
                    return;

                CollectPinEventsFrom(eventProvider.NotifySubscribers(keyPressed));
            })
        );

        return eventProvider;
    }
}
