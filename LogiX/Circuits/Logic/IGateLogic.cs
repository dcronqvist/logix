using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Circuits.Logic
{
    interface IGateLogic
    {
        public LogicValue GetOutput(LogicValue a, LogicValue b);
    }
}
