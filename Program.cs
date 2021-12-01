using Raylib_cs;
using ImGuiNET;
using System;
using System.Linq;
using System.Numerics;
using LogiX.Editor;

namespace Testing;

class Program
{
    static void Main(string[] args)
    {
        Editor editor = new Editor();
        editor.Run(1280, 720, "LogiX", 144);
    }
}