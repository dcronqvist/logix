using LogiX.Architecture;
using LogiX.Architecture.BuiltinComponents;
using LogiX.Architecture.Serialization;
using static LogiX.Utilities;

namespace LogiX.Tests;

public class Wiring
{
    [Fact]
    public void RandomWireAddAndRemoveNoFail()
    {
        var sim = Utilities.GetEmptySimulation();

        List<(Vector2i, Vector2i)> segments = new List<(Vector2i, Vector2i)>();
        int amountOfSegments = 5;

        var seed = 624565151;//Utilities.GetRandomSeed();
        var rand = Utilities.GetRandom(seed);

        var startPos = new Vector2i(rand.Next(0, 100), rand.Next(0, 100));
        for (int i = 0; i < amountOfSegments; i++)
        {
            var endPos = rand.Next(0, 4) switch
            {
                0 => new Vector2i(startPos.X + rand.Next(10, 100), startPos.Y),
                1 => new Vector2i(startPos.X - rand.Next(10, 100), startPos.Y),
                2 => new Vector2i(startPos.X, startPos.Y + rand.Next(10, 100)),
                3 => new Vector2i(startPos.X, startPos.Y - rand.Next(10, 100)),
                _ => throw new Exception("Invalid random number")
            };

            segments.Add((startPos, endPos));
            startPos = endPos;
        }

        foreach (var segment in segments)
        {
            sim.ConnectPointsWithWire(segment.Item1, segment.Item2);
        }

        Assert.True(sim.Wires.Count == 1, $"There should be 1 wire after connecting all of them, found {sim.Wires.Count}. Seed: {seed}");

        foreach (var segment in segments)
        {
            sim.DisconnectPoints(segment.Item1, segment.Item2);
        }

        Assert.True(sim.Wires.Count == 0, $"There should be no wires left after disconnecting all of them. Seed: {seed}");
    }
}