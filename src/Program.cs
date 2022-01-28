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
        Console.WriteLine(Raylib.GetMonitorRefreshRate(Raylib.GetCurrentMonitor()) + "Hz");
        editor.Run(Settings.GetSettingValue<int>("windowWidth"), Settings.GetSettingValue<int>("windowHeight"), "LogiX", Raylib.GetMonitorRefreshRate(Raylib.GetCurrentMonitor()), "assets/logo.png");
    }
}