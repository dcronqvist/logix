using ImGuiNET;
using LogiX.Circuits;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        public static void OpenPath(string path)
        {
            ProcessStartInfo psi = new ProcessStartInfo() { FileName = path, UseShellExecute = true };
            Process.Start(psi);
        }

        // File extensions
        public static string EXT_PROJ = ".lgxproj";
        public static string EXT_IC = ".lgxic";
        public static string CreateICFilePath(string icName)
        {
            return ASSETS_DIR + @"/" + icName + EXT_IC;
        }

        // Colors
        public static Color COLOR_ON = Color.BLUE;
        public static Color COLOR_OFF = Color.WHITE;
        public static Color COLOR_NAN = Color.RED;
        public static Color COLOR_BLOCK_DEFAULT = Color.WHITE;
        public static Color COLOR_BLOCK_BORDER_DEFAULT = Color.BLACK;
        public static Color COLOR_IO_HOVER_DEFAULT = Color.ORANGE;
        public static Color Opacity(this Color a, float f) 
        {
            return new Color(a.r, a.g, a.b, (byte)(a.a * f));
        }

        // Vectoring & positions
        public static System.Drawing.PointF ToPoint(this Vector2 vec)
        {
            return new System.Drawing.PointF(vec.X, vec.Y);
        }

        // GUI stuff
        public static void GuiHelpMarker(string desc)
        {
            ImGui.TextDisabled("(?)");
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                ImGui.TextUnformatted(desc);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }

        // Logic stuff
        public static LogicValue[] GetLogicValues(List<CircuitInput> lst)
        {
            LogicValue[] arr = new LogicValue[lst.Count];

            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = lst[i].Value;
            }
            return arr;
        }
    }
}
