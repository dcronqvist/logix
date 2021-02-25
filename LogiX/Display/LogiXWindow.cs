using LogiX.Logging;
using LogiX.Settings;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LogiX.Display
{
    class LogiXWindow : BaseWindow
    {
        public LogiXWindow() : base(new Vector2(1280, 720), "LogiX")
        {

        }

        public override void Initialize()
        {
            // Should initialize some basic settings and stuff
            // Load settings files etc.

            // Set window icon and stuff can also be done with this.
            SettingManager.LoadSettings();

            // Apply window size settings to window
            int windowWidth = int.Parse(SettingManager.GetSetting("window-width", "1440"));
            int windowHeight = int.Parse(SettingManager.GetSetting("window-height", "768"));
            Vector2 size = new Vector2(windowWidth, windowHeight);
            SetWindowSize(size, false);
            LogManager.AddEntry($"Creating LogiX window with width = {(int)size.X} and height = {(int)size.Y}");
        }

        public override void LoadContent()
        {
            // Load files for use in the program.
        }

        public override void Render()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.BLACK);
            Raylib.DrawFPS(5, 5);
            Raylib.EndDrawing();
        }

        public override void Update()
        {
            
        }

        public override void Unload()
        {
            LogManager.DumpToFile();
        }
    }
}
