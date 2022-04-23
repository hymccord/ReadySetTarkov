using System;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.Threading;
using ReadySetTarkov.LogReader;
using ReadySetTarkov.LogReader.Handlers;
using ReadySetTarkov.Settings;
using ReadySetTarkov.Tarkov;
using ReadySetTarkov.Utility;
using Serilog.Events;
using Serilog;
using Microsoft.Extensions.Logging;
using Serilog.Core;

namespace ReadySetTarkov;

public static class ReadySetTarkovHostBuilderExtensions
{
    public static IHostBuilder AddServices(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices(services =>
        {
            services.AddSingleton<ICoreService, Core>();
            services.AddSingleton<IHostedService>(s => s.GetRequiredService<ICoreService>());
            services.AddHostedService<GameEventHandler>();

            services.AddSingleton<Game>();
            services.AddSingleton<ITarkovGame>(s => s.GetRequiredService<Game>());
            services.AddSingleton<IGameEvents>(s => s.GetRequiredService<Game>());
            services.AddSingleton<INotifyIcon, Tray>();
            services.AddSingleton<ITray, TrayViewModel>();
            services.AddSingleton<ISettingsProvider, SettingsProvider>();
            services.AddSingleton<ApplicationHandler>();
            services.AddSingleton<ITarkovStateManager, TarkovStateManager>();
            services.AddSingleton<LogWatcherManager>();

            var joinableTaskContext = new JoinableTaskContext(Thread.CurrentThread, new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher, DispatcherPriority.Background));
            services.AddSingleton(joinableTaskContext);
            services.AddSingleton(joinableTaskContext.Factory);

            services.AddTransient<INativeMethods, NativeMethods>();
            services.AddTransient<IKernel32, Kernel32>();
            services.AddTransient<IUser32, User32>();
        });

        return hostBuilder;
    }

    public static LoggerConfiguration MinimumLevelFromConfiguration(this LoggerConfiguration builder, IConfiguration config)
    {
        foreach (var (key, value) in config.AsEnumerable())
        {
            var idx = key.LastIndexOf(':');
            var eventName = key[(idx + 1)..];

            if (Enum.TryParse<LogEventLevel>(value, out var level))
            {
                builder.MinimumLevel.Override(eventName, level);
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
