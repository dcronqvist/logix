using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Circuits.Logic
{
    class NORGateLogic : IGateLogic
    {
        public int GetExpectedInputAmount()
        {
            return 2;
        }

        public LogicValue GetOutput(LogicValue[] inputs)
        {
            LogicValue a = inputs[0];
            LogicValue b = inputs[1];

            if (a == LogicValue.HIGH || b == LogicValue.HIGH)
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
