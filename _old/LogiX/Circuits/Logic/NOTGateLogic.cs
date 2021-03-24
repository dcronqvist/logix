using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Circuits.Logic
{
    class NOTGateLogic : IGateLogic
    {
        public int GetExpectedInputAmount()
        {
            return 1;
        }

        public LogicValue GetOutput(LogicValue[] inputs)
        {
            if (inputs[0] == LogicValue.NAN)
                return LogicValue.NAN;

            return inputs[0] == LogicValue.HIGH ? LogicValue.LOW : LogicValue.HIGH;
        }
    }
}
