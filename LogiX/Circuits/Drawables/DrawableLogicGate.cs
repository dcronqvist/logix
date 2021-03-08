using LogiX.Circuits.Logic;
using LogiX.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LogiX.Circuits.Drawables
{
    class DrawableLogicGate : DrawableComponent
    {
        private IGateLogic logic;

        public DrawableLogicGate(Vector2 position, string text, IGateLogic logic) : base(position, text, logic.GetExpectedInputAmount(), 1)
        {
            this.logic = logic;
            CalculateOffsets();
        }

        protected override void PerformLogic()
        {
            this.Outputs[0].Value = logic.GetOutput(Utility.GetLogicValues(Inputs));
        }

        public string GetLogicName()
        {
            return logic.GetType().Name;
        }
    }
}
