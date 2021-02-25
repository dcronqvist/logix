using LogiX.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using LogiX.Logging;

namespace LogiX.Settings
{
    static class SettingManager
    {
        private static Dictionary<string, string> settings;

        public static bool SettingsFileExists()
        {
            return File.Exists(Utility.SETTINGS_FILE);
        }

        public static bool LogiXDirectoryExists()
        {
            return Directory.Exists(Utility.LOGIX_DIR);
        }

        public static void LoadSettings()
        {
            if (SettingsFileExists() && LogiXDirectoryExists())
            {
                LogManager.AddEntry("LogiX Settings found.");
                LoadSettingsFile();
            }
            else
            {
                LogManager.AddEntry("Could not find settings file or LogiX directory.", LogEntryType.WARNING);
                CreateLogiXDirectory();
                CreateDefaultSettingsFile();
                LoadSettings();
            }
        }

        public static Dictionary<string, string> GenerateDefaultSettings()
        {
            return new Dictionary<string, string>()
            {
                { "window-width", "1280" },
                { "window-height", "720" }
            };
        }
        
        public static bool CreateDefaultSettingsFile()
        {
            try
            {
                LogManager.AddEntry("Creating default settings file.");
                Dictionary<string, string> s = GenerateDefaultSettings();

                using(StreamWriter wr = new StreamWriter(Utility.SETTINGS_FILE))
                {
                    string jsonString = JsonConvert.SerializeObject(s, Formatting.Indented);
                    wr.Write(jsonString);
                }

                // Successfully created default settings file
                LogManager.AddEntry("Successfully created default settings file.");
                return true;
            }
            catch(Exception ex)
            {
                // Failed to create default settings file.
                LogManager.AddEntry("Failed to create default settings file.", LogEntryType.ERROR);
                return false;
            }
        }

        public static bool CreateLogiXDirectory()
        {
            LogManager.AddEntry("Creating LogiX directory in AppData/Roaming.");
            Directory.CreateDirectory(Utility.LOGIX_DIR);
            return true;
        }

        public static bool LoadSettingsFile()
        {
            try
            {
                LogManager.AddEntry("Loading settings...");
                using(StreamReader sr = new StreamReader(Utility.SETTINGS_FILE))
                {
                    settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(sr.ReadToEnd());
                }

                // Could load settings successfully.
                LogManager.AddEntry("Successfully loaded settings!");
                return true;
            }
            catch(Exception ex)
            {
                // Failed to load settings.
                LogManager.AddEntry("Failed to load settings!", LogEntryType.ERROR);
                return false;
            }
        }

        public static string GetSetting(string key)
        {
            return settings[key];
        }

        public static void SetSetting(string key, string value)
        {
            settings[key] = value;
            LogManager.AddEntry($"Setting {key} now has value {value}");
        }

        public static void SaveSettings()
        {
            using(StreamWriter sw = new StreamWriter(Utility.SETTINGS_FILE))
            {
                string jsonString = JsonConvert.SerializeObject(settings, Formatting.Indented);
                sw.Write(jsonString);
                LogManager.AddEntry("Settings saved!");
            }
        }
    }
}
