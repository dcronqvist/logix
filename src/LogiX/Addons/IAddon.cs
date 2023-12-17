using LogiX.Model.NodeModel;
using LogiX.Model.Projects;

namespace LogiX.Addons;

public interface IAddon
{
    IVirtualFileTree<string, INode> GetAddonNodeTree();
}
