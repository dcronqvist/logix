using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Circuits.Minimals
{
    class MinimalLamp : CircuitComponent
    {
        public MinimalLamp() : base(1, 0)
        {

        }

        protected override void PerformLogic()
        {
            
        }

        public LogicValue GetValue()
        {
            return Inputs[0].Value;
        }
    }
}
