using LogiX.Circuits.GateLogic;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LogiX.Circuits.Drawables
{
    class DrawableLogicGate : DrawableComponent
    {
        private IGateLogic logic;

        public DrawableLogicGate(Vector2 position, string text, IGateLogic logic) : base(position, text, 2, 1)
        {
            this.logic = logic;
        }

        protected override void PerformLogic()
        {
            this.Outputs[0].Value = logic.GetGateOutput(Inputs[0].Value, Inputs[1].Value);
        }
    }
}
