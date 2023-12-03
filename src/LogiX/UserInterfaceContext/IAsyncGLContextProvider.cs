using System;
using System.Threading.Tasks;

namespace LogiX.UserInterfaceContext;

public interface IAsyncGLContextProvider
{
    Task<T> PerformInGLContext<T>(Func<T> action);
    Task PerformInGLContext(Action action);
    void ProcessActions();
}
