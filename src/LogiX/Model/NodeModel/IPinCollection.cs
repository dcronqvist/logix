using System;
using System.Collections.Generic;
using System.Linq;

namespace LogiX.Model.NodeModel;

public interface IPinCollection
{
    IReadOnlyCollection<LogicValue> Read(string pinID);
    LogicValue Read(string pinID, int index) => Read(pinID).ElementAt(index);
}
