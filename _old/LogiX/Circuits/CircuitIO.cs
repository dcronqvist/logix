using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Circuits
{
    class CircuitIO
    {
        public LogicValue Value { get; set; }

        public CircuitIO()
        {
            this.Value = LogicValue.NAN;
        }
    }
}
