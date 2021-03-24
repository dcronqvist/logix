using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Circuits.Logic
{
    class ORGateLogic : IGateLogic
    {
        public int GetExpectedInputAmount()
        {
            return 2;
        }

        public LogicValue GetOutput(LogicValue[] inputs)
        {
            LogicValue a = inputs[0];
            LogicValue b = inputs[1];

            if (a == LogicValue.NAN || b == LogicValue.NAN)
            {
                return LogicValue.NAN;
            }

            if (a == LogicValue.HIGH || b == LogicValue.HIGH)
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
