using System;
using System.Collections.Generic;

namespace LogiX.Observables;

public sealed class AnonymousObserver<T>(Action<T> onNext, Action<Exception> onError, Action onCompleted) : IObserver<T>
{
    private readonly Action<T> _onNext = onNext;
    private readonly Action<Exception> _onError = onError;
    private readonly Action _onCompleted = onCompleted;

    public void OnCompleted() => _onCompleted();
    public void OnError(Exception error) => _onError(error);
    public void OnNext(T value) => _onNext(value);
}

public abstract class Observable<T> : IObservable<T>
{
    private readonly List<IObserver<T>> _observers = [];

    public IDisposable Subscribe(IObserver<T> observer)
    {
        if (!_observers.Contains(observer))
            _observers.Add(observer);

        return new Unsubscriber(_observers, observer);
    }

    public IDisposable Subscribe(Action<T> onNext, Action<Exception> onError, Action onCompleted) =>
        Subscribe(new AnonymousObserver<T>(onNext, onError, onCompleted));

    private sealed class Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer) : IDisposable
    {
        private readonly List<IObserver<T>> _observers = observers;
        private readonly IObserver<T> _observer = observer;

        public void Dispose() => _ = _observers.Remove(_observer);
    }

    public void NotifyObserversOfError(Exception error) => _observers.ForEach(o => o.OnError(error));
    public void NotifyObservers(T value) => _observers.ForEach(o => o.OnNext(value));

    public void NotifyObserversOfCompletion()
    {
        _observers.ForEach(o => o.OnCompleted());
        _observers.Clear();
    }

    public void SetPropertyAndNotify<TProp>(ref TProp property, TProp value, T notificationValue)
    {
        property = value;
        NotifyObservers(notificationValue);
    }

    public void SetDictionaryPropAndNotify<TKey, TValue>(ref Dictionary<TKey, TValue> dict, TKey key, TValue value, T notificationValue)
    {
        dict[key] = value;
        NotifyObservers(notificationValue);
    }
}
