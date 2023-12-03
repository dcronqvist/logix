using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotGLFW;
using LogiX.Content;
using LogiX.Debug.Logging;
using LogiX.Graphics.Rendering;
using LogiX.Input;
using LogiX.UserInterface.Views;
using LogiX.UserInterfaceContext;
using LogiX.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Symphony;
using Symphony.Common;
using LogiX.Graphics;
using LogiX.UserInterface.Coroutines;
using LogiX.UserInterface.Views.EditorView;
using LogiX.Model.Projects;
using NLua;
using LogiX.Scripting;

namespace LogiX;

public interface IFactory<T>
{
    T Create();
}

public class Factory<T>(Func<T> factory) : IFactory<T>
{
    private readonly Func<T> _factory = factory;

    public T Create() => _factory();
}

public class Program
{
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Debug & Logging
        services.AddSingleton<ILog, Log>(sp =>
        {
            var consoleLogger = new ConsoleLogger();
            var logger = new CompositeLogger(consoleLogger);
            var log = new Log(logger);
            log.Start();
            return log;
        });

        // Application stuff
        services.AddSingleton<IApplicationLogic, ApplicationLogic>();
        services.AddSingleton<IApplication, Application>();
        services.AddSingleton<ICoroutineService, CoroutineService>();
        services.AddSingleton<IFileSystemProvider, FileSystemProvider>();

        // Glfw & OpenGL context, OS-wrappers
        services.AddSingleton<IUserInterfaceContext<InputState, Keys, ModifierKeys, MouseButton>, GLFWUserInterfaceContext>();
        services.AddSingleton<IAsyncGLContextProvider, QueueBasedAsyncGLContextProvider>();
        services.AddSingleton<IKeyboard<char, Keys, ModifierKeys>, Keyboard>();
        services.AddSingleton<IMouse<MouseButton>, Mouse>();
        services.AddSingleton<IImGuiController, OpenGLImGuiController>();

        // Content loading
        services.AddSingleton<IContentManager<ContentMeta>, ContentManager<ContentMeta>>(sp =>
        {
            var structureValidator = sp.GetRequiredService<IContentStructureValidator<ContentMeta>>();
            var sources = sp.GetRequiredService<IEnumerable<IContentSource>>();
            var loader = sp.GetRequiredService<IContentLoader>();
            var overwriter = sp.GetRequiredService<IContentOverwriter>();

            return new ContentManager<ContentMeta>(
                structureValidator, sources, loader, overwriter);
        });
        services.AddSingleton<IContentStructureValidator<ContentMeta>, ContentStructureValidator>();
        services.AddSingleton<IEnumerable<IContentSource>, List<IContentSource>>(sp => GetContentSources());
        services.AddSingleton<IContentLoader, ContentLoader>();
        services.AddSingleton<IContentOverwriter, ContentOverwriter>();

        // Graphics
        services.AddSingleton<TextureRenderer>();
        services.AddSingleton<TextRenderer>();
        services.AddSingleton<PrimitiveRenderer>();
        services.AddSingleton<IRenderer, Renderer>();

        // User interface views
        AddFactory<LoadingView>(services);
        AddFactory<EditorView>(services);

        // LogiX stuff
        services.AddSingleton<IProjectService, ProjectService>();
        AddFactory<Lua>(services);
        services.AddSingleton<ILuaService, LuaService>();

        return services.BuildServiceProvider();
    }

    private static void AddFactory<T>(IServiceCollection services) where T : class
    {
        services.AddTransient<T>();
        services.AddSingleton<Func<T>>(sp => () => sp.GetService<T>());
        services.AddSingleton<IFactory<T>, Factory<T>>();
    }

    private static List<IContentSource> GetContentSources()
    {
        var sources = new List<IContentSource>();
#if DEBUG
        var logixCoreAssetSource = new DirectoryContentSource("assets");
        sources.Add(logixCoreAssetSource);
#elif RELEASE
        var embedSource = new EmbeddedSource(typeof(Program).Assembly, "LogiX._embeds.logix-core.zip");
        sources.Add(embedSource);
#endif
        string addonsPath = "addons";
        if (Directory.Exists(addonsPath))
        {
            var addonZipSources = Directory.GetFiles(addonsPath)
                .Select<string, IContentSource>(addonPath =>
                {
                    if (addonPath.EndsWith(".zip"))
                    {
                        return new ZipFileContentSource(addonPath);
                    }

                    throw new Exception($"Unknown addon file type: {addonPath}");
                })
                .ToList();

            var addonDirectorySources = Directory.GetDirectories(addonsPath)
                .Select<string, IContentSource>(addonPath =>
                {
                    return new DirectoryContentSource(addonPath);
                })
                .ToList();

            sources.AddRange(addonZipSources);
            sources.AddRange(addonDirectorySources);
        }

        return sources;
    }

    static void Main(string[] args)
    {
        var services = ConfigureServices();

        var app = services.GetRequiredService<IApplication>();

        app.Run();
    }
}
