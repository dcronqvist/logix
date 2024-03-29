using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using LogiX.Architecture;
using LogiX.Architecture.Plugins;
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

public class LogiXWindow : Game
{
    public static ContentManager<ContentMeta> ContentManager { get; private set; }
    bool allContentLoaded = false;
    bool coreLoaded = false;
    public Editor Editor { get; private set; }
    private string _loadingUnderstring = "";

    public override (Vector2i, bool) Initialize(string[] args)
    {
        Settings.LoadSettings();
        var settingsWindowSize = Settings.GetSetting<Vector2i>(Settings.WINDOW_SIZE);
        var settingsFullscreen = Settings.GetSetting<bool>(Settings.WINDOW_FULLSCREEN);

        DisplayManager.OnFramebufferResize += (sender, e) =>
        {
            Settings.SetSetting(Settings.WINDOW_SIZE, new Vector2i((int)e.X, (int)e.Y));
        };

        DisplayManager.OnToggleFullscreen += (sender, e) =>
        {
            _ = Settings.SetSettingAsync(Settings.WINDOW_FULLSCREEN, e);
        };

        return (settingsWindowSize, settingsFullscreen);
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
            else if (Directory.Exists(path))
            {
                return new DirectoryContentSource(path);
            }

            return null;
        };

        var basePath = AppDomain.CurrentDomain.BaseDirectory + "/assets";

        var coreSource = new DirectoryContentSource(Path.GetFullPath($"{basePath}/core"));
        var pluginSources = new Symphony.Common.DirectoryCollectionProvider(Path.GetFullPath($"{basePath}/plugins/"), factory);
        var validator = new ContentValidator();
        var collection = IContentCollectionProvider.FromListOfSources(pluginSources.GetModSources().Prepend(coreSource)); //new DirectoryCollectionProvider(@"C:\Users\RichieZ\repos\logix\assets\core", factory);
        var loader = new ContentLoader();

        var config = new ContentManagerConfiguration<ContentMeta>(validator, collection, loader);
        ContentManager = new ContentManager<ContentMeta>(config);

        ContentManager.StartedLoading += (sender, e) =>
        {
            Console.WriteLine($"Started Loading!");
            _loadingUnderstring = "Loading assets...";
        };

        ContentManager.FinishedLoading += (sender, e) =>
        {
            Console.WriteLine($"Finished Loading!");

#if DEBUG // Only start the polling if we are in DEBUG mode
            // The following background task will poll for content that is changed and then reloads it
            Task.Run(async () =>
            {
                while (true)
                {
                    await ContentManager.PollForSourceUpdates();
                    await Task.Delay(1000); // No more than 1 check per second is needed
                }
            });
#endif

            _loadingUnderstring = "Initializing scripts...";
            ScriptManager.Initialize(ContentManager);
            _loadingUnderstring = "Initializing plugins...";
            PluginManager.LoadPlugins(ContentManager);
            _loadingUnderstring = "Registering components...";
            NodeDescription.RegisterNodeTypes();

            _loadingUnderstring = "Starting editor...";
            this.Editor = new Editor();
            allContentLoaded = true;
            _loadingUnderstring = "Finished!";
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
                var tex = e.CurrentlyLoaded.GetContentItem<Texture2D>("logix_core:core/icon.png");
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
        };

        ContentManager.ContentItemReloaded += (sender, e) =>
        {
            Console.WriteLine($"Reloaded {e.Entry.EntryPath} in {e.Stage.StageName}!");
        };

        var sources = ContentManager.CollectValidSources().ToList();
        Console.WriteLine($"Found {sources.Count} valid sources!");

        Utilities.ContentManager = ContentManager;

        DisplayManager.OnFramebufferResize += (sender, e) =>
        {
            DisplayManager.LockedGLContext(() =>
            {
                glViewport(0, 0, (int)e.X, (int)e.Y);
                Console.WriteLine($"Framebuffer Resized to {e.X}x{e.Y}");
            });
        };

        TextureRenderer.InitGL();
        PrimitiveRenderer.InitGL(64);
        TextRenderer.InitGL();
        BezierRenderer.InitGL();

        DisplayManager.ReleaseGLContext();
        _ = ContentManager.LoadAsync();

        // ContentManager.Load();
    }

    public override void Update()
    {
        if (allContentLoaded)
        {
            this.Editor.Update();
        }
    }

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

                    var shader = ContentManager.GetContentItem<ShaderProgram>("logix_core:shaders/text/text.shader");
                    var font = Utilities.GetFont("logix_core:core/fonts/inconsolata.fontzip");

                    float scale = 0.4f;

                    var measure = font.MeasureString("Loading...", scale);
                    var measureUnder = font.MeasureString(_loadingUnderstring, scale);

                    TextRenderer.RenderText(font, "Loading...", DisplayManager.GetWindowSizeInPixels() / 2f - measure / 2f, scale, 0f, 0.5f, 0.1f, ColorF.White);
                    TextRenderer.RenderText(font, _loadingUnderstring, DisplayManager.GetWindowSizeInPixels() / 2f - measureUnder / 2f + new Vector2(0, 40), scale, 0f, 0.5f, 0.1f, ColorF.White);

                    TextRenderer.FinalizeRender(shader, Framebuffer.GetDefaultCamera());
                    DisplayManager.SwapBuffers(-1);
                }
            }
            else
            {
                Framebuffer.BindDefaultFramebuffer();
                Framebuffer.Clear(ColorF.Transparent);

                // TODO: Render editor
                this.Editor.Render();

                DisplayManager.SwapBuffers(-1);
            }
        });
    }

    public override void Unload()
    {
        ContentManager.UnloadAllContent();
    }
}