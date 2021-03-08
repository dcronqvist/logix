using LogiX.Circuits.Logic;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using LogiX.Utils;

namespace LogiX.Circuits.Minimals
{
    class MinimalLogicGate : CircuitComponent
    {
        private IGateLogic logic;

        public MinimalLogicGate(IGateLogic logic) : base(logic.GetExpectedInputAmount(), 1)
        {
            this.logic = logic;
        }

        protected override void PerformLogic()
        {
            LogicValue[] inputs = Utility.GetLogicValues(Inputs);
            this.Outputs[0].Value = logic.GetOutput(inputs);
        }
    }
}
