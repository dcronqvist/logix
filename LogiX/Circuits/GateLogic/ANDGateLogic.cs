using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Circuits.GateLogic
{
    class ANDGateLogic : IGateLogic
    {
        public LogicValue GetGateOutput(LogicValue a, LogicValue b)
        {
            if(a == LogicValue.NAN || b == LogicValue.NAN)
            {
                return LogicValue.NAN;
            }
            
            if(a == LogicValue.HIGH && b == LogicValue.HIGH)
            {
                return LogicValue.HIGH;
            }
            else
            {
                return LogicValue.LOW;
            }
        }
    }
}
