using System;
using System.Runtime.CompilerServices;

namespace LogiX;

public interface IThreadSafe<out T>
{
    void Locked(
        Action<T> action,
        Action<Exception> onError = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0);

    TRet Locked<TRet>(
        Func<T, TRet> func,
        Action<Exception> onError = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0);
}

public class ThreadSafe<T>(T initialValue) : IThreadSafe<T>
{
    private readonly T _value = initialValue;
    private readonly object _lock = new();

    private string _lockedByMemberName = "";
    private string _lockedBySourceFilePath = "";
    private int _lockedBySourceLineNumber = 0;

    public void Locked(
        Action<T> action,
        Action<Exception> onError = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        try
        {
            lock (_lock)
            {
                _lockedByMemberName = memberName;
                _lockedBySourceFilePath = sourceFilePath;
                _lockedBySourceLineNumber = sourceLineNumber;

                action(_value);
            }
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
        }
        finally
        {
            _lockedByMemberName = "";
            _lockedBySourceFilePath = "";
            _lockedBySourceLineNumber = 0;
        }
    }

    public TRet Locked<TRet>(
        Func<T, TRet> func,
        Action<Exception> onError = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        try
        {
            lock (_lock)
            {
                _lockedByMemberName = memberName;
                _lockedBySourceFilePath = sourceFilePath;
                _lockedBySourceLineNumber = sourceLineNumber;

                return func(_value);
            }
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
            return default;
        }
        finally
        {
            _lockedByMemberName = "";
            _lockedBySourceFilePath = "";
            _lockedBySourceLineNumber = 0;
        }
    }

    public override string ToString() => _value.ToString() + $" (locked by {_lockedByMemberName} in {_lockedBySourceFilePath}:{_lockedBySourceLineNumber})";
}
