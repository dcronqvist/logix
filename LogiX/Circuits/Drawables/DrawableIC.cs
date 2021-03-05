using LogiX.Circuits.Integrated;
using LogiX.Circuits.Minimals;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LogiX.Circuits.Drawables
{
    class DrawableIC : DrawableComponent
    {
        public ICDescription Description { get; set; }
        public List<CircuitComponent> Components { get; set; }
        public List<MinimalSwitch> ICInputs { get; set; }
        public List<MinimalLamp> ICOutputs { get; set; }

        public DrawableIC(Vector2 position, string text, ICDescription desc) : base(position, text, desc.Inputs, desc.Outputs)
        {
            this.Description = desc;
            this.Components = desc.GenerateComponents();
            //this.ICInputs = this.Components
        }

        protected override void PerformLogic()
        {
            for (int i = 0; i < ICInputs.Count; i++)
            {
                ICInputs[i].SetValue(Inputs[i].Value);
            }

            foreach(CircuitComponent cc in Components)
            {
                cc.UpdateInputsAndLogic();
            }

            foreach(CircuitComponent cc in Components)
            {
                cc.UpdateOutputs();
            }

            for (int i = 0; i < ICOutputs.Count; i++)
            {
                Outputs[i].Value = ICOutputs[i].GetValue();
            }
        }
    }
}
