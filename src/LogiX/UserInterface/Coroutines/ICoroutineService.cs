using System;
using System.Collections;
using LogiX.Graphics;

namespace LogiX.UserInterface.Coroutines;

public record CoroutineHandle
{
    private readonly ICoroutineService _service;
    private readonly IEnumerator _coroutine;

    public bool IsRunning => _coroutine is not null && _service.IsRunning(_coroutine);

    internal CoroutineHandle(ICoroutineService service, IEnumerator coroutine)
    {
        _service = service;
        _coroutine = coroutine;
    }

    public bool Stop() => IsRunning && _service.Stop(_coroutine);
    public IEnumerator WaitForCompletion()
    {
        if (_coroutine is not null)
        {
            while (_service.IsRunning(_coroutine))
                yield return null;
        }

        yield break;
    }
}

public interface ICoroutineService
{
    int RunningCount { get; }

    CoroutineHandle Run(float timeUntilStart, IEnumerator coroutine);
    CoroutineHandle Run(IEnumerator coroutine);
    bool Stop(IEnumerator routine);
    bool Stop(CoroutineHandle handle);
    void StopAll();
    bool IsRunning(IEnumerator routine);
    bool IsRunning(CoroutineHandle handle);
    bool Update(float deltaTime);
    void Render(IRenderer renderer, float deltaTime, float totalTime);
}

public interface ICoroutineRenderer
{
    void Render(IRenderer renderer, float deltaTime, float totalTime);
}

public class LambdaCoroutineRenderer(Action<IRenderer, float, float> render) : ICoroutineRenderer
{
    public void Render(IRenderer renderer, float deltaTime, float totalTime) => render(renderer, deltaTime, totalTime);
}
