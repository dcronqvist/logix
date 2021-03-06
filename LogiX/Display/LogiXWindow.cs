using ImGuiNET;
using LogiX.Circuits.Drawables;
using LogiX.Circuits.Integrated;
using LogiX.Circuits.Logic;
using LogiX.Logging;
using LogiX.Settings;
using LogiX.Simulation;
using LogiX.UI;
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
        Tuple<int, DrawableComponent> outtup;
        DrawableComponent lastSelectedComponent;

        ImGUIController controller;
        bool _simulationOn;

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
            _simulationOn = true;
        }

        public override void LoadContent()
        {
            // Load files for use in the program.
            Raylib.SetTargetFPS(500);
            controller = new ImGUIController();
            controller.Load((int)base.WindowSize.X, (int)base.WindowSize.Y);
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
            Raylib.EndMode2D();
            controller.Draw();
            Raylib.EndDrawing();
        }

        public override void Update()
        {
            controller.Update(Raylib.GetFrameTime());
            SubmitUI();

            // TODO: Move this to more suitable place
            currentMousePos = Raylib.GetMousePosition();

            if(_simulationOn)
                sim.UpdateLogic(GetMousePositionInWorld());
            sim.Update(GetMousePositionInWorld());

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
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_DELETE))
            {
                sim.DeleteSelectedComponents();
            }
            /*
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
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_L))
            {
                sim.AddComponent(new DrawableCircuitLamp(GetMousePositionInWorld()));
            }
            */

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
                    lastSelectedComponent = hovered;
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
                    if(tempintup.Item2.Inputs[tempintup.Item1].Signal == null)
                    {
                        DrawableWire dw = new DrawableWire(outtup.Item2, tempintup.Item2, outtup.Item1, tempintup.Item1);
                        sim.AddWire(dw);
                        outtup.Item2.AddOutputWire(outtup.Item1, dw);
                        tempintup.Item2.SetInputWire(tempintup.Item1, dw);
                        outtup = null;
                        return;
                    }
                }
            }
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON))
            {
                if(tempintup != null)
                {
                    DrawableWire dw = (DrawableWire)tempintup.Item2.Inputs[tempintup.Item1].Signal;
                    tempintup.Item2.Inputs[tempintup.Item1].RemoveSignal();
                    dw.From.RemoveOutputWire(dw.FromIndex, dw);
                    sim.RemoveWire(dw);
                }
            }

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_ESCAPE))
            {
                sim.ClearSelectedComponents();
                outtup = null;
            }

            previousMousePos = currentMousePos;
        }

        public override void Unload()
        {
            LogManager.DumpToFile();
        }

        public void SubmitUI()
        {
            ImGui.BeginMainMenuBar();

            // File thing
            if (ImGui.BeginMenu("File"))
            {
                ImGui.MenuItem("Empty for now");
                ImGui.EndMenu();
            }
            
            // Edit thing
            if(ImGui.BeginMenu("Edit"))
            {
                ImGui.MenuItem("Empty for now");
                ImGui.EndMenu();
            }

            // Simulation
            if (ImGui.BeginMenu("Simulation"))
            {
                ImGui.Checkbox("Simulating", ref _simulationOn);
                if (ImGui.Button("Update simulation"))
                {
                    sim.UpdateLogic(GetMousePositionInWorld());
                }
                ImGui.EndMenu();
            }

            if(ImGui.BeginMenu("Integrated Circuits"))
            {
                if(ImGui.MenuItem("Create From Selection"))
                {
                    ICDescription icd = new ICDescription(sim.SelectedComponents);
                    sim.AddComponent(new DrawableIC(new Vector2(100, 100), "TESTER", icd));
                }
                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();


            // SIMULATION WINDOW
            {
                ImGui.SetNextWindowPos(new Vector2(25, 50), ImGuiCond.Appearing);
                ImGui.SetNextWindowCollapsed(false, ImGuiCond.Appearing);
                ImGui.Begin("Components", ImGuiWindowFlags.AlwaysAutoResize); 

                if (ImGui.Button("Switch"))
                {
                    sim.AddComponent(new DrawableCircuitSwitch(GetMousePositionInWorld() + new Vector2(200, 0)));
                }

                if (ImGui.Button("Lamp"))
                {
                    sim.AddComponent(new DrawableCircuitLamp(GetMousePositionInWorld() + new Vector2(200, 0)));
                }

                if (ImGui.Button("AND"))
                {
                    sim.AddComponent(new DrawableLogicGate(GetMousePositionInWorld() + new Vector2(200, 0), "AND", new ANDGateLogic()));
                }

                if (ImGui.Button("XOR"))
                {
                    sim.AddComponent(new DrawableLogicGate(GetMousePositionInWorld() + new Vector2(200, 0), "XOR", new XORGateLogic()));
                }

                if (ImGui.Button("OR"))
                {
                    sim.AddComponent(new DrawableLogicGate(GetMousePositionInWorld() + new Vector2(200, 0), "OR", new ORGateLogic()));
                }

                if (ImGui.Button("NOR"))
                {
                    sim.AddComponent(new DrawableLogicGate(GetMousePositionInWorld() + new Vector2(200, 0), "NOR", new NORGateLogic()));
                }

                ImGui.End();
            }

            if(lastSelectedComponent is DrawableCircuitSwitch || lastSelectedComponent is DrawableCircuitLamp)
            {
                ImGui.Begin("Setting ID", ImGuiWindowFlags.AlwaysAutoResize);

                if(lastSelectedComponent is DrawableCircuitSwitch)
                {
                    DrawableCircuitSwitch dcs = (DrawableCircuitSwitch)lastSelectedComponent;
                    ImGui.InputText("ID", ref dcs.ID, 10); 
                }
                if (lastSelectedComponent is DrawableCircuitLamp)
                {
                    DrawableCircuitLamp dcl = (DrawableCircuitLamp)lastSelectedComponent;
                    ImGui.InputText("ID", ref dcl.ID, 10);
                }

                ImGui.End();
            }
        }
    }
}
