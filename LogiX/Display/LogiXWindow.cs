using LogiX.Circuits.Drawables;
using LogiX.Circuits.Logic;
using LogiX.Logging;
using LogiX.Settings;
using LogiX.Simulation;
using LogiX.Utils;
using Raylib_cs;
using System;
using System.Numerics;

namespace LogiX.Display
{
    class LogiXWindow : BaseWindow
    {
        Camera2D cam;

        Vector2 currentMousePos;
        Vector2 previousMousePos;
        public Vector2 TopLeftCorner
        {
            get
            {
                return new Vector2(cam.target.X - ((WindowSize.X / 2f) / cam.zoom), cam.target.Y - ((WindowSize.Y / 2f) / cam.zoom));
            }
        }

        Simulator sim;

        Tuple<int, DrawableComponent> intup;
        Tuple<int, DrawableComponent> outtup;

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

            // Create new camera that's focused on (0,0).
            // This should maybe be done somewhere else. But is good for testing atm.
            cam = new Camera2D(size / 2f, Vector2.Zero, 0f, 1f);
            sim = new Simulator();
        }

        public override void LoadContent()
        {
            // Load files for use in the program.
            Raylib.SetTargetFPS(144);
        }

        public Vector2 GetMousePositionInWorld()
        {
            return TopLeftCorner + currentMousePos / cam.zoom;
        }

        public void RenderGrid()
        {
            // TODO: Move this to more suitable spot, also try to shorten this down.
            int windowWidth = int.Parse(SettingManager.GetSetting("window-width", "1440"));
            int windowHeight = int.Parse(SettingManager.GetSetting("window-height", "768"));
            Vector2 size = new Vector2(windowWidth, windowHeight);
            Vector2 camPos = cam.target;
            Vector2 viewSize = size / cam.zoom;

            int pixelsBetweenLines = int.Parse(SettingManager.GetSetting("grid-size", "250"));

            // Render vertical lines
            for (int i = (int)((camPos.X - viewSize.X / 2f) / pixelsBetweenLines); i <= (int)((camPos.X + viewSize.X / 2f) / pixelsBetweenLines); i++)
            {
                float lineX = i * pixelsBetweenLines;
                float lineYstart = (camPos.Y - viewSize.Y / 2f);
                float lineYend = (camPos.Y + viewSize.Y / 2f);
                Raylib.DrawLine((int)lineX, (int)lineYstart, (int)lineX, (int)lineYend, Color.DARKGRAY.Opacity(0.3f));
            }

            // Render vertical lines
            for (int i = (int)((camPos.Y - viewSize.Y / 2f) / pixelsBetweenLines); i <= (int)((camPos.Y + viewSize.Y / 2f) / pixelsBetweenLines); i++)
            {
                float lineY = i * pixelsBetweenLines;
                float lineXstart = (camPos.X - viewSize.X / 2f);
                float lineXend = (camPos.X + viewSize.X / 2f);
                Raylib.DrawLine((int)lineXstart, (int)lineY, (int)lineXend, (int)lineY, Color.DARKGRAY.Opacity(0.3f));
            }
        }

        public override void Render()
        {
            Raylib.BeginDrawing();
            Raylib.BeginMode2D(cam);
            Raylib.ClearBackground(Color.LIGHTGRAY);
            RenderGrid();
            sim.Render(GetMousePositionInWorld());
            if(outtup != null)
            {
                Raylib.DrawLineBezier(outtup.Item2.GetOutputPosition(outtup.Item1), GetMousePositionInWorld(), 2f, Color.WHITE);
            }
            if(intup != null)
            {
                Raylib.DrawLineBezier(intup.Item2.GetInputPosition(intup.Item1), GetMousePositionInWorld(), 2f, Color.WHITE);
            }
            Raylib.EndMode2D();
            Raylib.DrawRectangle(0, 0, 250, (int)base.WindowSize.Y, Color.DARKGRAY.Opacity(0.8f));
            Raylib.DrawFPS(5, 5);
            Raylib.EndDrawing();
        }

        public override void Update()
        {
            // TODO: Move this to more suitable place
            currentMousePos = Raylib.GetMousePosition();
            sim.Update(GetMousePositionInWorld());

            if (Raylib.GetMousePosition().X > 250)
            {
                Vector2 mousePos = GetMousePositionInWorld();

                if (Raylib.GetMouseWheelMove() > 0)
                {
                    cam.zoom *= 1.05f;
                }
                if (Raylib.GetMouseWheelMove() < 0)
                {
                    cam.zoom *= 1/ 1.05f;
                }
                if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_MIDDLE_BUTTON))
                {
                    cam.target -= (currentMousePos - previousMousePos) * 1f / cam.zoom;
                }
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_BACKSPACE) || Raylib.IsKeyPressed(KeyboardKey.KEY_DELETE))
                {
                    sim.DeleteSelectedComponents();
                }
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_A))
                {
                    sim.AddComponent(new DrawableLogicGate(mousePos, "AND", new ANDGateLogic()));
                }
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_Z))
                {
                    cam.zoom = 1f;
                }
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_S))
                {
                    sim.AddComponent(new DrawableCircuitSwitch(GetMousePositionInWorld()));
                }
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_X))
                {
                    sim.AddComponent(new DrawableLogicGate(GetMousePositionInWorld(), "XOR", new XORGateLogic()));
                }
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_O))
                {
                    sim.AddComponent(new DrawableLogicGate(GetMousePositionInWorld(), "OR", new ORGateLogic()));
                }

                DrawableComponent hovered = sim.GetComponentFromPosition(mousePos);

                if(Raylib.IsMouseButtonDown(MouseButton.MOUSE_LEFT_BUTTON))
                {
                    if(hovered != null)
                    {
                        if(sim.SelectedComponents.Contains(hovered))
                        {
                            foreach (DrawableComponent dc in sim.SelectedComponents)
                            {
                                dc.Position += (currentMousePos - previousMousePos) / cam.zoom;
                            }
                        }
                    }
                }

                Tuple<int, DrawableComponent> tempintup = sim.GetComponentAndInputFromPos(mousePos);
                Tuple<int, DrawableComponent> tempouttup = sim.GetComponentAndOutputFromPos(mousePos);

                if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
                {
                    if(hovered != null)
                    {
                        if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT))
                        {
                            sim.AddSelectedComponent(hovered);
                        }
                        else 
                        {
                            sim.ClearSelectedComponents();
                            sim.AddSelectedComponent(hovered);
                        }
                    }

                    // Pressing output first and then input
                    if (outtup == null && tempintup == null)
                    {
                        if(tempouttup != null)
                        {
                            outtup = tempouttup;
                        }
                    }
                     
                    if(tempintup != null && outtup != null)
                    {
                        DrawableWire dw = new DrawableWire(outtup.Item2, tempintup.Item2, outtup.Item1, tempintup.Item1);
                        sim.AddWire(dw);
                        outtup.Item2.AddOutputWire(outtup.Item1, dw);
                        tempintup.Item2.SetInputWire(tempintup.Item1, dw);
                        outtup = null;
                        return;
                    }
                }

                if (Raylib.IsKeyPressed(KeyboardKey.KEY_ESCAPE))
                {
                    sim.ClearSelectedComponents();
                }
            }

            previousMousePos = currentMousePos;
        }

        public override void Unload()
        {
            LogManager.DumpToFile();
        }
    }
}
