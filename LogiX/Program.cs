using LogiX.Display;
using Raylib_cs;
using System;

namespace LogiX
{
    class Program
    {
        static void Main(string[] args)
        {
            // Creates the LogiX window and then runs the program.
            BaseWindow logix = new LogiXWindow();
            logix.Run();
        }
    }
}
