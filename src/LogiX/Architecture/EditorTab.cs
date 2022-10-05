using System.Numerics;
using LogiX.Architecture.Commands;
using LogiX.Architecture.Serialization;
using LogiX.Architecture.StateMachine;
using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Graphics.UI;
using LogiX.Rendering;

namespace LogiX.Architecture;

public class EditorTab : Invoker<EditorTab>
{
    public Camera2D Camera { get; set; }
    public ThreadSafe<Simulation> Sim { get; private set; }
    public Circuit SavedCircuit { get; private set; }
    public Framebuffer WorkspaceFramebuffer { get; private set; }
    public Framebuffer GUIFramebuffer { get; private set; }
    public TabFSM FSM { get; private set; }

    public EditorTab(string tabName, Circuit initialCircuit)
    {
        this.Camera = new Camera2D(Vector2.Zero, 1f);
        this.SavedCircuit = initialCircuit;
        this.Sim = new(Simulation.FromCircuit(initialCircuit));
        this.WorkspaceFramebuffer = new Framebuffer();
        this.GUIFramebuffer = new Framebuffer();
        this.FSM = new TabFSM();

        Input.OnScroll += (sender, e) =>
        {
            // TODO: Only perform if the tab is focused.

            if (e > 0)
                Camera.Zoom *= 1.1f;
            else if (e < 0)
                Camera.Zoom /= 1.1f;

            Camera.Zoom = Math.Clamp(Camera.Zoom, 0.3f, 10f);
        };
    }

    public EditorTab(string tabName)
    {
        this.Camera = new Camera2D(Vector2.Zero, 1f);
        this.SavedCircuit = new Circuit();
        this.Sim = new(new());
        this.WorkspaceFramebuffer = new Framebuffer();
        this.GUIFramebuffer = new Framebuffer();
        this.FSM = new TabFSM();

        Input.OnScroll += (sender, e) =>
        {
            // TODO: Only perform if the tab is focused.

            if (e > 0)
                Camera.Zoom *= 1.1f;
            else if (e < 0)
                Camera.Zoom /= 1.1f;

            Camera.Zoom = Math.Clamp(Camera.Zoom, 0.3f, 10f);
        };

        NewGUI.Init(LogiX.ContentManager.GetContentItem<Font>("content_1.font.default"));
    }

    public void DrawGrid()
    {
        // Get camera's position and the size of the current view
        // This depends on the zoom of the camera. Dividing by the
        // zoom gives a correct representation of the actual visible view.
        Vector2 viewSize = DisplayManager.GetViewSize(this.Camera);
        Vector2 camPos = this.Camera.FocusPosition;

        int pixelsInBetweenLines = 16;

        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");
        var color = ColorF.Darken(ColorF.LightGray, 0.8f);

        // Draw vertical lines
        for (int i = (int)((camPos.X - viewSize.X / 2.0F) / pixelsInBetweenLines); i < ((camPos.X + viewSize.X / 2.0F) / pixelsInBetweenLines); i++)
        {
            int lineX = i * pixelsInBetweenLines;
            int lineYstart = (int)(camPos.Y - viewSize.Y / 2.0F);
            int lineYend = (int)(camPos.Y + viewSize.Y / 2.0F);

            PrimitiveRenderer.RenderLine(pShader, new Vector2(lineX, lineYstart), new Vector2(lineX, lineYend), 1, color, Camera);
        }

        // Draw horizontal lines
        for (int i = (int)((camPos.Y - viewSize.Y / 2.0F) / pixelsInBetweenLines); i < ((camPos.Y + viewSize.Y / 2.0F) / pixelsInBetweenLines); i++)
        {
            int lineY = i * pixelsInBetweenLines;
            int lineXstart = (int)(camPos.X - viewSize.X / 2.0F);
            int lineXend = (int)(camPos.X + viewSize.X / 2.0F);

            PrimitiveRenderer.RenderLine(pShader, new Vector2(lineXstart, lineY), new Vector2(lineXend, lineY), 1, color, Camera);
        }
    }

