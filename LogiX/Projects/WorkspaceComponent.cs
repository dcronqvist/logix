using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LogiX.Projects
{
    class WorkspaceComponent
    {
        public string Type { get; set; }
        public Vector2 Position { get; set; }
        public List<WorkspaceComponentConnection> ConnectedTo { get; set; }
        public string ID { get; set; }
        public string FileComponentFile { get; set; }

        public WorkspaceComponent(string type, Vector2 position, List<WorkspaceComponentConnection> connectedTo)
        {
            this.Type = type;
            this.Position = position;
            this.ConnectedTo = connectedTo;
        }

        public void SetID(string s)
        {
            this.ID = s;
        }

        public void SetFileComponentFile(string file)
        {
            this.FileComponentFile = file;
        }
    }
}
