using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Circuits.Logic
{
    interface IGateLogic
    {
        public int GetExpectedInputAmount();

        public LogicValue GetOutput(LogicValue[] inputs);
    }
}
