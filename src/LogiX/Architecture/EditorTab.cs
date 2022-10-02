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

        this.Sim.LockedAction(s => s.Tick());
    }

    public void Render()
    {
        var fShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.fb_default");
        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");

        // Render workspace to its own framebuffer, then render that framebuffer to the screen
        this.WorkspaceFramebuffer.Bind(() =>
        {
            Framebuffer.Clear(ColorF.Darken(ColorF.LightGray, 0.9f));

            this.DrawGrid();
            this.Sim.LockedAction(s => s.Render(this.Camera));
            this.FSM.Render(this);

            PrimitiveRenderer.RenderCircle(pShader, Input.GetMousePosition(this.Camera).ToVector2i(16).ToVector2(16), 4, 0, ColorF.Red, this.Camera);
        });

        this.GUIFramebuffer.Bind(() =>
        {
            //Framebuffer.Clear(ColorF.Transparent);
            GUI.Begin(Framebuffer.GetDefaultCamera());

            this.SubmitGUI();
            this.FSM.SubmitUI(this);

            DisplayManager.SetWindowTitle($"STATE: {this.FSM.CurrentState.GetType().ToString()}, HOT: {GUI._hotID}, ACTIVE: {GUI._activeID}, KBD: {GUI._kbdFocusID}, _CARET: {GUI._caretPosition}, DROP: {GUI._showingDropdownID}, WIRES: {this.Sim.LockedAction(s => s.Wires.Count)}");
            GUI.End();
        });

        Framebuffer.RenderFrameBufferToScreen(fShader, this.WorkspaceFramebuffer);
        Framebuffer.RenderFrameBufferToScreen(fShader, this.GUIFramebuffer);
    }

    public void SubmitGUI()
    {
        if (GUI.Button("TEST", new Vector2(50, 50), new Vector2(100, 50)))
        {
            Console.WriteLine("TEST");
        }

        if (GUI.Button("ADD", new Vector2(50, 150), new Vector2(100, 50)))
        {
            var desc = ComponentDescription.CreateDefaultComponentDescription("content_1.script_type.ORGATE");
            var middleOfCam = Camera.FocusPosition / 16;
            var pos = new Vector2i((int)middleOfCam.X, (int)middleOfCam.Y);
            desc.Position = pos;
            CAddComponent add = new(desc);

            this.Execute(add, this);
        }

        if (GUI.Button("UNDO", new Vector2(50, 250), new Vector2(100, 50)))
        {
            this.Undo(this);
        }

        if (GUI.Button("REDO", new Vector2(50, 350), new Vector2(100, 50)))
        {
            this.Redo(this);
        }
    }

    public void PerformSimulationTick()
    {
        this.Sim.LockedAction(s =>
        {
            s.Tick();
        });
    }
}