using System.CommandLine;
using Antlr4.Runtime;
using LogiX.Architecture.BuiltinComponents;
using LogiX.Architecture.Serialization;
using LogiX.Content;
using LogiX.Content.Scripting;
using LogiX.Minimal.ActionSequencing;
using Symphony;
using Symphony.Common;

namespace LogiX.Minimal;

// This class will be used to simulate circuits in a console application without the need for a GUI.
public class MinimalLogiX
{
    public ContentManager<ContentMeta> ContentManager { get; private set; }
    public string[] Args { get; private set; }

    public MinimalLogiX(string[] args)
    {
        this.Args = args;
    }

    public void Run()
    {
        this.InitContent();
        Utilities.ContentManager = this.ContentManager;

        var rootCommand = new RootCommand("LogiX - CLI");

        // Add stuff here
        var projectArg = new Argument<FileInfo>("project", "The project to load");
        var circuitArg = new Argument<string>("circuit", "The circuit to load");
        var actionSequenceOpt = new Option<string>(new string[] { "--action-sequence", "-a" }, () => "", "The action sequence file to load");

        // SIMULATE
        var simulateCommand = new Command("simulate", "Simulate a circuit");
        simulateCommand.Add(projectArg);
        simulateCommand.Add(circuitArg);
        simulateCommand.Add(actionSequenceOpt);

        simulateCommand.SetHandler((projectPath, circuitName, actionSequence) =>
        {
            if (!projectPath.Exists)
            {
                Console.WriteLine($"Project file {projectPath.FullName} does not exist");
                return;
            }

            this.Simulate(projectPath.FullName, circuitName, actionSequence);
        }, projectArg, circuitArg, actionSequenceOpt);

        // ADD SIMULATE AND PARSE + INVOKE
        rootCommand.Add(simulateCommand);
        rootCommand.Invoke(this.Args);
    }

    public void Simulate(string projectPath, string circuitName, string actionSequencePath)
    {
        var project = LogiXProject.FromFile(projectPath);
        ComponentDescription.CurrentProject = project;

        if (!project.HasCircuitWithName(circuitName))
        {
            Console.WriteLine($"Circuit {circuitName} does not exist in project {project.Name}");
            return;
        }

        var circuit = project.GetCircuitWithName(circuitName);
        var simulation = Simulation.FromCircuit(circuit);

        if (actionSequencePath != "")
        {
            if (!File.Exists(actionSequencePath))
            {
                Console.WriteLine($"Action sequence file {actionSequencePath} does not exist");
                return;
            }

            using (StreamReader sr = new StreamReader(actionSequencePath, new FileStreamOptions() { Access = FileAccess.Read, Share = FileShare.ReadWrite }))
            {
                var text = sr.ReadToEnd();
                var validator = new ActionSequenceValidator(circuit, text);
                if (validator.TryValidatePins(out var errors))
                {
                    var actionRunner = new ActionSequenceRunner(circuit, text, Path.GetDirectoryName(actionSequencePath));
                    actionRunner.Run();
                }
                else
                {
                    // Some errors.
                    foreach (var error in errors)
                    {
                        Console.WriteLine(error);
                    }
                }
            }
        }
        else
        {
            while (true)
            {
                simulation.Tick();
            }
        }
    }

    public void InitContent()
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory + "/assets"; ;

        var coreSource = new DirectoryContentSource(Path.GetFullPath($"{basePath}/core"));
        var validator = new ContentValidator();
        var collection = IContentCollectionProvider.FromListOfSources(coreSource); //new DirectoryCollectionProvider(@"C:\Users\RichieZ\repos\logix\assets\core", factory);
        var loader = new MinimalContentLoader();

        var config = new ContentManagerConfiguration<ContentMeta>(validator, collection, loader);
        ContentManager = new ContentManager<ContentMeta>(config);

        ContentManager.Load();

        ScriptManager.Initialize(ContentManager);
        ComponentDescription.RegisterComponentTypes();
    }
}