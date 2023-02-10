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

    public IEnumerable<T> NotifyObservers(Observer<T> origin)
    {
        return this.Observers.Select(o => o.Update(origin));
    }

    public IEnumerable<T> NotifyObservers(Observer<T> origin, params Observer<T>[] skip)
    {
        return this.NotifyObservers(origin, (IEnumerable<Observer<T>>)skip);
    }

    public IEnumerable<T> NotifyObservers(Observer<T> origin, IEnumerable<Observer<T>> skip)
    {
        return this.Observers.Except(skip).ToList().Select(o => o.Update(origin));
    }
}