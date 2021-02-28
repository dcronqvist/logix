using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Circuits.Integrated
{
    class ICDescription
    {
        public ICDescription(List<CircuitComponent> components)
        {
            //this.Descriptions = GenerateDescription(components);
            //this.Inputs = CountComponentsOfType<CircuitSwitch>(components);
            //this.Outputs = CountComponentsOfType<CircuitLamp>(components);
        }

        public int CountComponentsOfType<T>(IEnumerable<CircuitComponent> components) where T : CircuitComponent
        {
            int count = 0;
            foreach (CircuitComponent cc in components)
            {
                if (cc.GetType() == typeof(T))
                    count++;
            }
            return count;
        }
    }
}
