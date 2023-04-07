using System.Drawing;
using System.Numerics;
using LogiX.Architecture;
using LogiX.Architecture.BuiltinComponents;
using LogiX.Content;
using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Rendering;
using Symphony;
using Symphony.Common;
using static LogiX.OpenGL.GL;

namespace LogiX.Minimal;

public class LEDMatrixWindow : Game
{
    public LEDMatrix Matrix { get; private set; }
    public Keyboard Keyboard { get; private set; }
    public Camera2D Camera { get; private set; }
    public Simulation Simulation { get; private set; }

    public ContentManager<ContentMeta> ContentManager { get; private set; }
    public Vector2 WindowSize { get; private set; }
    public int Scale { get; private set; }

    public LEDMatrixWindow(int scale, Simulation simulation, LEDMatrix matrix, Keyboard keyboard)
    {
        this.Matrix = matrix;
        this.Simulation = simulation;
        this.Scale = scale;
        this.Keyboard = keyboard;
    }

    public override (Vector2i, bool) Initialize(string[] args)
    {
        var data = (LEDMatrixData)this.Matrix.GetNodeData();
        var x = new Vector2i(data.Columns * this.Scale, data.Rows * this.Scale);
        this.WindowSize = new Vector2(x.X, x.Y);
        return (x, false);
    }

    public override void LoadContent(string[] args)
    {
        glEnable(GL_BLEND);
        glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

        this.Camera = new Camera2D(this.WindowSize / 2f, 1f);

        glViewport(0, 0, (int)this.WindowSize.X, (int)this.WindowSize.Y);
        PrimitiveRenderer.InitGL(64);

        var basePath = AppDomain.CurrentDomain.BaseDirectory + "/assets";

        var coreSource = new DirectoryContentSource(Path.GetFullPath($"{basePath}/core"));
        var validator = new ContentValidator();
        var collection = IContentCollectionProvider.FromListOfSources(coreSource);
        var loader = new ContentLoader();

        var config = new ContentManagerConfiguration<ContentMeta>(validator, collection, loader);
        ContentManager = new ContentManager<ContentMeta>(config);
        Utilities.ContentManager = ContentManager;

        DisplayManager.ReleaseGLContext();
        ContentManager.Load();
        DisplayManager.AcquireGLContext();

        var tex = ContentManager.GetContentItem<Texture2D>("logix_core:core/icon.png");
        DisplayManager.SetWindowIcon(tex);

        Task.Run(() =>
        {
            while (true)
            {
                this.Simulation.Step();
            }
        });

        Input.OnChar += (sender, e) =>
        {
            if (this.Keyboard is not null)
            {
                this.Keyboard.RegisterChar(e);
            }
        };

        Input.OnEnterPressed += (sender, e) =>
        {
            if (this.Keyboard is not null)
            {
                this.Keyboard.RegisterChar('\n');
            }
        };

        Input.OnBackspace += (sender, e) =>
        {
            if (this.Keyboard is not null)
            {
                this.Keyboard.RegisterChar('\b');
            }
        };

        Input.OnCharMods += (sender, e) =>
        {
            if (this.Keyboard is not null)
            {
                var c = e.Item1;
                var mods = e.Item2;
                if (mods.HasFlag(ModifierKeys.Control) && c == 'l')
                {
                    this.Keyboard.RegisterChar('\f');
                }
            }
        };
    }

    public override void Render()
    {
        var data = this.Matrix._matrix;
        var matrixData = this.Matrix.GetNodeData() as LEDMatrixData;

        var shader = ContentManager.GetContentItem<ShaderProgram>("logix_core:shaders/primitive/primitive.shader");

        Framebuffer.BindDefaultFramebuffer();
        Framebuffer.Clear(matrixData.BackgroundColor);

        var mode = matrixData.Mode;

        for (int x = 0; x < matrixData.Columns; x++)
        {
            for (int y = 0; y < matrixData.Rows; y++)
            {
                var value = data[x, y];
                var color = value == LogicValue.HIGH ? matrixData.OnColor : matrixData.OffColor;
                var pos = new Vector2(x * Scale, y * Scale) + new Vector2(Scale / 2f);

                if (mode == LEDMatrixMode.Circular)
                {
                    PrimitiveRenderer.RenderCircle(pos, this.Scale / 2f, 0f, color, sides: 20);
                }
                else
                {
                    var rect = new RectangleF(pos.X - Scale / 2f, pos.Y - Scale / 2f, Scale, Scale);
                    PrimitiveRenderer.RenderRectangle(rect, new Vector2(Scale, Scale), 0f, color);
                }
            }
        }

        PrimitiveRenderer.FinalizeRender(shader, this.Camera);
        DisplayManager.SwapBuffers(-1);
    }

    public override void Unload()
    {

    }

    public override void Update()
    {
        Input.Begin();

        Input.End();
    }
}