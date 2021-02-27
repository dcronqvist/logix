using LogiX.Circuits.GateLogic;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Circuits.Minimals
{
    class MinimalLogicGate : CircuitComponent
    {
        private IGateLogic logic;

        public MinimalLogicGate(IGateLogic logic) : base(2, 1)
        {
            this.logic = logic;
        }

        protected override void PerformLogic()
        {
            this.Outputs[0].Value = logic.GetGateOutput(Inputs[0].Value, Inputs[1].Value);
        }
    }
}
