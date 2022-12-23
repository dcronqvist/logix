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
    public TextWriter ConsoleOut { get; private set; }

    public MinimalLogiX(TextWriter output, string[] args)
    {
        this.Args = args;
        this.ConsoleOut = output;
    }

    public void Run(bool initContent = true)
    {
        if (initContent)
        {
            this.InitContent();
            Utilities.ContentManager = this.ContentManager;
        }

        var rootCommand = new RootCommand("LogiX - CLI");

        // Add stuff here
        var projectArg = new Argument<FileInfo>("project", "The project to load");
        var circuitArg = new Argument<string>("circuit", "The circuit to load");
        var actionSequenceOpt = new Option<FileInfo>(new string[] { "--action-sequence-file", "-a" }, "The action sequence file to load");

        // SIMULATE
        var simulateCommand = new Command("simulate", "Simulate a circuit");
        simulateCommand.Add(projectArg);
        simulateCommand.Add(circuitArg);
        simulateCommand.Add(actionSequenceOpt);

        simulateCommand.SetHandler((projectPath, circuitName, actionSequence) =>
        {
            if (!projectPath.Exists)
            {
                this.ConsoleOut.WriteLine($"Project file {projectPath.FullName} does not exist");
                return;
            }

            if (!actionSequence.Exists)
            {
                this.ConsoleOut.WriteLine($"Action sequence file {actionSequence.FullName} does not exist");
                return;
            }

            this.Simulate(projectPath.FullName, circuitName, actionSequence.FullName);
        }, projectArg, circuitArg, actionSequenceOpt);

        // ADD SIMULATE AND PARSE + INVOKE
        rootCommand.Add(simulateCommand);
        rootCommand.Invoke(this.Args);
    }

    public void Simulate(string projectPath, string circuitName, string actionSequencePath)
    {
        var project = LogiXProject.FromFile(projectPath);
        NodeDescription.CurrentProject = project;

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
                    actionRunner.Run(this.ConsoleOut);
                }
                else
                {
                    // Some errors.
                    foreach (var error in errors)
                    {
                        this.ConsoleOut.WriteLine(error);
                    }
                }
            }
        }
        else
        {
            while (true)
            {
                simulation.Step();
            }
        }
    }

    public void InitContent()
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory + "/assets";

        Func<string, IContentSource> factory = (path) =>
        {
            if (Path.GetExtension(path) == ".zip")
            {
                return new ZipFileContentSource(path);
            }
            else if (Directory.Exists(path))
            {
                return new DirectoryContentSource(path);
            }

            return null;
        };

        var coreSource = new DirectoryContentSource(Path.GetFullPath($"{basePath}/core"));
        var pluginSources = new Symphony.Common.DirectoryCollectionProvider(Path.GetFullPath($"{basePath}/plugins/"), factory);
        var validator = new ContentValidator();
        var collection = IContentCollectionProvider.FromListOfSources(pluginSources.GetModSources().Prepend(coreSource));
        var loader = new MinimalContentLoader();

        var config = new ContentManagerConfiguration<ContentMeta>(validator, collection, loader);
        ContentManager = new ContentManager<ContentMeta>(config);
        ContentManager.Load();

        ScriptManager.Initialize(ContentManager);
        NodeDescription.RegisterNodeTypes();
    }
}