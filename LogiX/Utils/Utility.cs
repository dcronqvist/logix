using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Utils
{
    static class Utility
    {
        public static string ROAMING_DIR = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string LOGIX_DIR = ROAMING_DIR + @"/LogiX";

        public static string SETTINGS_FILE = LOGIX_DIR + @"/settings.json";

        public static string LOG_FILE = LOGIX_DIR + @"/log.txt";

        public static Color Opacity(this Color a, float f) 
        {
            return new Color(a.r, a.g, a.b, (byte)(a.a * f));
        }
    }
}
