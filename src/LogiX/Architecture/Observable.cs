namespace LogiX.Architecture;

public abstract class Observable
{
    protected List<Observer> Observers { get; set; } = new();

    public void AddObserver(Observer observer)
    {
        this.Observers.Add(observer);
    }

    public void RemoveObserver(Observer observer)
    {
        this.Observers.Remove(observer);
    }

    public void ClearObservers()
    {
        this.Observers.Clear();
    }

    public void NotifyObservers()
    {
        this.Observers.ForEach(o => o.Update());
    }

    public void NotifyObservers(params Observer[] skip)
    {
        this.NotifyObservers((IEnumerable<Observer>)skip);
    }

    public void NotifyObservers(IEnumerable<Observer> skip)
    {
        this.Observers.Except(skip).ToList().ForEach(o => o.Update());
    }
}