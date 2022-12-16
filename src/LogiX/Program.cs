using Symphony;
using LogiX.Content;
using Symphony.Common;
using System.CommandLine;
using LogiX.Architecture.Serialization;
using LogiX.Minimal;

namespace LogiX;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "run")
        {
            // Remove the first argument
            args = args.Skip(1).ToArray();
        }

        if (args.Length == 0)
        {
            var game = new LogiXWindow();
            game.Run("LogiX", args);
        }
        else
        {
            var minimal = new MinimalLogiX(args);
            minimal.Run();
        }
    }
}
