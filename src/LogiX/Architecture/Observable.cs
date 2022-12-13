namespace LogiX.Architecture;

public abstract class Observable<T>
{
    protected List<Observer<T>> Observers { get; set; } = new();

    public void AddObserver(Observer<T> observer)
    {
        this.Observers.Add(observer);
    }

    public void RemoveObserver(Observer<T> observer)
    {
        this.Observers.Remove(observer);
    }

    public void ClearObservers()
    {
        this.Observers.Clear();
    }

    public IEnumerable<T> NotifyObservers()
    {
        return this.Observers.Select(o => o.Update());
    }

    public IEnumerable<T> NotifyObservers(params Observer<T>[] skip)
    {
        return this.NotifyObservers((IEnumerable<Observer<T>>)skip);
    }

    public IEnumerable<T> NotifyObservers(IEnumerable<Observer<T>> skip)
    {
        return this.Observers.Except(skip).ToList().Select(o => o.Update());
    }
}