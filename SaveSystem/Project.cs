using LogiX.Components;

namespace LogiX.SaveSystem;

public class Project
{
    [JsonProperty(PropertyName = "currentWorkspace")]
    public CircuitDescription CurrentWorkspace { get; set; }

    [JsonProperty(PropertyName = "includedICFiles")]
    public List<string> IncludedICFiles { get; set; }

    [JsonProperty(PropertyName = "projectCreatedICs")]
    public List<ICDescription> ProjectCreatedICs { get; set; }

    [JsonProperty(PropertyName = "includedICCollectionFiles")]
    public List<string> IncludedICCollectionFiles { get; set; }

    [JsonIgnore]
    public string LoadedFromFile { get; set; }

    [JsonIgnore]
    public List<ICDescription> ICsFromFile { get; set; }

    [JsonIgnore]
    public Dictionary<string, ICCollection> ICCollections { get; set; }

    [JsonIgnore]
    public Dictionary<ICDescription, string> ICFromFileToFilePath { get; set; }

    [JsonIgnore]
    public Dictionary<ICCollection, string> ICCollectionToFilePath { get; set; }

    [JsonIgnore]
    public const string EXTENSION = ".lgxpr";

    public Project()
    {
        this.CurrentWorkspace = new CircuitDescription();
        this.IncludedICFiles = new List<string>();
        this.ProjectCreatedICs = new List<ICDescription>();
        this.ICsFromFile = new List<ICDescription>();
        this.ICFromFileToFilePath = new Dictionary<ICDescription, string>();
        this.IncludedICCollectionFiles = new List<string>();
        this.ICCollections = new Dictionary<string, ICCollection>();
        this.ICCollectionToFilePath = new Dictionary<ICCollection, string>();
    }

    [JsonConstructor]
    public Project(CircuitDescription workspace, string name)
    {
        this.CurrentWorkspace = workspace;
        this.IncludedICFiles = new List<string>();
        this.ProjectCreatedICs = new List<ICDescription>();
        this.ICsFromFile = new List<ICDescription>();
        this.ICFromFileToFilePath = new Dictionary<ICDescription, string>();
        this.IncludedICCollectionFiles = new List<string>();
        this.ICCollections = new Dictionary<string, ICCollection>();
        this.ICCollectionToFilePath = new Dictionary<ICCollection, string>();
    }

    public string GetFileName()
    {
        if (this.LoadedFromFile == null || this.LoadedFromFile == "")
        {
            return "new project";
        }

        return Path.GetFileName(this.LoadedFromFile);
    }

    public bool IncludeICFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return false;
        }

        this.IncludedICFiles.Add(filePath);

        this.ReloadProjectICs();

        return true;
    }

    public void ExcludeICFromFile(ICDescription icd)
    {
        this.IncludedICFiles.Remove(this.ICFromFileToFilePath[icd]);
        this.ReloadProjectICs();
    }

    public void AddProjectCreatedIC(ICDescription icd)
    {
        this.ProjectCreatedICs.Add(icd);
        this.ReloadProjectICs();
    }

    public void RemoveProjectCreatedIC(ICDescription icd)
    {
        this.ProjectCreatedICs.Remove(icd);
        this.ReloadProjectICs();
    }

    public void IncludeICCollectionFile(string file)
    {
        this.IncludedICCollectionFiles.Add(file);
        this.ReloadProjectICs();
    }

    public void ExcludeICCollection(ICCollection coll)
    {
        this.ICCollections.Remove(coll.Name);
        this.IncludedICCollectionFiles.Remove(this.ICCollectionToFilePath[coll]);
        this.ReloadProjectICs();
    }

    public void ReloadProjectICs()
    {
        this.ICsFromFile.Clear();
        this.ICFromFileToFilePath.Clear();
        this.ICCollectionToFilePath.Clear();
        this.ICCollections.Clear();

        foreach (string includedIcfile in this.IncludedICFiles)
        {
            if (File.Exists(includedIcfile))
            {
                using (StreamReader sr = new StreamReader(includedIcfile))
                {
                    ICDescription icd = JsonConvert.DeserializeObject<ICDescription>(sr.ReadToEnd(), new JsonSerializerSettings()
                    {
                        Converters = new List<JsonConverter>() { new ComponentConverter() },
                        ObjectCreationHandling = ObjectCreationHandling.Replace
                    });

                    this.ICsFromFile.Add(icd);
                    this.ICFromFileToFilePath.Add(icd, includedIcfile);
                }
            }
        }

        foreach (string includedICCollectionFile in this.IncludedICCollectionFiles)
        {
            if (File.Exists(includedICCollectionFile))
            {
                using (StreamReader sr = new StreamReader(includedICCollectionFile))
                {
                    ICCollection icc = JsonConvert.DeserializeObject<ICCollection>(sr.ReadToEnd(), new JsonSerializerSettings()
                    {
                        Converters = new List<JsonConverter>() { new ComponentConverter() },
                        ObjectCreationHandling = ObjectCreationHandling.Replace
                    });

                    this.ICCollections.Add(icc.Name, icc);
                    this.ICCollectionToFilePath.Add(icc, includedICCollectionFile);
                }
            }
        }
    }

    public static Project LoadFromFile(string file)
    {
        using (StreamReader sr = new StreamReader(file))
        {
            Project p = JsonConvert.DeserializeObject<Project>(sr.ReadToEnd(), new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>() { new ComponentConverter() },
                ObjectCreationHandling = ObjectCreationHandling.Replace
            });
            p.ReloadProjectICs();
            p.LoadedFromFile = file;
            return p;
        }
    }

    public void SaveComponentsInWorkspace(List<Component> components)
    {
        this.CurrentWorkspace = new CircuitDescription(components);
    }

    public bool HasFile()
    {
        return this.LoadedFromFile != null && File.Exists(this.LoadedFromFile);
    }

    public void SaveToFile(string path)
    {
        string finalPath = path.Contains(EXTENSION) ? path : path + EXTENSION;
        this.LoadedFromFile = finalPath;
        using (StreamWriter sw = new StreamWriter(finalPath))
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            sw.Write(json);
        }
    }

    public Tuple<List<Component>, List<Wire>> GetComponentsAndWires()
    {
        return this.CurrentWorkspace.CreateComponentsAndWires(Vector2.Zero, true);
    }
}