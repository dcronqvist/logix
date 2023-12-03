using System.Collections;
using System.Collections.Generic;
using LogiX.Graphics;

namespace LogiX.UserInterface.Coroutines;

public class CoroutineService : ICoroutineService
{
    private readonly List<IEnumerator> _coroutines = [];
    private readonly List<ICoroutineRenderer> _coroutineRenders = [];
    private readonly List<float> _coroutineTimers = [];

    public int RunningCount => _coroutines.Count;

    public CoroutineHandle Run(float timeUntilStart, IEnumerator coroutine)
    {
        _coroutineTimers.Add(timeUntilStart);
        _coroutines.Add(coroutine);
        _coroutineRenders.Add(null);
        return new CoroutineHandle(this, coroutine);
    }

    public CoroutineHandle Run(IEnumerator coroutine) => Run(0, coroutine);

    public bool Stop(IEnumerator routine)
    {
        int index = _coroutines.IndexOf(routine);
        if (index == -1)
            return false;

        _coroutines.RemoveAt(index);
        _coroutineTimers.RemoveAt(index);
        _coroutineRenders.RemoveAt(index);
        return true;
    }

    public bool Stop(CoroutineHandle handle) => handle.Stop();

    public void StopAll()
    {
        _coroutines.Clear();
        _coroutineTimers.Clear();
        _coroutineRenders.Clear();
    }

    public bool IsRunning(IEnumerator routine) => _coroutines.Contains(routine);

    public bool IsRunning(CoroutineHandle handle) => handle.IsRunning;

    public bool Update(float deltaTime)
    {
        if (_coroutines.Count == 0)
            return false;

        for (int i = 0; i < _coroutines.Count; i++)
        {
            if (_coroutineTimers[i] > 0f)
            {
                _coroutineTimers[i] -= deltaTime;
            }
            else if (_coroutines[i] == null || !MoveNext(_coroutines[i], i))
            {
                _coroutines.RemoveAt(i);
                _coroutineTimers.RemoveAt(i);
                _coroutineRenders.RemoveAt(i);
                i--;
            }
        }

        return true;
    }

    public void Render(IRenderer renderer, float deltaTime, float totalTime)
    {
        for (int i = 0; i < _coroutineRenders.Count; i++)
        {
            _coroutineRenders[i]?.Render(renderer, deltaTime, totalTime);
        }
    }

    private bool MoveNext(IEnumerator routine, int index)
    {
        if (routine.Current is IEnumerator enumerator)
        {
            if (MoveNext(enumerator, index))
                return true;

            _coroutineTimers[index] = 0f;
        }

        if (routine.Current is ICoroutineRenderer renderer)
        {
            _coroutineRenders[index] = renderer;
        }

        bool result = routine.MoveNext();

        if (routine.Current is float v)
            _coroutineTimers[index] = v;

        return result;
    }
}
