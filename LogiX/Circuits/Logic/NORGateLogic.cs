using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Circuits.Logic
{
    class NORGateLogic : IGateLogic
    {
        public LogicValue GetOutput(LogicValue a, LogicValue b)
        {
            if(a == LogicValue.HIGH || b == LogicValue.HIGH)
            {
                return LogicValue.LOW;
            }
            else
            {
                return LogicValue.HIGH;
            }
        }
    }
}
