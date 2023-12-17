using System.Numerics;
using System.Threading.Tasks;
using DotGL;
using DotGLFW;
using LogiX.Content;
using LogiX.Debug.Logging;
using LogiX.Graphics;
using LogiX.Graphics.Cameras;
using LogiX.Graphics.Framebuffers;
using LogiX.Graphics.Rendering;
using LogiX.Graphics.Textures;
using LogiX.UserInterface.Views;
using LogiX.UserInterface.Views.EditorView;
using LogiX.UserInterfaceContext;
using Symphony;

namespace LogiX;

public class ApplicationLogic : IApplicationLogic
{
    private readonly ILog _log;
    private readonly IUserInterfaceContext<InputState, Keys, ModifierKeys, MouseButton> _userInterfaceContext;
    private readonly IAsyncGLContextProvider _asyncGLContextProvider;
    private readonly IContentManager<ContentMeta> _contentManager;
    private readonly IImGuiController _imGuiController;
    private readonly IFactory<LoadingView> _viewLoadingFactory;
    private readonly IFactory<EditorView> _viewEditorFactory;
    private readonly TextureRenderer _textureRenderer;

    private IView _currentView;

    private readonly IFramebuffer _currentViewFramebuffer;
    private readonly IFramebuffer _currentViewGUIFramebuffer;

    private readonly IFramebuffer _defaultFramebuffer;
    private readonly ICamera2D _defaultCamera;

    public ApplicationLogic(
        ILog log,
        IUserInterfaceContext<InputState, Keys, ModifierKeys, MouseButton> userInterfaceContext,
        IAsyncGLContextProvider asyncGLContextProvider,
        IContentManager<ContentMeta> contentManager,
        IImGuiController imGuiController,
        IFactory<LoadingView> viewLoadingFactory,
        IFactory<EditorView> viewEditorFactory,
        TextureRenderer textureRenderer)
    {
        _log = log;
        _userInterfaceContext = userInterfaceContext;
        _asyncGLContextProvider = asyncGLContextProvider;
        _contentManager = contentManager;
        _imGuiController = imGuiController;
        _viewLoadingFactory = viewLoadingFactory;
        _viewEditorFactory = viewEditorFactory;
        _textureRenderer = textureRenderer;

        // Size that follows window size
        var framebufferSize = new ComputedValue<Vector2>(() => _userInterfaceContext.GetWindowSizeAsVector2());

        _defaultFramebuffer = new Framebuffer.DefaultFramebuffer(_userInterfaceContext);
        _defaultCamera = new Camera2D(framebufferSize, new ComputedValue<Vector2>(() => framebufferSize.Get() / 2f), new FixedValue<float>(1));

        _currentViewFramebuffer = new Framebuffer(framebufferSize);
        _currentViewGUIFramebuffer = new Framebuffer(framebufferSize);

        _contentManager.StartedLoading += (sender, e) => _log.LogMessage(LogLevel.Info, $"Started loading.");
        _contentManager.FinishedLoading += (sender, e) =>
        {
            _log.LogMessage(LogLevel.Info, $"Finished loading.");

            _ = _asyncGLContextProvider.PerformInGLContext(() => _currentView = _viewEditorFactory.Create());
        };
        _contentManager.StageStarted += (sender, e) => _log.LogMessage(LogLevel.Info, $"Started stage {e.Stage.StageName}.");
        _contentManager.StageFinished += (sender, e) => _log.LogMessage(LogLevel.Info, $"Finished stage {e.Stage.StageName}.");
        _contentManager.ContentItemSuccessfullyLoaded += (sender, e) =>
        {
            _log.LogMessage(LogLevel.Info, $"Loaded {e.Item.Identifier}.");
            if (e.Item.Identifier == "logix-core:textures/icon.png")
            {
                var iconTexture = e.GetContent<Texture2D>("logix-core:textures/icon.png");

                _ = _asyncGLContextProvider.PerformInGLContext(() => _userInterfaceContext.SetWindowIcon(iconTexture));
            }
        };
        _contentManager.ContentItemFailedToLoad += (sender, e) => _log.LogMessage(LogLevel.Error, $"Failed to load {e.ItemIdentifier}: {e.Error}");
        _contentManager.ContentItemReloaded += (sender, e) => _log.LogMessage(LogLevel.Info, $"Reloaded {e.Item.Identifier}.");
    }

    public void Initialize()
    {
        _contentManager.StageFinished += (sender, e) =>
        {
            if (e.Stage.StageName == "Core")
            {
                // Core content loaded, can transition into showing "loading screen"
                _ = _asyncGLContextProvider.PerformInGLContext(() => _currentView = _viewLoadingFactory.Create());
            }
        };

        _ = Task.Run(() => _contentManager.LoadAsync());
    }

    public void Frame(float deltaTime, float totalTime)
    {
        if (_currentView is null)
            return;

        _currentView.Update(deltaTime, totalTime);

        _currentViewFramebuffer.Bind(() => _currentView.Render(deltaTime, totalTime));

        _currentViewGUIFramebuffer.Bind(() =>
        {
            GL.glClearColor(0, 0, 0, 0);
            GL.glClear(GL.GL_COLOR_BUFFER_BIT);

            _imGuiController.Update(deltaTime);

            _currentView.SubmitGUI(deltaTime, totalTime);

            _imGuiController.Render();
        });

        _defaultFramebuffer.Bind(() =>
        {
            GL.glClearColor(0, 0, 0, 0);
            GL.glClear(GL.GL_COLOR_BUFFER_BIT);

            GL.glEnable(GL.GL_BLEND);
            GL.glBlendFunc(GL.GL_SRC_ALPHA, GL.GL_ONE_MINUS_SRC_ALPHA);

            var defaultShader = _contentManager.GetContent<ShaderProgram>("logix-core:shaders/textures.shader");

            var currentViewTexture = _currentViewFramebuffer.GetUnderlyingTexture2D();

            _textureRenderer.Render(
                defaultShader,
                currentViewTexture,
                Vector2.Zero,
                Vector2.One,
                0f,
                ColorF.White,
                Vector2.Zero,
                new System.Drawing.RectangleF(0, 0, currentViewTexture.Width, currentViewTexture.Height),
                _defaultCamera,
                TextureRenderEffects.FlipVertical
            );

            var currentViewGUITexture = _currentViewGUIFramebuffer.GetUnderlyingTexture2D();

            _textureRenderer.Render(
                defaultShader,
                currentViewGUITexture,
                Vector2.Zero,
                Vector2.One,
                0f,
                ColorF.White,
                Vector2.Zero,
                new System.Drawing.RectangleF(0, 0, currentViewGUITexture.Width, currentViewGUITexture.Height),
                _defaultCamera,
                TextureRenderEffects.FlipVertical
            );
        });
    }

    public void Unload()
    {

    }
}
