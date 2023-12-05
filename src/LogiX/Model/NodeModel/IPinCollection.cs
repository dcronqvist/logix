using System;
using System.Collections.Generic;
using System.Linq;
using NLua;

namespace LogiX.Model.NodeModel;

public interface IPinCollection
{
    [LuaMember(Name = "read")]
    IReadOnlyCollection<LogicValue> Read(string pinID);

    [LuaMember(Name = "read_at")]
    LogicValue Read(string pinID, int index) => Read(pinID).ElementAt(index);
}
