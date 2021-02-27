using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Circuits.GateLogic
{
    interface IGateLogic
    {
        public LogicValue GetGateOutput(LogicValue a, LogicValue b);
    }
}
