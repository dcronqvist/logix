﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Circuits.Logic
{
    class ORGateLogic : IGateLogic
    {
        public LogicValue GetOutput(LogicValue a, LogicValue b)
        {
            if(a == LogicValue.NAN || b == LogicValue.NAN)
            {
                return LogicValue.NAN;
            }

            if(a == LogicValue.HIGH || b == LogicValue.HIGH)
            {
                return LogicValue.HIGH;
            }
            else
            {
                return LogicValue.LOW;
            }
        }
    }
}
