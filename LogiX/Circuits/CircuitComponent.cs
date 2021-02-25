using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Circuits
{
    abstract class CircuitComponent
    {
        /*  Alright, so the logic here goes like this.
         *  Update all inputs, and have them retrieve their values from the wires
         *  that connect to them.
         *  Then perform the specific logic for this component.
         *  Make sure that outputs are being set in the logic, depending on some inputs.
         *  
         *  We do the above for all components in the world, then we do the next thing.
         *  
         *  Then we update outputs but setting all wires that are connected to them
         *  to their newly set values.
         */

        public List<CircuitInput> Inputs { get; set; }
        public List<CircuitOutput> Outputs { get; set; }

        public CircuitComponent(int inputs, int outputs)
        {
            this.Inputs = new List<CircuitInput>();
            this.Outputs = new List<CircuitOutput>();

            for (int i = 0; i < inputs; i++)
            {
                this.Inputs.Add(new CircuitInput());
            }

            for (int i = 0; i < outputs; i++)
            {
                this.Outputs.Add(new CircuitOutput());
            }
        }

        protected abstract void PerformLogic();

        private void UpdateInputs()
        {
            foreach(CircuitInput ci in Inputs)
            {
                ci.GetValueFromSignal();
            }
        }

        /// <summary>
        /// This should be run after UpdateInputsAndLogic, on all components.
        /// </summary>
        public void UpdateOutputs()
        {
            foreach(CircuitOutput co in Outputs)
            {
                co.SetSignals();
            }
        }

        /// <summary>
        /// This should be run before UpdateOutputs, on all components.
        /// </summary>
        public void UpdateInputsAndLogic()
        {
            UpdateInputs();
            PerformLogic();
        }
    }
}
