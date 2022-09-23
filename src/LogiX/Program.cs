using Symphony;
using GoodGame.Content;
using Symphony.Common;

namespace GoodGame;

public class Program
{
    public static void Main(string[] args)
    {
        var game = new GoodGame();
        game.Run(1280, 720, "GoodGame", args);
    }
}
