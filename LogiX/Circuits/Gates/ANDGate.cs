using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Circuits.Gates
{
    class ANDGate : CircuitComponent
    {
        // All gates will have 2 inputs and one output
        public ANDGate() : base(2, 1)
        {

        }

        protected override void PerformLogic()
        {
            // Both inputs should be on for its only output to be on.
            // However, if any of the inputs is NAN, then the output will be NAN
            if (AnyInputNAN())
            {
                Outputs[0].Value = LogicValue.NAN;
                // Do not continue if any input is NAN.
                return;
            }

            bool first = Inputs[0].Value == LogicValue.HIGH;
            bool second = Inputs[1].Value == LogicValue.HIGH;

            if(first && second)
            {
                Outputs[0].Value = LogicValue.HIGH;
            }
            else
            {
                Outputs[0].Value = LogicValue.LOW;
            }
        }
    }
}
