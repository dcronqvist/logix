// using System.Drawing;
// using System.Numerics;
// using LogiX.Architecture.BuiltinComponents;
// using LogiX.Content;
// using LogiX.Graphics;
// using LogiX.Rendering;
// using Symphony;
// using Symphony.Common;
// using static LogiX.OpenGL.GL;

// namespace LogiX.Minimal;

// public class LEDMatrixWindow : Game
// {
//     public LEDMatrix Matrix { get; private set; }
//     public Keyboard Keyboard { get; private set; }
//     public Camera2D Camera { get; private set; }
//     public Simulation Simulation { get; private set; }

//     public ContentManager<ContentMeta> ContentManager { get; private set; }
//     public Vector2 WindowSize { get; private set; }
//     public int Scale { get; private set; }

//     public LEDMatrixWindow(int scale, int width, int height, Simulation simulation, LEDMatrix matrix, Keyboard keyboard)
//     {
//         this.Matrix = matrix;
//         this.Simulation = simulation;
//         this.WindowSize = new Vector2(width, height);
//         this.Scale = scale;
//         this.Keyboard = keyboard;
//     }

//     public override void Initialize(string[] args)
//     {

//     }

//     public override void LoadContent(string[] args)
//     {
//         glEnable(GL_BLEND);
//         glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

//         this.Camera = new Camera2D(this.WindowSize / 2f, 1f);

//         glViewport(0, 0, (int)this.WindowSize.X, (int)this.WindowSize.Y);
//         PrimitiveRenderer.InitGL(64);

//         var basePath = AppDomain.CurrentDomain.BaseDirectory + "/assets";

//         var coreSource = new DirectoryContentSource(Path.GetFullPath($"{basePath}/core"));
//         var validator = new ContentValidator();
//         var collection = IContentCollectionProvider.FromListOfSources(coreSource); //new DirectoryCollectionProvider(@"C:\Users\RichieZ\repos\logix\assets\core", factory);
//         var loader = new ContentLoader();

//         var config = new ContentManagerConfiguration<ContentMeta>(validator, collection, loader);
//         ContentManager = new ContentManager<ContentMeta>(config);
//         Utilities.ContentManager = ContentManager;

//         DisplayManager.ReleaseGLContext();
//         ContentManager.Load();
//         DisplayManager.AcquireGLContext();

//         var tex = ContentManager.GetContentItem<Texture2D>("core.texture.icon");
//         DisplayManager.SetWindowIcon(tex);

//         Task.Run(() =>
//         {
//             while (true)
//             {
//                 this.Simulation.Tick();
//             }
//         });

//         Input.OnChar += (sender, e) =>
//         {
//             if (this.Keyboard is not null)
//             {
//                 this.Keyboard.RegisterChar(e);
//             }
//         };
//     }

//     public override void Render()
//     {
//         var data = this.Matrix._matrix;
//         var matrixData = this.Matrix.GetDescriptionData() as LEDMatrixData;

//         var shader = ContentManager.GetContentItem<ShaderProgram>("core.shader_program.primitive");

//         Framebuffer.BindDefaultFramebuffer();
//         Framebuffer.Clear(matrixData.BackgroundColor);

//         for (int x = 0; x < data.Length; x++)
//         {
//             var column = data[x];

//             for (int y = 0; y < column.Length; y++)
//             {
//                 var value = column[y];

//                 if (value == LogicValue.HIGH)
//                 {
//                     PrimitiveRenderer.RenderRectangle(new RectangleF(x * Scale, y * Scale, this.Scale, this.Scale), Vector2.Zero, 0f, matrixData.ForegroundColor);
//                 }
//             }
//         }

//         PrimitiveRenderer.FinalizeRender(shader, this.Camera);
//         DisplayManager.SwapBuffers(-1);
//     }

//     public override void Unload()
//     {

//     }

//     public override void Update()
//     {
//         Input.Begin();

//         Input.End();
//     }
// }