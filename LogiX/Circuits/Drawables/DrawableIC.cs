using LogiX.Circuits.Integrated;
using LogiX.Circuits.Minimals;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace LogiX.Circuits.Drawables
{
    class DrawableIC : DrawableComponent
    {
        public ICDescription Description { get; set; }
        public List<CircuitComponent> Components { get; set; }
        public Dictionary<string, MinimalSwitch> InputDictionary { get; set; }
        public Dictionary<string, MinimalLamp> OutputDictionary { get; set; }
        public string[] InputIDS { get; set; }
        public string[] OutputIDS { get; set; }

        public DrawableIC(Vector2 position, string text, ICDescription desc) : base(position, text, desc.Inputs, desc.Outputs)
        {
            this.Description = desc;
            this.Components = desc.GenerateComponents();
            this.InputDictionary = GetInputDictionary(this.Components);
            this.OutputDictionary = GetOutputDictionary(this.Components);
            this.InputIDS = GetInputIDS(this.InputDictionary);
            this.OutputIDS = GetOutputIDS(this.OutputDictionary);
            CalculateOffsets();
        }

        private Dictionary<string, MinimalSwitch> GetInputDictionary(List<CircuitComponent> comps)
        {
            Dictionary<string, MinimalSwitch> cis = new Dictionary<string, MinimalSwitch>();
            comps.FindAll(x => x.GetType() == typeof(MinimalSwitch)).ForEach(ci =>
            {
                cis.Add(((MinimalSwitch)ci).ID, (MinimalSwitch)ci);
            });
            return cis;
        }
        private Dictionary<string, MinimalLamp> GetOutputDictionary(List<CircuitComponent> comps)
        {
            Dictionary<string, MinimalLamp> cis = new Dictionary<string, MinimalLamp>();
            comps.FindAll(x => x.GetType() == typeof(MinimalLamp)).ForEach(ci =>
            {
                cis.Add(((MinimalLamp)ci).ID, (MinimalLamp)ci);
            });
            return cis;
        }
        private string[] GetInputIDS(Dictionary<string, MinimalSwitch> inputs)
        {
            string[] arr = new string[inputs.Count];

            int index = 0;
            foreach (KeyValuePair<string, MinimalSwitch> kvp in inputs)
            {
                arr[index] = kvp.Key;
                index++;
            }

            return arr;
        }
        private string[] GetOutputIDS(Dictionary<string, MinimalLamp> outputs)
        {
            string[] arr = new string[outputs.Count];

            int index = 0;
            foreach (KeyValuePair<string, MinimalLamp> kvp in outputs)
            {
                arr[index] = kvp.Key;
                index++;
            }

            return arr;
        }

        public override string GetInputID(int index)
        {
            return InputIDS[index];
        }
        public override string GetOutputID(int index)
        {
            return OutputIDS[index];
        }
        protected override void PerformLogic()
        {
            for (int i = 0; i < this.InputIDS.Length; i++)
            {
                string id = this.InputIDS[i];
                MinimalSwitch ms = this.InputDictionary[id];

                ms.SetValue(base.Inputs[i].Value);
            }

            foreach(CircuitComponent cc in Components)
            {
                cc.UpdateInputsAndLogic();
            }

            foreach(CircuitComponent cc in Components)
            {
                cc.UpdateOutputs();
            }

            for (int i = 0; i < this.OutputIDS.Length; i++)
            {
                string id = this.OutputIDS[i];
                MinimalLamp ml = this.OutputDictionary[id];

                base.Outputs[i].Value = ml.GetValue();
            }
        }
    }
}
