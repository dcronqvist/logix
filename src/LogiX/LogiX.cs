using System.Drawing;
using System.Numerics;
using GoodGame.Content;
using GoodGame.Content.Scripting;
using GoodGame.Graphics;
using GoodGame.Graphics.UI;
using GoodGame.Rendering;
using Symphony;
using Symphony.Common;
using static GoodGame.OpenGL.GL;

namespace GoodGame;
public class GoodGame : Game
{
    public static ContentManager<ContentMeta> ContentManager { get; private set; }
    Camera2D cam;

    public override void Initialize(string[] args)
    {
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

    float slider1 = 0;
    float slider2 = 0;
    string string1 = "";
    string string2 = "";
    bool bool1 = false;
    bool bool2 = false;
    int selected1 = 0;
    int selected2 = 0;
    string[] options = new string[] { "Option 1", "Option 2", "Option 3" };

    public override void Render()
    {
        DisplayManager.LockedGLContext(() =>
        {
            glClearColor(0.1f, 0.2f, 0.3f, 1);
            glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

            var shader = ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.texture");

            if (shader is null)
            {
                return;
            }

            GUI.Begin(this.cam);

            if (GUI.Button("hej 1", new Vector2(100, 100), new Vector2(50, 30)))
            {
                Console.WriteLine("CLICKED 1");
            }
            if (GUI.Button("hej 2", new Vector2(160, 100), new Vector2(50, 30)))
            {
                Console.WriteLine("CLICKED 2");
            }
            if (GUI.Button("hej 3", new Vector2(220, 100), new Vector2(50, 30)))
            {
                Console.WriteLine("CLICKED 3");
            }
            if (GUI.Button("hej 4", new Vector2(280, 100), new Vector2(50, 30)))
            {
                Console.WriteLine("CLICKED 4");
            }

            if (GUI.Slider("slider 1", new Vector2(100, 200), new Vector2(200, 30), ref slider1))
            {
                Console.WriteLine($"slider 1: {MathF.Round(slider1, 2)}");
            }

            if (GUI.Slider($"slider 2, {MathF.Round(slider2, 2)}", new Vector2(100, 250), new Vector2(200, 30), ref slider2))
            {
                Console.WriteLine($"slider 2: {MathF.Round(slider2, 2)}");
            }

            if (GUI.TextField("username", new Vector2(100, 300), new Vector2(200, 30), ref string1))
            {
                Console.WriteLine($"text field 1 submitted: {string1}");
            }
            if (GUI.TextField("password", new Vector2(100, 350), new Vector2(200, 30), ref string2, GUI.TextFieldFlags.Password))
            {
                Console.WriteLine($"text field 2 submitted: {string2}");
            }

            GUI.Checkbox("checkbox 1", new Vector2(100, 400), new Vector2(30, 30), ref bool1);
            GUI.Checkbox("checkbox 2", new Vector2(100, 450), new Vector2(30, 30), ref bool2);

            if (GUI.Dropdown(new Vector2(100, 500), new Vector2(200, 30), options, ref selected1))
            {
                Console.WriteLine($"dropdown 1 selected: {options[selected1]}");
            }

            if (GUI.Dropdown(new Vector2(320, 500), new Vector2(200, 30), options, ref selected2))
            {
                Console.WriteLine($"dropdown 2 selected: {options[selected2]}");
            }

            DisplayManager.SetWindowTitle($"HOT: {GUI._hotID}, ACTIVE: {GUI._activeID}, KBD: {GUI._kbdFocusID}, _CARET: {GUI._caretPosition}, DROP: {GUI._showingDropdownID}");

            GUI.End();

            DisplayManager.SwapBuffers(-1);
        });
    }

    public override void Unload()
    {
        ContentManager.UnloadAllContent();
    }
}