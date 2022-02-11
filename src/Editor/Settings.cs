namespace LogiX.Editor;

public enum SettingType
{
    None,
    Editor
}

public class Setting
{
    [JsonIgnore]
    public object _value;
    public object Value
    {
        get { return _value; }
        set
        {
            _value = value;
            if (this.OnChange != null)
                this.OnChange(this);
        }
    }
    public bool VisibleInSettingsEditor { get; set; }
    public SettingType Type { get; set; }
    public string Name { get; set; }
    public Action<Setting>? OnChange { get; set; }

    public Setting(object value, bool visibleInSettings, string name = "", SettingType type = SettingType.None, Action<Setting> onChange = null)
    {
        this.Value = value;
        this.VisibleInSettingsEditor = visibleInSettings;
        this.Type = type;
        this.Name = name;
        this.OnChange = onChange;
    }

    public T GetValue<T>()
    {
        return (T)this.Value;
    }

    public void SetValue(object value)
    {
        this._value = value;
    }
}

public static class Settings
{

    private static Dictionary<string, Setting> settings;
    private static string settingsFile;

    static Settings()
    {
        settings = new Dictionary<string, Setting>();
    }

    private static Dictionary<string, Setting> GetDefaultSettings()
    {
        return new Dictionary<string, Setting>() {
            { "windowWidth", new Setting(1280, false) },
            { "windowHeight", new Setting(720, false) },
            { "latestProject", new Setting("", false) },
            { "preferredFramerate", new Setting(144, true, "Preferred FPS", SettingType.Editor, (setting) => { Raylib.SetTargetFPS(setting.GetValue<int>()); }) },
            { "editorBackgroundColor", new Setting(Color.LIGHTGRAY, true, "Editor Background Color", SettingType.Editor) },
        };
    }

    public static void LoadSettings()
    {
        settingsFile = $"{Util.EnvironmentPath}/config.json";

        Dictionary<string, Setting> defaultSettings = GetDefaultSettings();

        if (File.Exists(settingsFile))
        {
            using (StreamReader sr = new StreamReader(settingsFile))
            {
                Dictionary<string, JsonElement> sets = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(sr.ReadToEnd());
                foreach (KeyValuePair<string, JsonElement> kvp in sets)
                {
                    if (defaultSettings.ContainsKey(kvp.Key))
                    {
                        defaultSettings[kvp.Key].SetValue(kvp.Value.Deserialize(defaultSettings[kvp.Key].Value.GetType(), new JsonSerializerOptions() { IncludeFields = true }));
                    }
                }

                settings = defaultSettings;
            }
        }
        else
        {
            settings = defaultSettings;
            SaveSettings();
        }
    }

    public static Dictionary<string, Setting> GetAllSettings()
    {
        return settings;
    }

    public static void SetSetting<T>(string setting, T value)
    {
        settings[setting].Value = value;
    }

    public static void SaveSettings()
    {
        Directory.CreateDirectory(Util.EnvironmentPath);

        using (StreamWriter sw = new StreamWriter(settingsFile))
        {
            Dictionary<string, Object> sets = new Dictionary<string, object>();

            foreach (KeyValuePair<string, Setting> setting in settings)
            {
                sets.Add(setting.Key, setting.Value.Value);
            }


            string json = JsonSerializer.Serialize(sets, new JsonSerializerOptions() { WriteIndented = true, IncludeFields = true });
            sw.Write(json);
        }
    }

    public static Setting GetSetting(string setting)
    {
        return settings[setting];
    }

    public static T GetSettingValue<T>(string setting)
    {
        return settings[setting].GetValue<T>();
    }
}