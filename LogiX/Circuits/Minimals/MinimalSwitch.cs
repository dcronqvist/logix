using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Circuits.Minimals
{
    class MinimalSwitch : CircuitComponent
    {
        public MinimalSwitch() : base(0, 1)
        {

        }

        protected override void PerformLogic()
        {
            
        }

        public void SetValue(LogicValue logic)
        {
            Outputs[0].Value = logic;
        }
    }
}
