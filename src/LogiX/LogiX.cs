using System.Drawing;
using System.Numerics;
using LogiX.Content;
using LogiX.Content.Scripting;
using LogiX.Graphics;
using LogiX.Graphics.UI;
using LogiX.Rendering;
using Symphony;
using Symphony.Common;
using static LogiX.OpenGL.GL;

namespace LogiX;

public class LogiX : Game
{
    public static ContentManager<ContentMeta> ContentManager { get; private set; }
    Camera2D cam;
    Simulation sim;

    public override void Initialize(string[] args)
    {
        sim = new Simulation();

        IOGroup g1 = new IOGroup("g1", ComponentSide.LEFT, 0);
        IOGroup g2 = new IOGroup("g2", ComponentSide.LEFT, 1);
        IOGroup g3 = new IOGroup("g2", ComponentSide.RIGHT, 2);
        IOMapping mapping = new IOMapping(g1, g2, g3);

        sim.AddComponent(new ANDGate(mapping, 2), new Vector2i(35, 20));
        sim.AddComponent(new ANDGate(mapping, 2), new Vector2i(45, 20));

        var w1 = new Wire(new Vector2i(38, 20), new Vector2i(42, 20));
        w1.RootNode.Children[0].AddChild(new WireNode(new Vector2i(44, 20)));
        var w2 = new WireNode(new Vector2i(42, 21));
        w1.RootNode.Children[0].AddChild(w2);
        w2.AddChild(new WireNode(new Vector2i(44, 21)));

        sim.AddWire(w1);
    }

    public override void LoadContent(string[] args)
    {
        glEnable(GL_BLEND);
        glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

        Func<string, IContentSource> factory = (path) =>
        {
            if (Path.GetExtension(path) == ".zip")
            {
                return new ZipFileContentSource(path);
            }
            else
            {
                return new DirectoryContentSource(path);
            }
        };

        var validator = new ContentValidator();
        var collection = new DirectoryCollectionProvider(@"C:\Users\RichieZ\repos\GoodGame\assets\core", factory);
        var loader = new ContentLoader();

        var config = new ContentManagerConfiguration<ContentMeta>(validator, collection, loader);
        ContentManager = new ContentManager<ContentMeta>(config);

        ContentManager.StartedLoading += (sender, e) =>
        {
            Console.WriteLine($"Started Loading!");
        };

        ContentManager.FinishedLoading += (sender, e) =>
        {
            Console.WriteLine($"Finished Loading!");

            // Log all loaded entries
            foreach (var entry in ContentManager.GetContentItems())
            {
                Console.WriteLine($"Loaded {entry.Identifier}");
            }

            // The following background task will poll for content that is changed and then reloads it
            Task.Run(async () =>
            {
                while (true)
                {
                    await ContentManager.PollForSourceUpdates();
                    await Task.Delay(1000); // No more than 1 check per second is needed
                }
            });

            ScriptManager.Initialize(ContentManager);
        };

        ContentManager.InvalidContentStructureError += (sender, e) =>
        {
            Console.WriteLine($"Invalid Content Structure Error: {e.Error}");
        };

        ContentManager.ContentFailedToLoadError += (sender, e) =>
        {
            Console.WriteLine($"Failed to load content: {e.Error}");
        };

        ContentManager.StartedLoadingStage += (sender, e) =>
        {
            Console.WriteLine($"Starting stage {e.Stage.StageName}...");
        };

        ContentManager.FinishedLoadingStage += (sender, e) =>
        {
            Console.WriteLine($"Finished stage {e.Stage.StageName}!");
            if (e.Stage is CoreLoadingStage)
            {
                var tex = e.CurrentlyLoaded.GetContentItem<Texture2D>("content_1.texture.gravel");
                tex.ContentUpdated += (sender, e) =>
                {
                    DisplayManager.SetWindowIcon(tex);
                };
                DisplayManager.SetWindowIcon(tex);
            }
            if (e.Stage is NormalLoadingStage)
            {
                normalDone = true;
            }
        };

        ContentManager.ContentItemStartedLoading += (sender, e) =>
        {
            Console.WriteLine($"Loading {e.ItemPath}...");
            DisplayManager.SetWindowTitle($"GoodGame - {MathF.Round(e.CurrentStageProgress, 2)} - Loading {e.ItemPath}");
        };

        ContentManager.ContentItemReloaded += (sender, e) =>
        {
            Console.WriteLine($"Reloaded {e.Entry.EntryPath} in {e.Stage.StageName}!");
        };

        DisplayManager.OnFramebufferResize += (sender, e) =>
        {
            DisplayManager.LockedGLContext(() =>
            {
                glViewport(0, 0, (int)e.X, (int)e.Y);
                Console.WriteLine($"Framebuffer Resized to {e.X}x{e.Y}");
                cam = new Camera2D(DisplayManager.GetWindowSizeInPixels() / 2f, 1f);
            });
        };

        cam = new Camera2D(DisplayManager.GetWindowSizeInPixels() / 2f, 1f);

        TextureRenderer.InitGL();
        PrimitiveRenderer.InitGL(500);
        TextRenderer.InitGL();

        GUI.Init("content_1.font.default", "content_1.shader_program.text");

        DisplayManager.ReleaseGLContext();
        _ = ContentManager.LoadAsync();
    }

    public override void Update()
    {

    }

    bool u1 = false;
    bool u2 = false;
    bool h1 = false;
    bool h2 = false;
    bool normalDone = false;

    public override void Render()
    {
        DisplayManager.LockedGLContext(() =>
        {
            glClearColor(0.1f, 0.2f, 0.3f, 1);
            glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

            if (!normalDone)
            {
                return;
            }

            GUI.Begin(this.cam);

            //this.sim.Reset();

            GUI.Checkbox("U1", new Vector2(100, 100), new Vector2(30, 30), ref u1);
            GUI.Checkbox("U2", new Vector2(100, 150), new Vector2(30, 30), ref u2);
            GUI.Checkbox("H1", new Vector2(100, 200), new Vector2(30, 30), ref h1);
            GUI.Checkbox("H2", new Vector2(100, 250), new Vector2(30, 30), ref h2);

            if (u1)
            {
                LogicValue val = h1 ? LogicValue.HIGH : LogicValue.LOW;
                this.sim.PushValuesAt(new Vector2i(34, 20), val);
            }
            if (u2)
            {
                LogicValue val = h2 ? LogicValue.HIGH : LogicValue.LOW;
                this.sim.PushValuesAt(new Vector2i(34, 21), val);
            }

            this.sim.PushValuesAt(new Vector2i(38, 21), LogicValue.HIGH);

            DisplayManager.SetWindowTitle($"HOT: {GUI._hotID}, ACTIVE: {GUI._activeID}, KBD: {GUI._kbdFocusID}, _CARET: {GUI._caretPosition}, DROP: {GUI._showingDropdownID}");

            GUI.End();

            this.sim.Tick();
            this.sim.Render(cam);

            DisplayManager.SwapBuffers(-1);
        });
    }

    public override void Unload()
    {
        ContentManager.UnloadAllContent();
    }
}