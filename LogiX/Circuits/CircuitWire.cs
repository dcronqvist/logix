using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Circuits
{
    enum LogicValue
    {
        HIGH,
        LOW,
        NAN
    }

    class CircuitWire
    {
        public LogicValue Value { get; set; }

        public CircuitWire()
        {
            this.Value = LogicValue.NAN;
        }
    }
}
