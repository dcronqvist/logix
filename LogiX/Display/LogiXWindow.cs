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
    }
}
