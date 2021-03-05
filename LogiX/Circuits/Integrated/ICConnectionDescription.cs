using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Circuits.Integrated
{
    class ICConnectionDescription
    {
        public int To { get; set; }
        public int OutIndex { get; set; }
        public int InIndex { get; set; }

        public ICConnectionDescription()
        {

        }

        public ICConnectionDescription(int to, int outIndex, int inIndex)
        {
            this.To = to;
            this.OutIndex = outIndex;
            this.InIndex = inIndex;
        }
    }
}
