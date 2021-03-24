using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Circuits.Logic
{
    interface IICLogic
    {
        public LogicValue[] GetOutputs(LogicValue[] inputs, List<CircuitComponent> components);
    }
}
