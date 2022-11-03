using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;

namespace Symphony;

public static class Utilities
{
    // public static async Task<Assembly?> BuildAndGetAssemblyFromProjectFile(string pathToCSProjFile)
    // {
    //     var manager = new AnalyzerManager();
    //     manager.SetGlobalProperty("Configuration", "Release");

    //     var analyzer = manager.GetProject(pathToCSProjFile);
    //     var build = analyzer.Build();

    //     var workspace = new AdhocWorkspace();
    //     var project = build.First().AddToWorkspace(workspace);
    //     var compilation = (await project.GetCompilationAsync())!;

    //     var options = (compilation.Options as CSharpCompilationOptions)!;
    //     var outputKind = analyzer.ProjectFile.OutputType switch
    //     {
    //         "exe" => OutputKind.ConsoleApplication,
    //         "library" => OutputKind.DynamicallyLinkedLibrary,
    //         "winexe" => OutputKind.WindowsApplication,
    //         "module" => OutputKind.NetModule,
    //         _ => OutputKind.DynamicallyLinkedLibrary
    //     };

    //     options = options.WithOutputKind(outputKind);
    //     options = options.WithAllowUnsafe(true); // Always allow unsafe since there's no way to determine if the csproj file has declared it or not.
    //     compilation = compilation.WithOptions(options);

    //     if (outputKind != OutputKind.DynamicallyLinkedLibrary)
    //     {
    //         return null; // Will only allow classlibs
    //     }

    //     using (var stream = new MemoryStream())
    //     {
    //         var result = compilation.Emit(stream);

    //         if (!result.Success)
    //         {
    //             return null;
    //         }
    //         else
    //         {
    //             stream.Seek(0, SeekOrigin.Begin);
    //             return Assembly.Load(stream.ToArray());
    //         }
    //     }
    // }
}
