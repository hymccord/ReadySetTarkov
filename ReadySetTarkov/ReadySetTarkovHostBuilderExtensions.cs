using System;
using System.Threading;
using System.Windows.Threading;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;

using ReadySetTarkov.LogReader;
using ReadySetTarkov.LogReader.Handlers.Application;
using ReadySetTarkov.LogReader.Handlers.Application.LineHandlers;
using ReadySetTarkov.Services;
using ReadySetTarkov.Settings;
using ReadySetTarkov.Tarkov;
using ReadySetTarkov.Utility;

using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace ReadySetTarkov;

public static class ReadySetTarkovHostBuilderExtensions
{
    public static IHostBuilder AddServices(this IHostBuilder hostBuilder)
    {
        _ = hostBuilder.ConfigureServices(services =>
        {
            _ = services.AddSingleton<ICoreService, Core>();
            _ = services.AddSingleton<IHostedService>(s => s.GetRequiredService<ICoreService>());
            _ = services.AddHostedService<GameEventHandler>();
            _ = services.AddHostedService<ShutdownHandler>();

            _ = services.AddSingleton<Game>();
            _ = services.AddSingleton<ITarkovGame>(s => s.GetRequiredService<Game>());
            _ = services.AddSingleton<IGameEvents>(s => s.GetRequiredService<Game>());
            _ = services.AddSingleton<INotifyIcon, Tray>();
            _ = services.AddSingleton<ITray, TrayViewModel>();
            _ = services.AddSingleton<ISettingsProvider, SettingsProvider>();
            _ = services.AddSingleton<ITarkovStateManager, TarkovStateManager>();
            _ = services.AddSingleton<LogWatcherManager>();
            _ = services.AddSingleton<LogWatcher>();

            var joinableTaskContext = new JoinableTaskContext(Thread.CurrentThread, new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher, DispatcherPriority.Background));
            _ = services.AddSingleton(joinableTaskContext);
            _ = services.AddSingleton(joinableTaskContext.Factory);

            _ = services.AddTransient<INativeMethods, NativeMethods>();
            _ = services.AddTransient<IKernel32, Kernel32>();
            _ = services.AddTransient<IUser32, User32>();

            // Tarkov Log Watchers
            _ = services.AddSingleton<ILogFileHandlerProvider, ApplicationLogFileHandlerProvider>();
            _ = services.AddTransient<ApplicationHandler>();
            _ = services.AddSingleton<IApplicationLogLineContentHandler, GameLineHandler>();
            _ = services.AddSingleton<IApplicationLogLineContentHandler, LocationLoadedLineHandler>();
            _ = services.AddSingleton<IApplicationLogLineContentHandler, NetworkGameAbortedLineHandler>();
            _ = services.AddSingleton<IApplicationLogLineContentHandler, SelectProfileLineHandler>();
            _ = services.AddSingleton<IApplicationLogLineContentHandler, TraceNetworkLineHandler>();
        });

        return hostBuilder;
    }

    public static LoggerConfiguration MinimumLevelFromConfiguration(this LoggerConfiguration builder, IConfiguration config)
    {
        foreach ((string key, string value) in config.AsEnumerable())
        {
            int idx = key.LastIndexOf(':');
            string? eventName = key[(idx + 1)..];

            if (Enum.TryParse<LogEventLevel>(value, out LogEventLevel level))
            {
                _ = builder.MinimumLevel.Override(eventName, level);
            }
        }

        return builder;
    }
}

public class LogSettings
{
    public LogSettings(bool verbose)
    {
        Console = new LoggingLevelSwitch();
        File = new LoggingLevelSwitch();
        SetConsoleLevel(verbose ? LogLevel.Trace : LogLevel.Information);
        SetFileLevel(LogLevel.Debug);
    }

    public LoggingLevelSwitch Console { get; }

    public LoggingLevelSwitch File { get; }

    public void SetConsoleLevel(LogLevel level)
        => Console.MinimumLevel = GetLogLevel(level);

    public void SetFileLevel(LogLevel level)
        => File.MinimumLevel = GetLogLevel(level);

    private static LogEventLevel GetLogLevel(LogLevel newLogLevel)
        => newLogLevel switch
        {
            LogLevel.Trace => LogEventLevel.Verbose,
            LogLevel.Debug => LogEventLevel.Debug,
            LogLevel.Information => LogEventLevel.Information,
            LogLevel.Warning => LogEventLevel.Warning,
            LogLevel.Error => LogEventLevel.Error,
            LogLevel.Critical => LogEventLevel.Fatal,
            LogLevel.None => LogEventLevel.Fatal,
            _ => throw new NotImplementedException()
        };
}
