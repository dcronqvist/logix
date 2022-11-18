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
        if (args.Length == 0)
        {
            var game = new LogiX();
            game.Run(1280, 720, "LogiX", args);
        }
        else
        {
            var minimal = new MinimalLogiX(args);
            minimal.Run();
        }
    }
}
