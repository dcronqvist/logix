using LogiX.Architecture;
using LogiX.Architecture.Serialization;

namespace LogiX.Tests;

public static class Utilities
{
    public static Simulation GetEmptySimulation()
    {
        return new Simulation();
    }

    public static Random GetRandom(int seed)
    {
        return new Random(seed);
    }

    private static Random _random = new Random();
    public static int GetRandomSeed()
    {
        return _random.Next();
    }
}