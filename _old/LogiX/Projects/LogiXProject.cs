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
        public List<string> IncludedFileComponents { get; set; }
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
            this.IncludedFileComponents = new List<string>();
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

        public void IncludeFileComponent(string fileComponent)
        {
            if(!this.IncludedFileComponents.Contains(fileComponent))
                this.IncludedFileComponents.Add(fileComponent);
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
            List<string> notFound = new List<string>();

            foreach(string desc in IncludedDescriptions)
            {
                string fileName = Path.GetFileName(desc);

                if(File.Exists(desc))
                {
                    descriptions.Add(GetDescription(fileName));
                }
                else
                {
                    notFound.Add(desc);
                    LogManager.AddEntry($"Could not find description {desc}");
                }
            }

            return descriptions;
        }

        public List<ICCollection> GetAllCollections()
        {
            // Initialize list for returning
            List<ICCollection> colls = new List<ICCollection>();
            // Initialize list for not found collections for removal.
            List<string> couldNotFind = new List<string>();

            // Loop through all included collections
            foreach(string collection in IncludedCollections)
            {
                // Get the filename, without extension, from the collection
                string fileName = Path.GetFileName(collection);

                // If the collection file exists, load it and add it to the list of collections
                if (File.Exists(collection))
                {
                    colls.Add(GetCollection(fileName));
                }
                else // If it does not exist, tell error to log and add collection to not found.
                {
                    couldNotFind.Add(collection);
                    LogManager.AddEntry($"Could not find collection {collection}");
                }
            }

            // Loop through all non found collections and remove them from included collections.
            foreach (string notFound in couldNotFind)
            {
                IncludedCollections.Remove(notFound);
            }

            // Return the collection list
            return colls;
        }

        public ICCollection GetCollection(string name)
        {
            foreach(string collection in IncludedCollections)
            {
                string fileName = Path.GetFileName(collection);

                if(fileName == name)
                {
                    try
                    {
                        using (StreamReader sr = new StreamReader(collection))
                        {
                            ICCollection co = JsonConvert.DeserializeObject<ICCollection>(sr.ReadToEnd());
                            LogManager.AddEntry($"Successfully loaded collection {name}");
                            return co;
                        }
                    }
                    catch(Exception ex)
                    {
                        LogManager.AddEntry($"Failed to load collection {collection}: {ex.Message} - {ex.StackTrace}");
                    }
                }    
            }
            return null;
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
                    try
                    {
                        using (StreamReader sr = new StreamReader(desc))
                        {
                            ICDescription icd = JsonConvert.DeserializeObject<ICDescription>(sr.ReadToEnd());
                            LogManager.AddEntry($"Successfully loaded description {name}");
                            return icd; 
                        }
                    }
                    catch(Exception ex)
                    {
                        LogManager.AddEntry($"Failed to load description {name}: {ex.Message} - {ex.StackTrace}");
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
