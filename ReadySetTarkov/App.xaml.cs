using System;
using System.Threading.Tasks;
using System.Windows;

using DryIoc.Microsoft.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using ReadySetTarkov.Settings;

using Serilog;
using Serilog.Events;

namespace ReadySetTarkov;

public partial class App : Application
{
    private readonly IHost _host;
    private bool _exitHandled = false;
    private INotifyIcon? _icon;

    public App()
    {
        const string LogFilePath = "ready-set-tarkov.log";

        InstallExceptionHandlers();

        var logSettings = new LogSettings(true);
        _host = Host.CreateDefaultBuilder()
            .UseServiceProviderFactory(new DryIocServiceProviderFactory())
            .AddServices()
            .UseSerilog((context, loggerConfiguration) => loggerConfiguration
                .MinimumLevelFromConfiguration(context.Configuration.GetSection("Logging:Loglevel"))
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Is(LogEventLevel.Verbose)
                .WriteTo.Debug(levelSwitch: logSettings.Console)
                .WriteTo.File(LogFilePath, levelSwitch: logSettings.File))
            .Build();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _ = _host.StartAsync();
        _icon = _host.Services.GetRequiredService<INotifyIcon>();

        RegisterExitEvents();
    }

    private void RegisterExitEvents()
    {
        Current.Exit += (s, e) => Exiting();
    }

    private void Exiting()
    {
        if (_exitHandled)
        {
            return;
        }

        _exitHandled = true;
        _host.Services.GetRequiredService<ISettingsProvider>().Save();
    }

    private void InstallExceptionHandlers()
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            LogUnhandledExceptionAsync((Exception)e.ExceptionObject, nameof(AppDomain.CurrentDomain.UnhandledException));
        DispatcherUnhandledException += (s, e) =>
        {
            LogUnhandledExceptionAsync(e.Exception, nameof(DispatcherUnhandledException));
            e.Handled = true;
        };
        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            LogUnhandledExceptionAsync(e.Exception, nameof(TaskScheduler.UnobservedTaskException));
            e.SetObserved();
        };
    }

#pragma warning disable VSTHRD100, VSTHRD200
    private async void LogUnhandledExceptionAsync(Exception ex, string sources)
#pragma warning restore VSTHRD100, VSTHRD200
    {
#if DEBUG
        await _icon!.ShowBalloonTipAsync("ReadySetTarkove fatal error", sources, Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Error);
        await Task.Delay(5000);
        await _icon.CloseBalloonTipAsync();
#endif
    }
}

