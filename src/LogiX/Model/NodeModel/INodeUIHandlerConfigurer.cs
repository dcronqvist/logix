using DotGLFW;
using LogiX.Model.Simulation;
using System;

namespace LogiX.Model.NodeModel;

public interface INodeUIHandlerConfigurer
{
    void SetCurrentConfiguringNode(Guid nodeID);

    void ClearAllHandlers();

    INodeUIHandlerConfigurer IfCondition(Func<bool> condition);
    INodeUIEventProvider<char> OnCharacterTyped();
    INodeUIEventProvider<Keys> OnKeyPressed();

    bool TryGetNextPinEventFrom(out Guid nodeID, out PinEvent pinEvent);
}
