using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Utils
{
    class Utility
    {
        public static string ROAMING_DIR = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string LOGIX_DIR = ROAMING_DIR + @"\LogiX";

        public static string SETTINGS_FILE = LOGIX_DIR + @"\settings.json";

        public static string LOG_FILE = LOGIX_DIR + @"\log.txt";
    }
}
