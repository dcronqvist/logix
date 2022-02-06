using Raylib_cs;
using ImGuiNET;
using System;
using System.Linq;
using System.Numerics;
using LogiX.Editor;

namespace Testing;

class Program
{
    static void Main(string[] args)
    {
        Editor editor = new Editor();
        Settings.LoadSettings();
        editor.Run(Settings.GetSettingValue<int>("windowWidth"), Settings.GetSettingValue<int>("windowHeight"), "LogiX", Settings.GetSettingValue<int>("preferredFramerate"), "assets/logo.png");
    }
}