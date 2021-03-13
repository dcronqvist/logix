using LogiX.Assets;
using LogiX.Circuits.Integrated;
using LogiX.Logging;
using LogiX.Utils;
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
            this.IncludedDescriptions = new List<string>();
        }

        public void IncludeCollection(string collection)
        {
            if(!this.IncludedCollections.Contains(collection))
            {
                this.IncludedCollections.Add(collection);
            }
        }

        public void IncludeDescription(string description)
        {
            this.IncludedDescriptions.Add(description);
        }

        public List<ICDescription> GetAllDescriptions()
        {
            List<ICDescription> descriptions = new List<ICDescription>();

            foreach(string desc in IncludedDescriptions)
            {
                string fileName = Path.GetFileName(desc);

                descriptions.Add(GetDescription(fileName));
            }

            return descriptions;
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
            try
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
            catch(Exception ex)
            {
                LogManager.AddEntry($"Failed to load collection '{name}': {ex.Message}");
                return null;
            }
        }

        public static LogixProject LoadFromFile(string path)
        {
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    return JsonConvert.DeserializeObject<LogixProject>(sr.ReadToEnd());
                }
            }
            catch(Exception ex)
            {
                LogManager.AddEntry($"Failed to load project at {path}", LogEntryType.ERROR);
                return null;
            }
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

        public string SaveProjectToFile()
        {
            using(StreamWriter sw = new StreamWriter(Utility.CreateProjectFilePath(ProjectName)))
            {
                sw.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
            }
            return Utility.CreateProjectFilePath(ProjectName);
        }
    }
}
