using LogiX.Assets;
using LogiX.Circuits.Drawables;
using LogiX.Circuits.Integrated;
using LogiX.Logging;
using LogiX.Simulation;
using LogiX.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace LogiX.Projects
{
    [JsonObject(MemberSerialization.OptOut)]
    class LogiXProject : Asset
    {
        public string ProjectName { get; set; }
        public List<string> IncludedCollections { get; set; }
        public List<string> IncludedDescriptions { get; set; }
        public WorkspaceContainer Container { get; set; }

        [JsonIgnore]
        public Simulator Simulation { get; set; }

        public LogiXProject()
        {

        }

        public LogiXProject(string name)
        {
            this.ProjectName = name;
            this.IncludedCollections = new List<string>();
            this.IncludedDescriptions = new List<string>();
            this.Simulation = new Simulator();
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
            List<ICDescription> nonColl = GetAllNonCollectionDescriptions();

            foreach(ICCollection coll in GetAllCollections())
            {
                nonColl.AddRange(coll.Descriptions);
            }

            return nonColl;
        }

        public List<ICDescription> GetAllNonCollectionDescriptions()
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

        public static LogiXProject LoadFromFile(string path)
        {
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    string json = sr.ReadToEnd();
                    LogiXProject lp = JsonConvert.DeserializeObject<LogiXProject>(json);
                    Tuple<List<DrawableComponent>, List<DrawableWire>> tup = lp.Container.GenerateDrawables(lp.GetAllDescriptions(), Vector2.Zero);
                    lp.Simulation = new Simulator();
                    lp.Simulation.AllComponents = tup.Item1;
                    lp.Simulation.AllWires = tup.Item2;
                    return lp;
                }
            }
            catch(Exception ex)
            {
                LogManager.AddEntry($"Failed to load project at {path}: {ex.Message}", LogEntryType.ERROR);
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

        public void SaveProjectToFile(string path)
        {
            this.Container = new WorkspaceContainer(Simulation.AllComponents);
            this.ProjectName = Path.GetFileNameWithoutExtension(path);

            using(StreamWriter sw = new StreamWriter(path))
            {
                sw.Write(JsonConvert.SerializeObject(this));
            }
        }
    }
}
