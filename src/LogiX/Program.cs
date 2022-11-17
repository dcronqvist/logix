using Symphony;
using LogiX.Content;
using Symphony.Common;
using System.CommandLine;
using LogiX.Architecture.Serialization;

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
            // HERE WE WANT TO START A MINIMALISTIC CONSOLE APPLICATION
            // THAT WILL BE ABLE TO LOAD PROJECTS AND RUN SIMULATIONS ON CIRCUITS, UNTIL QUIT.

            // YOU SHOULD ALSO BE ABLE TO TEST CIRCUITS IN PROJECTS BY RUNNING THEM IN THE CONSOLE APP.
            // SPECIFYING THE PROJECT AND THE CIRCUIT TO RUN, TOGETHER WITH THE VALUES OF INPUTS AND EXPECTED VALUES
            // OF OUTPUTS.

            // A GOOD WAY MIGHT BE TO SPECIFY SOME KIND OF GENERIC WAY TO "INTERACT" WITH COMPONENTS IN THE CIRCUIT FROM
            // A SPECIFICATION IN A FILE, LIKE "PRESS BUTTON RESET FOR 100 TICKS", "SET PIN AUTO_CLK TO [HIGH]"

            // logix noguisim project.lxprojj circuit1 --interactions inters.json

            // logix test project.lxproj circuit1 --test-case test.json
        }
    }
}
