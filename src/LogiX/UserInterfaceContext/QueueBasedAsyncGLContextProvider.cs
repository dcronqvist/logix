using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LogiX.UserInterfaceContext;

public class QueueBasedAsyncGLContextProvider : IAsyncGLContextProvider
{
    private readonly Queue<Action> _actions = new();
    private readonly object _lock = new();

    public async Task<T> PerformInGLContext<T>(Func<T> action)
    {
        var tcs = new TaskCompletionSource<T>();

        lock (_lock)
        {
            _actions.Enqueue(() =>
            {
                try
                {
                    T result = action();
                    tcs.SetResult(result);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });
        }

        return await tcs.Task;
    }

    public Task PerformInGLContext(Action action)
    {
        var tcs = new TaskCompletionSource<bool>();

        lock (_lock)
        {
            _actions.Enqueue(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });
        }

        return tcs.Task;
    }

    public void ProcessActions()
    {
        lock (_lock)
        {
            while (_actions.Count > 0)
            {
                var action = _actions.Dequeue();
                action();
            }
        }
    }
}
