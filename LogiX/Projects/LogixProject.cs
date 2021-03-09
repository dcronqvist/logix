using LogiX.Assets;
using LogiX.Circuits.Integrated;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Projects
{
    class LogixProject : Asset
    {
        public string ProjectName { get; set; }
        public List<string> IncludedCollections { get; set; }
        public List<string> IncludedDescriptions { get; set; }

        public LogixProject()
        {

        }

        public LogixProject(string name)
        {
            this.ProjectName = name;
        }

        public void IncludeCollection(string collection)
        {
            this.IncludedCollections.Add(collection);
        }

        public void IncludeDescription(string description)
        {
            this.IncludedDescriptions.Add(description);
        }
    }
}
