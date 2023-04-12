using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using LogiX.Architecture;

namespace LogiX;

public class Setting
{
    [JsonIgnore]
    public string Name { get; set; }
    public object Value { get; set; }

    public T GetValue<T>()
    {
        return (T)Value;
    }
}

public static class Settings
{
    // SETTING CONSTANTS
    public const string LAST_OPEN_PROJECT = "lastOpenProject";
    public const string RECENT_OPEN_PROJECTS = "recentOpenProjects";
    public const string UI_SCALE = "uiScale";
    public const string WINDOW_SIZE = "windowSize";
    public const string WINDOW_FULLSCREEN = "windowFullscreen";
    public const string SHOW_URL_WARNING = "showUrlWarning";

    private static string _settingsFileLocation = "./";
    private static string _settingsFile = "settings.json";
    private static JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        IncludeFields = true,
        AllowTrailingCommas = true,
        WriteIndented = true
    };
    private static Dictionary<string, Setting> _settings = new Dictionary<string, Setting>();

    private static List<Setting> GetDefaultSettings()
    {
        return new List<Setting>() {
            new Setting() { Name = LAST_OPEN_PROJECT, Value = "" },
            new Setting() { Name = RECENT_OPEN_PROJECTS, Value = new List<string>() },
            new Setting() { Name = UI_SCALE, Value = "Medium" },
            new Setting() { Name = WINDOW_SIZE, Value = new Vector2i(1280, 720) },
            new Setting() { Name = WINDOW_FULLSCREEN, Value = false },
            new Setting() { Name = SHOW_URL_WARNING, Value = true }
        };
    }

    public static string GetSettingsFilePath()
    {
        return _settingsFileLocation + _settingsFile;
    }

    private static void CreateSettingsFileWithDefaults()
    {
        List<Setting> settings = GetDefaultSettings();

        Dictionary<string, object> settingsDict = new Dictionary<string, object>();

        foreach (Setting setting in settings)
        {
            settingsDict.Add(setting.Name, setting.Value);
        }

        SaveSettings(settingsDict);
    }

    private static async Task CreateSettingsFileWithDefaultsAsync()
    {
        List<Setting> settings = GetDefaultSettings();

        Dictionary<string, object> settingsDict = new Dictionary<string, object>();

        foreach (Setting setting in settings)
        {
            settingsDict.Add(setting.Name, setting.Value);
        }

        await SaveSettingsAsync(settingsDict);
    }

    public static void LoadSettings()
    {
        if (!File.Exists(GetSettingsFilePath()))
        {
            _ = CreateSettingsFileWithDefaultsAsync();
        }

        List<Setting> defaultSettings = GetDefaultSettings();

        using (StreamReader sr = new StreamReader(GetSettingsFilePath()))
        {
            string json = sr.ReadToEnd();
            Dictionary<string, JsonElement> settings = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, _jsonSerializerOptions);

            foreach (KeyValuePair<string, JsonElement> kvp in settings)
            {
                if (defaultSettings.Find(x => x.Name == kvp.Key) != null)
                {
                    defaultSettings.Find(x => x.Name == kvp.Key).Value = kvp.Value.Deserialize(defaultSettings.Find(x => x.Name == kvp.Key).Value.GetType(), _jsonSerializerOptions);
                }
            }
        }

        foreach (Setting setting in defaultSettings)
        {
            _settings.Add(setting.Name, setting);
        }
    }

    public static async Task LoadSettingsAsync()
    {
        if (!File.Exists(GetSettingsFilePath()))
        {
            await CreateSettingsFileWithDefaultsAsync();
        }

        List<Setting> defaultSettings = GetDefaultSettings();

        using (StreamReader sr = new StreamReader(GetSettingsFilePath()))
        {
            string json = await sr.ReadToEndAsync();
            Dictionary<string, JsonElement> settings = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, _jsonSerializerOptions);

            foreach (KeyValuePair<string, JsonElement> kvp in settings)
            {
                if (defaultSettings.Find(x => x.Name == kvp.Key) != null)
                {
                    defaultSettings.Find(x => x.Name == kvp.Key).Value = kvp.Value.Deserialize(defaultSettings.Find(x => x.Name == kvp.Key).Value.GetType(), _jsonSerializerOptions);
                }
            }
        }

        foreach (Setting setting in defaultSettings)
        {
            _settings.Add(setting.Name, setting);
        }
    }

    public static void SaveSettings(Dictionary<string, object> settings)
    {
        using (StreamWriter sw = new StreamWriter(GetSettingsFilePath()))
        {
            string json = JsonSerializer.Serialize(settings, _jsonSerializerOptions);
            sw.Write(json);
        }
    }

    public static void SaveSettings(Dictionary<string, Setting> settings)
    {
        Dictionary<string, object> settingsDict = new Dictionary<string, object>();
        foreach (KeyValuePair<string, Setting> kvp in settings)
        {
            settingsDict.Add(kvp.Key, kvp.Value.Value);
        }

        SaveSettings(settingsDict);
    }

    public static async Task SaveSettingsAsync(Dictionary<string, object> settings)
    {
        string json = JsonSerializer.Serialize(settings, _jsonSerializerOptions);

        using (StreamWriter sw = new StreamWriter(GetSettingsFilePath()))
        {
            await sw.WriteAsync(json);
        }
    }

    public static async Task SaveSettingsAsync(Dictionary<string, Setting> settings)
    {
        Dictionary<string, object> settingsDict = new Dictionary<string, object>();
        foreach (KeyValuePair<string, Setting> kvp in settings)
        {
            settingsDict.Add(kvp.Key, kvp.Value.Value);
        }

        await SaveSettingsAsync(settingsDict);
    }

    public static T GetSetting<T>(string name)
    {
        return _settings[name].GetValue<T>();
    }

    public static async Task SetSettingAsync(string name, object value)
    {
        _settings[name].Value = value;
        await SaveSettingsAsync(_settings);
    }

    public static void SetSetting(string name, object value)
    {
        _settings[name].Value = value;
        SaveSettings(_settings);
    }
}