namespace LogiX.Architecture;

public class Editor
{
    public Simulation Simulation { get; private set; }

    public Editor(Simulation simulation)
    {
        this.Simulation = simulation;
    }
}