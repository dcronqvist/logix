using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using LogiX.Architecture;
using LogiX.Architecture.Serialization;
using LogiX.Content;
using LogiX.Content.Scripting;
using LogiX.GLFW;
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
    bool allContentLoaded = false;
    bool coreLoaded = false;

    // Camera2D cam;
    // ThreadSafe<Simulation> sim;
    // Mutex simTickMutex = new();
    // Task simTickTask;
    // Framebuffer gui;
    // double currentDiff = 0;
    // float ticksPerSecond = 0;
    // bool ticking = true;

    public override void Initialize(string[] args)
    {
        // sim = new(new());

        // sim.LockedAction(s =>
        // {
        //     // s.AddComponent(new ANDGate(mapping, 2), new Vector2i(35, 20));
        //     // s.AddComponent(new ANDGate(mapping, 2), new Vector2i(45, 20));

        //     // var w1 = new Wire(new Vector2i(38, 20), new Vector2i(42, 20));
        //     // w1.RootNode.Children[0].AddChild(new WireNode(new Vector2i(44, 20)));
        //     // var w2 = new WireNode(new Vector2i(42, 21));
        //     // w1.RootNode.Children[0].AddChild(w2);
        //     // w2.AddChild(new WireNode(new Vector2i(44, 21)));

        //     // var desc = w1.GetDescriptionOfInstance();

        //     // s.AddWire(w1);
        //     // s.AddComponent(new ANDGate(m1, 2), new Vector2i(35, 30));

        //     // s.AddComponent(new Architecture.Switch(new IOMapping(new IOGroup("g1", ComponentSide.RIGHT, 0))), new Vector2i(30, 20));
        //     // s.AddComponent(new Architecture.Switch(new IOMapping(new IOGroup("g1", ComponentSide.RIGHT, 0))), new Vector2i(30, 24));

        //     // var w3 = new Wire(new Vector2i(32, 20), new Vector2i(34, 20));
        //     // s.AddWire(w3);

        //     // var w4 = new Wire(new Vector2i(32, 24), new Vector2i(34, 24));
        //     // w4.RootNode.Children[0].AddChild(new WireNode(new Vector2i(34, 21)));
        //     // s.AddWire(w4);

        //     // s.AddComponent(new Architecture.Switch(new IOMapping(new IOGroup("g1", ComponentSide.TOP, 0))), new Vector2i(34, 25));
        // });


        // simTickTask = new Task(async () =>
        // {
        //     Stopwatch sw = new();
        //     sw.Start();

        //     while (true)
        //     {
        //         long start = sw.Elapsed.Ticks;
        //         this.simTickMutex.WaitOne();
        //         sim.LockedAction(s =>
        //         {
        //             s.Tick();
        //         });

        //         int targetTps = this.tickRates[this.selectedTickRate];
        //         long targetDiff = TimeSpan.TicksPerSecond / targetTps;

        //         this.simTickMutex.ReleaseMutex();

        //         while (sw.Elapsed.Ticks < start + targetDiff)
        //         {
        //             await Task.Delay(TimeSpan.FromTicks(targetDiff / 10));
        //         }

        //         long diff = sw.Elapsed.Ticks - start;
        //         double seconds = diff / (double)TimeSpan.TicksPerSecond;
        //         this.ticksPerSecond = this.ticksPerSecond + (1f / (float)seconds - this.ticksPerSecond) * (0.8f / MathF.Sqrt(targetTps));
        //     }
        // });
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
        var collection = new DirectoryCollectionProvider(@"C:\Users\RichieZ\repos\logix\assets\core", factory);
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
            ComponentDescription.RegisterComponentTypes();

            tab = new EditorTab("TestTab");
            allContentLoaded = true;
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
                coreLoaded = true;
                Console.WriteLine($"Loaded core content!");
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
            });
        };

        TextureRenderer.InitGL();
        PrimitiveRenderer.InitGL(500);
        TextRenderer.InitGL();

        GUI.Init("content_1.font.default", "content_1.shader_program.text");

        DisplayManager.ReleaseGLContext();
        _ = ContentManager.LoadAsync();
    }

    public override void Update()
    {
        if (allContentLoaded)
        {
            tab.Update();
        }
    }

    private EditorTab tab;

    public override void Render()
    {
        DisplayManager.LockedGLContext(() =>
        {
            if (!allContentLoaded)
            {
                if (coreLoaded)
                {
                    // TODO: Render loading screen
                    Framebuffer.BindDefaultFramebuffer();
                    Framebuffer.Clear(ColorF.Black);

                    var shader = ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.text");
                    var font = ContentManager.GetContentItem<Font>("content_1.font.default");

                    var measure = font.MeasureString("Loading...", 2f);

                    TextRenderer.RenderText(shader, font, "Loading...", DisplayManager.GetWindowSizeInPixels() / 2f - measure / 2f, 2f, ColorF.White, Framebuffer.GetDefaultCamera());
                    DisplayManager.SwapBuffers(-1);
                }
            }
            else
            {
                Framebuffer.BindDefaultFramebuffer();
                Framebuffer.Clear(ColorF.Transparent);

                // TODO: Render editor
                tab.Render();

                DisplayManager.SwapBuffers(-1);
            }
        });
    }

    public override void Unload()
    {
        ContentManager.UnloadAllContent();
    }
}