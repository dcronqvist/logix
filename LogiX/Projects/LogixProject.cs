using LogiX.Assets;
using LogiX.Circuits.Integrated;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
            this.IncludedCollections = new List<string>();
        }

        public void IncludeCollection(string collection)
        {
            this.IncludedCollections.Add(collection);
        }

        public void IncludeDescription(string description)
        {
            this.IncludedDescriptions.Add(description);
        }

        public List<ICCollection> GetAllCollections()
        {
            List<ICCollection> colls = new List<ICCollection>();

            foreach(string collection in IncludedCollections)
            {
                string fileName = Path.GetFileName(collection);

                colls.Add(GetCollection(fileName));
            }

            return colls;
        }

        public ICCollection GetCollection(string name)
        {
            foreach(string collection in IncludedCollections)
            {
                string fileName = Path.GetFileName(collection);

                if(fileName == name)
                {
                    using (StreamReader sr = new StreamReader(collection))
                    {
                        return JsonConvert.DeserializeObject<ICCollection>(sr.ReadToEnd());
                    }
                }    
            }
            return null;
        }

        public ICDescription GetDescription(string name)
        {
            foreach(string desc in IncludedDescriptions)
            {
                string fileName = Path.GetFileName(desc);

                if (fileName == name)
                {
                    using (StreamReader sr = new StreamReader(desc))
                    {
                        return JsonConvert.DeserializeObject<ICDescription>(sr.ReadToEnd());
                    }
                }
            }
            return null;
        }
    }
}
