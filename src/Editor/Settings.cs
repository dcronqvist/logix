namespace LogiX.Editor;

public enum SettingType
{
    None,
    Editor
}

public class Setting
{
    public object Value { get; set; }
    public bool VisibleInSettingsEditor { get; set; }
    public SettingType Type { get; set; }

    public Setting(object value, bool visibleInSettings, SettingType type = SettingType.None)
    {
        this.Value = value;
        this.VisibleInSettingsEditor = visibleInSettings;
        this.Type = type;
    }

    public T GetValue<T>()
    {
        return (T)this.Value;
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
            { "latestProject", new Setting("", false) }
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
                        if (kvp.Value.ValueKind == JsonValueKind.String)
                        {
                            defaultSettings[kvp.Key] = new Setting(kvp.Value.ToString(), defaultSettings[kvp.Key].VisibleInSettingsEditor, defaultSettings[kvp.Key].Type);
                        }
                        else if (kvp.Value.ValueKind == JsonValueKind.Number)
                        {
                            defaultSettings[kvp.Key] = new Setting(int.Parse(kvp.Value.ToString()), defaultSettings[kvp.Key].VisibleInSettingsEditor, defaultSettings[kvp.Key].Type);
                        }
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
        settings[setting] = new Setting(value, settings[setting].VisibleInSettingsEditor, settings[setting].Type);
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

            string json = JsonSerializer.Serialize(sets, new JsonSerializerOptions() { WriteIndented = true });
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