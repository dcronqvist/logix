using LogiX.Architecture;
using LogiX.Architecture.BuiltinComponents;
using LogiX.Architecture.Serialization;
using LogiX.Minimal;
using static LogiX.Utilities;

namespace LogiX.Tests.Minimal;

[Collection("Tests Collection")]
public class TestCases
{
    // [Fact]
    // public void EmptyProjectPrintTest()
    // {
    //     var basePath = Path.GetFullPath($"{Utilities.GetRootDir()}/src/LogiX.Tests/Minimal/test-projects");
    //     var writer = new StringWriter();
    //     var minimal = new MinimalLogiX(writer, Utilities.Arrayify("simulate", $"{basePath}/empty.lxprojj", "main", "-a", $"{basePath}/empty.txt"));

    //     minimal.Run(false);

    //     var output = writer.ToString();
    //     Assert.Contains("This is a test message!", output);
    // }

    // [Fact]
    // public void TestBenEaterExample()
    // {
    //     var basePath = Path.GetFullPath($"{Utilities.GetRootDir()}/examples/beneater-8bit");
    //     var writer = new StringWriter();
    //     var minimal = new MinimalLogiX(writer, Utilities.Arrayify("simulate", $"{basePath}/beneater.lxprojj", "main", "-a", $"{basePath}/run.txt"));

    //     minimal.Run(false);

    //     var output = writer.ToString();
    //     Assert.Contains(@"Circuit has been reset and clock is enabled!", output);
    //     Assert.Contains(@"Waiting for processor to HALT", output);
    //     Assert.Contains(@"Done! OUTPUT=0xFF", output);
    //     Assert.Contains(@"Executed 1023 instructions!", output);
    // }
}