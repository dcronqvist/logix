using LogiX.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using System.IO;

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
                LoadSettingsFile();
            }
            else
            {
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
                Dictionary<string, string> s = GenerateDefaultSettings();

                using(StreamWriter wr = new StreamWriter(Utility.SETTINGS_FILE))
                {
                    string jsonString = JsonConvert.SerializeObject(s, Formatting.Indented);
                    wr.Write(jsonString);
                }

                // Successfully created default settings file
                return true;
            }
            catch(Exception ex)
            {
                // Failed to create default settings file.
                return false;
            }
        }

        public static bool CreateLogiXDirectory()
        {
            Directory.CreateDirectory(Utility.LOGIX_DIR);
            return true;
        }

        public static bool LoadSettingsFile()
        {
            try
            {
                using(StreamReader sr = new StreamReader(Utility.SETTINGS_FILE))
                {
                    settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(sr.ReadToEnd());
                }

                // Could load settings successfully.
                return true;
            }
            catch(Exception ex)
            {
                // Failed to load settings.
                return false;
            }
        }
    }
}
