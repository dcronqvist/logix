using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Circuits.Integrated
{
    class ICComponentDescription
    {
        public string Type { get; set; }
        public List<ICConnectionDescription> To { get; set; }
        public string ID { get; set; }

        public ICComponentDescription(string type, List<ICConnectionDescription> to)
        {
            this.Type = type;
            this.To = to;
        }

        public void SetID(string s)
        {
            this.ID = s;
        }
    }
}
