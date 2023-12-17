using System.Collections.Generic;
using LogiX.Model.NodeModel;
using LogiX.Model.Projects;

namespace LogiX.Addons;

public interface IAddonService
{
    IEnumerable<IAddon> GetAddons();
}

public class AddonService : IAddonService
{
    public IEnumerable<IAddon> GetAddons()
    {
        yield return new LogiXCoreAddon();
    }
}

public class LogiXCoreAddon : IAddon
{
    public IVirtualFileTree<string, INode> GetAddonNodeTree()
    {
        var tree = new VirtualFileTree<INode>("root");

        tree.AddDirectory("LogiX Core")
            .AddFile("nor", new NorNode())
            .AddFile("pin", new PinNode());

        return tree;
    }
}