    public void Update()
    {
        this.FSM.Update(this);

        var mousePos = Input.GetMousePosition(this.Camera).ToVector2i(16);

        if (Input.IsKeyPressed(Keys.Alpha1))
        {
            var c = ComponentDescription.CreateDefaultComponent("content_1.script_type.CONST");
            c.Position = mousePos;
            this.Sim.LockedAction(s => s.AddComponent(c, c.Position));
        }
        if (Input.IsKeyPressed(Keys.Alpha2))
        {
            var c = ComponentDescription.CreateDefaultComponent("content_1.script_type.ORGATE");
            c.Position = mousePos;
            this.Sim.LockedAction(s => s.AddComponent(c, c.Position));
        }
        if (Input.IsKeyPressed(Keys.Alpha3))
        {
            var c = ComponentDescription.CreateDefaultComponent("content_1.script_type.ANDGATE");
            c.Position = mousePos;
            this.Sim.LockedAction(s => s.AddComponent(c, c.Position));
        }

        if (Input.IsKeyPressed(Keys.Space))
        {
            this.Sim.LockedAction(s =>
            {
                if (s.TryGetWireSegmentAtPos(mousePos, out var segment, out var wire))
                {
                    s.DisconnectPoints(segment.Item1, segment.Item2);
                }
            });
        }

        if (Input.IsKeyPressed(Keys.G))
        {
            this.Sim.LockedAction(s =>
            {
                if (s.TryGetWireAtPos(mousePos, out var wire))
                {
                    wire.MergeEdgesThatMeetAt(mousePos);
                }
            });
        }

        if (Input.IsKeyPressed(Keys.H))
        {
            this.Sim.LockedAction(s =>
            {
                s.RemoveWirePoint(mousePos);
            });
        }

        this.Sim.LockedAction(s => s.Tick());
    }

    public void Render()
    {
        var fShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.fb_default");
        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");
        var mousePos = Input.GetMousePosition(this.Camera).ToVector2i(16);

        // Render workspace to its own framebuffer, then render that framebuffer to the screen
        this.WorkspaceFramebuffer.Bind(() =>
        {
            Framebuffer.Clear(ColorF.Darken(ColorF.LightGray, 0.9f));

            this.DrawGrid();
            this.Sim.LockedAction(s => s.Render(this.Camera));
            this.FSM.Render(this);

            PrimitiveRenderer.RenderCircle(pShader, Input.GetMousePosition(this.Camera).ToVector2i(16).ToVector2(16), 4, 0, ColorF.Red, this.Camera);

            this.Sim.LockedAction(s =>
            {
                if (s.TryGetWireSegmentAtPos(mousePos, out var segment, out var wire))
                {
                    var points = Utilities.GetAllGridPointsBetween(segment.Item1, segment.Item2);

                    foreach (var point in points)
                    {
                        PrimitiveRenderer.RenderCircle(pShader, point.ToVector2(16), 4, 0, ColorF.Green, this.Camera);
                    }
                }
            });
        });

        this.GUIFramebuffer.Bind(() =>
        {
            NewGUI.Begin();
            SubmitGUI();
            NewGUI.End();

            string s = $"ANY_WIN_HOV: {NewGUI.AnyWindowHovered()}, HOVERED: {(NewGUI.HoveredEnvironment == null ? "null" : NewGUI.HoveredEnvironment.GetID())}, HOT: {NewGUI.HotID}";
            DisplayManager.SetWindowTitle(s);
        });

        Framebuffer.RenderFrameBufferToScreen(fShader, this.WorkspaceFramebuffer);
        Framebuffer.RenderFrameBufferToScreen(fShader, this.GUIFramebuffer);
    }

    bool cute = false;

    public void SubmitGUI()
    {
        NewGUI.BeginMainMenuBar();

        if (NewGUI.Button("Butt"))
        {

        }
        NewGUI.Spacer(5);
        if (NewGUI.Button("Butt1"))
        {

        }
        NewGUI.Spacer(5);

        if (NewGUI.Button("Butt2"))
        {

        }
        NewGUI.Spacer(5);

        if (NewGUI.Button("Butt3"))
        {

        }
        NewGUI.Spacer(5);
        NewGUI.Checkbox("cute", ref cute);

        NewGUI.EndMainMenuBar();

        if (NewGUI.BeginWindow("Another One", new Vector2(120, 90)))
        {
            NewGUI.Spacer(5);

            if (NewGUI.Button("Testi Testi"))
            {
                Console.WriteLine("Testi Testi");
            }

            NewGUI.SameLine(5);

            if (NewGUI.Button("Testi Testi 2"))
            {
                Console.WriteLine("Testi Testi 2");
            }
        }
        NewGUI.EndWindow();

        if (NewGUI.BeginWindow("är saga söt?", new Vector2(100, 100)))
        {
            NewGUI.Spacer(5);

            if (NewGUI.Button("se svar"))
            {
                cute = !cute;
            }

            NewGUI.SameLine(5);
            NewGUI.Checkbox("Cute", ref cute);

            if (cute)
            {
                NewGUI.Spacer(5);
                NewGUI.Label("japp! saga är jättesöt!");
            }

        }
        NewGUI.EndWindow();
    }

    public void PerformSimulationTick()
    {
        this.Sim.LockedAction(s =>
        {
            s.Tick();
        });
    }
}