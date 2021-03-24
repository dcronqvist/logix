using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Projects
{
    class WorkspaceComponentConnection
    {
        public int To { get; set; }
        public int OutIndex { get; set; }
        public int InIndex { get; set; }

        public WorkspaceComponentConnection(int to, int outIndex, int inIndex)
        {
            this.To = to;
            this.OutIndex = outIndex;
            this.InIndex = inIndex;
        }
    }
}
