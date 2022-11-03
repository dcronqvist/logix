using Symphony;
using LogiX.Content;
using Symphony.Common;

namespace LogiX;

public class Program
{
    public static void Main(string[] args)
    {
        var game = new LogiX();
        game.Run(1280, 720, "GoodGame", args);
    }
}
