using System.Runtime.CompilerServices;

namespace LogiX.Architecture;

// Makes one variable thread safe
public class ThreadSafe<T>
{
    // The data & lock
    private T _value;
    private object _lock = new object();

    public string LastLockedMemberName { get; private set; }
    public string LastLockedFileName { get; private set; }
    public int LastLockedLineNumber { get; private set; }

    // How to get & set the data
    public T Value
    {
        get
        {
            lock (_lock)
                return _value;
        }

        set
        {
            lock (_lock)
                _value = value;
        }
    }

    // Initializes the value
    public ThreadSafe(T value = default(T))
    {
        _value = value;
        Value = value;
    }

    public void LockedAction(Action<T> action, Action<Exception> onError = null, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
    {
        try
        {
            lock (_lock)
            {
                LastLockedMemberName = memberName;
                LastLockedFileName = sourceFilePath;
                LastLockedLineNumber = sourceLineNumber;
                action(_value);
            }

        }
        catch (Exception e)
        {
            onError?.Invoke(e);
        }
        finally
        {
            LastLockedMemberName = null;
            LastLockedFileName = null;
            LastLockedLineNumber = 0;
        }
    }

    public TReturn LockedAction<TReturn>(Func<T, TReturn> action, Action<Exception> onError = null, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
    {
        try
        {
            TReturn result = default(TReturn);

            lock (_lock)
            {
                LastLockedMemberName = memberName;
                LastLockedFileName = sourceFilePath;
                LastLockedLineNumber = sourceLineNumber;

                result = action(_value);
            }

            LastLockedMemberName = null;
            LastLockedFileName = null;
            LastLockedLineNumber = 0;

            return result;
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
        }
        finally
        {
            LastLockedMemberName = null;
            LastLockedFileName = null;
            LastLockedLineNumber = 0;
        }
        return default(TReturn);
    }
}