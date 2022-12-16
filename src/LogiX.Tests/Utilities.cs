using LogiX.Architecture;
using LogiX.Architecture.BuiltinComponents;
using LogiX.Architecture.Serialization;
using LogiX.Content;
using LogiX.Content.Scripting;
using LogiX.Minimal;
using Symphony;
using Symphony.Common;

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

    public static T[] Arrayify<T>(params T[] vals)
    {
        return vals;
    }

    public static Circuit GetEmptyCircuit()
    {
        return new Circuit("");
    }

    public static Circuit AddNode(this Circuit circuit, NodeDescription node, INodeDescriptionData data, Vector2i position, int rotation)
    {
        node.Data = data;
        node.Position = position;
        node.Rotation = rotation;
        circuit.Nodes.Add(node);
        return circuit;
    }

    public static Circuit Connect(this Circuit circuit, Guid node1, string pin1, Guid node2, string pin2)
    {
        var sim = Simulation.FromCircuit(circuit);

        var pin1Node = sim.GetNodeFromID(node1);
        var pin2Node = sim.GetNodeFromID(node2);

        var pin1Collection = sim.Scheduler.GetPinCollectionForNode(pin1Node);
        var pin2Collection = sim.Scheduler.GetPinCollectionForNode(pin2Node);

        var pin1Pos = pin1Node.GetPinPosition(pin1Collection, pin1);
        var pin2Pos = pin2Node.GetPinPosition(pin2Collection, pin2);

        var cornerRequired = pin1Pos.X != pin2Pos.X && pin1Pos.Y != pin2Pos.Y;

        if (cornerRequired)
        {
            var cornerPos = new Vector2i(pin1Pos.X, pin2Pos.Y);
            sim.ConnectPointsWithWire(pin1Pos, cornerPos);
            sim.ConnectPointsWithWire(cornerPos, pin2Pos);
        }
        else
        {
            sim.ConnectPointsWithWire(pin1Pos, pin2Pos);
        }

        return sim.GetCircuitInSimulation("");
    }

    public static Circuit AddPin(this Circuit circuit, Vector2i position, int rotation, int bits, out Guid pinID, bool isInput = true, params LogicValue[] values)
    {
        var pin = NodeDescription.CreateDefaultNodeDescription("logix_builtin.script_type.PIN");
        var pinData = (NodeDescription.CreateDefaultNodeDescriptionData("logix_builtin.script_type.PIN") as PinData)!;

        if (bits != values.Length)
        {
            throw new ArgumentException("Bits and values must be the same length.");
        }

        pinData.Bits = bits;
        pinData.Values = values;
        pinData.Behaviour = isInput ? PinBehaviour.INPUT : PinBehaviour.OUTPUT;

        circuit = circuit.AddNode(pin, pinData, position, rotation);
        pinID = pin.ID;
        return circuit;
    }

    public static LogicValue[] ReadPin(this Simulation sim, Guid pinID, int bits)
    {
        var pinNode = sim.GetNodeFromID(pinID);
        var data = (pinNode.GetNodeData() as PinData)!;

        return data.Values;
    }
}

public class TestsFixture : IDisposable
{
    public ContentManager<ContentMeta> ContentManager { get; private set; }

    public TestsFixture()
    {
        // Do "global" initialization here; Only called once.
        var basePath = @"../../../../../assets";
        var corePath = Path.GetFullPath($"{basePath}/core");

        var coreSource = new DirectoryContentSource(corePath);
        var validator = new ContentValidator();
        var collection = IContentCollectionProvider.FromListOfSources(coreSource); //new DirectoryCollectionProvider(@"C:\Users\RichieZ\repos\logix\assets\core", factory);
        var loader = new MinimalContentLoader();

        var config = new ContentManagerConfiguration<ContentMeta>(validator, collection, loader);
        ContentManager = new ContentManager<ContentMeta>(config);
        ContentManager.Load();

        ScriptManager.Initialize(ContentManager);
        NodeDescription.RegisterNodeTypes();

        LogiX.Utilities.ContentManager = ContentManager;
    }

    public void Dispose()
    {
        // Do "global" teardown here; Only called once.
    }
}

[CollectionDefinition("Tests Collection")]
public class TestsCollection : ICollectionFixture<TestsFixture>
{
}