﻿using ImGuiNET;
using LogiX.Circuits;
using LogiX.Settings;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        public static Dictionary<string, string> QUICKLINK_DIRS = new Dictionary<string, string>()
        {
            { "LogiX", LOGIX_DIR },
            { "AppData", ROAMING_DIR },
            { "Desktop", Environment.GetFolderPath(Environment.SpecialFolder.Desktop) },
            { "Documents", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) }
        };

        public static void OpenPath(string path)
        {
            ProcessStartInfo psi = new ProcessStartInfo() { FileName = path, UseShellExecute = true };
            Process.Start(psi);
        }

        // File extensions
        public static string EXT_PROJ = ".lgxproj";
        public static string EXT_IC = ".lgxic";
        public static string EXT_ICCOLLECTION = ".lgxcoll";
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
        public static Color COLOR_IO_HOVER_DEFAULT = Color.SKYBLUE;
        public static Color COLOR_SELECTED_DEFAULT = Color.BLUE;
        public static Color Opacity(this Color a, float f) 
        {
            return new Color(a.r, a.g, a.b, (byte)(a.a * f));
        }

        // Drawable components
        public static float IO_V_DIST
        {
            get
            {
                return float.Parse(SettingManager.GetSetting("io-v-dist", "14"));
            }
        }
        public static float IO_H_DIST
        {
            get
            {
                return float.Parse(SettingManager.GetSetting("io-h-dist", "10"));
            }
        }
        public static float IO_SIZE
        {
            get
            {
                return float.Parse(SettingManager.GetSetting("io-size", "6"));
            }
        }
        public static int TEXT_SIZE = 10;

        // Vectoring & positions
        public static System.Drawing.PointF ToPoint(this Vector2 vec)
        {
            return new System.Drawing.PointF(vec.X, vec.Y);
        }

        // GUI stuff
        public static Vector2 BUTTON_DEFAULT_SIZE = new Vector2(80, 0);
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
        public static void GuiSettingFloatSlider(string label, string setting, float def, float min, float max)
        {
            float value = float.Parse(SettingManager.GetSetting(setting, def.ToString()));
            ImGui.SliderFloat(label, ref value, min, max, "%.1f");
            if (ImGui.BeginPopupContextItem(setting))
            {
                if (ImGui.Selectable($"Set to default ({def})")) { value = def; }
                ImGui.SetNextItemWidth(150);
                ImGui.InputFloat("", ref value, 0.1f, 1f, "%.1f");
                ImGui.EndPopup();
            }
            SettingManager.SetSetting(setting, value.ToString());
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

        // Quickdrawing
        public static void DrawRectangleLines(Vector2 tl, Vector2 tr, Vector2 br, Vector2 bl, float thick, Color col)
        {
            Vector2 topLeft = tl;
            Vector2 topRight = tr;
            Vector2 bottomLeft = bl;
            Vector2 bottomRight = br;

            Raylib.DrawLineEx(topLeft, topRight, thick, Utility.COLOR_SELECTED_DEFAULT);
            Raylib.DrawLineEx(topRight + new Vector2(0, -thick / 2f), bottomRight + new Vector2(0, thick / 2f), thick, col);
            Raylib.DrawLineEx(bottomRight, bottomLeft, thick, Utility.COLOR_SELECTED_DEFAULT);
            Raylib.DrawLineEx(bottomLeft + new Vector2(0, thick / 2f), topLeft + new Vector2(0, -thick / 2f), thick, col);
        }

        public static bool TryGetFileInfo(string fileName, out FileInfo realFile)
        {
            try
            {
                realFile = new FileInfo(fileName);
                return true;
            }
            catch
            {
                realFile = null;
                return false;
            }
        }
    }
}
