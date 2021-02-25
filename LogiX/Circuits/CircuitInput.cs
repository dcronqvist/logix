using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Circuits
{
    class CircuitInput : CircuitIO
    {
        public CircuitWire Signal { get; set; }

        public CircuitInput()
        {
            this.Signal = null;
        }

        public void SetSignal(CircuitWire wire)
        {
            this.Signal = wire;
        }

        public void GetValueFromSignal()
        {
            if (this.Signal != null)
                Value = Signal.Value;
            else
                Value = LogicValue.NAN;
        }
    }
}
