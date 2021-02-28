using LogiX.Circuits.Logic;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Circuits.Minimals
{
    class MinimalIC : CircuitComponent
    {
        IICLogic logic;

        public MinimalIC(int inputs, int outputs) : base(inputs, outputs)
        {

        }

        protected override void PerformLogic()
        {
            //LogicValue[] inputs = 
            //LogicValue[] outputs = logic.GetOutputs()
        }
    }
}
