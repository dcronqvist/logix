using System.Collections.Generic;
using System.Linq;

namespace LogiX.Model.NodeModel;

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public interface IPinCollection
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    IReadOnlyCollection<LogicValue> Read(string pinID);

    LogicValue Read(string pinID, int index) => Read(pinID).ElementAt(index);
}
