using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LogiX.Utils
{
    static class Utility
    {
        // Files and directories
        public static string ROAMING_DIR = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string LOGIX_DIR = ROAMING_DIR + @"/LogiX";
        public static string ASSETS_DIR = LOGIX_DIR + @"/assets";
        public static string SETTINGS_FILE = LOGIX_DIR + @"/settings.json";
        public static string LOG_FILE = LOGIX_DIR + @"/log.txt";

        // Colors
        public static Color COLOR_ON = Color.BLUE;
        public static Color COLOR_OFF = Color.WHITE;
        public static Color COLOR_NAN = Color.RED;
        public static Color COLOR_BLOCK_DEFAULT = Color.WHITE;

        public static Color Opacity(this Color a, float f) 
        {
            return new Color(a.r, a.g, a.b, (byte)(a.a * f));
        }

        public static System.Drawing.PointF ToPoint(this Vector2 vec)
        {
            return new System.Drawing.PointF(vec.X, vec.Y);
        }
    }
}
