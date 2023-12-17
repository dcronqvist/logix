using DotGLFW;
using LogiX.Input;
using LogiX.UserInterfaceContext;

namespace LogiX;

public class Application : IApplication
{
    private readonly IUserInterfaceContext<InputState, Keys, ModifierKeys, MouseButton> _userInterfaceContext;
    private readonly IAsyncGLContextProvider _glContextProvider;
    private readonly IApplicationLogic _appLogic;
    private readonly IKeyboard<char, Keys, ModifierKeys> _keyboard;
    private readonly IMouse<MouseButton> _mouse;

    private bool _isRunning = false;

    public Application(
        IUserInterfaceContext<InputState, Keys, ModifierKeys, MouseButton> userInterfaceContext,
        IAsyncGLContextProvider glContextProvider,
        IKeyboard<char, Keys, ModifierKeys> keyboard,
        IMouse<MouseButton> mouse,
        IApplicationLogic appLogic)
    {
        _userInterfaceContext = userInterfaceContext;
        _glContextProvider = glContextProvider;
        _keyboard = keyboard;
        _mouse = mouse;
        _appLogic = appLogic;
    }

    public void Run()
    {
        _appLogic.Initialize();

        double totalTime = 0.0;
        _isRunning = true;

        while (_isRunning)
        {
            double currentTime = _userInterfaceContext.GetTimeSinceLaunch();
            double deltaTime = currentTime - totalTime;
            totalTime = currentTime;

            _keyboard.Begin();
            _mouse.Begin();

            _glContextProvider.ProcessActions();
            _appLogic.Frame((float)deltaTime, (float)totalTime);

            _userInterfaceContext.PerformFrame();

            if (_userInterfaceContext.ContextRequestsClose())
                _isRunning = false;

            _keyboard.End();
            _mouse.End();
        }

        _appLogic.Unload();
        _userInterfaceContext.Destroy();
    }

    public void Stop()
    {
        _isRunning = false;
    }
}
