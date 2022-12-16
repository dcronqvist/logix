using LogiX.Architecture;
using LogiX.Architecture.BuiltinComponents;
using LogiX.Architecture.Serialization;
using LogiX.Minimal;
using static LogiX.Utilities;

namespace LogiX.Tests.Minimal;

[Collection("Tests Collection")]
public class TestCases
{
    [Fact]
    public void EmptyProjectPrintTest()
    {
        var basePath = Path.GetFullPath(@"../../../Minimal/test-projects");
        var minimal = new MinimalLogiX(Utilities.Arrayify("simulate", $"{basePath}/empty.lxprojj", "main", "-a", $"{basePath}/empty.txt"));

        var output = minimal.RunAndGetOutput();
        Assert.Contains("This is a test message!", output);
    }

    [Fact]
    public void TestBenEaterExample()
    {
        var basePath = Path.GetFullPath(@"../../../../../examples/beneater-8bit");
        var minimal = new MinimalLogiX(Utilities.Arrayify("simulate", $"{basePath}/beneater.lxprojj", "main", "-a", $"{basePath}/run.txt"));

        var output = minimal.RunAndGetOutput();
        Assert.Contains(@"Circuit has been reset and clock is enabled!
Waiting for processor to HALT
Done! OUTPUT=0xFF
Executed 1023 instructions!", output);
    }
}