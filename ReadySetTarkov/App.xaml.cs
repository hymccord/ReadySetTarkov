using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Threading;
using ReadySetTarkov.Settings;
using ReadySetTarkov.Tarkov;
using ReadySetTarkov.Utility;

namespace ReadySetTarkov
{
    public partial class App : Application
    {
        private bool _exitHandled = false;
        private readonly JoinableTaskContext _joinableTaskContext;
        private Core? _core;
        private INotifyIcon? _icon;

        public static IServiceProvider GlobalProvider { get; private set; } = default!;

        public App()
        {
            InstallExceptionHandlers();

            _joinableTaskContext = new JoinableTaskContext(Thread.CurrentThread, new DispatcherSynchronizationContext());
            GlobalProvider = ConfigureServices();
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<Core>();
            services.AddSingleton<Game>();
            services.AddSingleton<INotifyIcon, Tray>();
            services.AddSingleton<ITray, TrayViewModel>();
            services.AddSingleton<ISettingsProvider, SettingsProvider>();
            services.AddSingleton<JoinableTaskContext>(_joinableTaskContext);
            services.AddSingleton<JoinableTaskFactory>(_joinableTaskContext.Factory);

            services.AddTransient<INativeMethods, NativeMethods>();
            services.AddTransient<IKernel32, Kernel32>();
            services.AddTransient<IUser32, User32>();

            return services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _core = GlobalProvider.GetRequiredService<Core>();
            _icon = GlobalProvider.GetRequiredService<INotifyIcon>();
            _core.Start();

            RegisterExitEvents();
        }

        private void RegisterExitEvents()
        {
            AppDomain.CurrentDomain.ProcessExit += (s, e) => Exiting();
            Current.Exit += (s, e) => Exiting();
            Current.SessionEnding += (s, e) => Exiting();
        }

        private void Exiting()
        {
            if (_exitHandled)
            {
                return;
            }

            GlobalProvider.GetRequiredService<ISettingsProvider>().Save();

            _exitHandled = true;
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
            await _icon!.ShowBalloonTipAsync("ReadySetTarkove fatal error", sources, Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Error);

            await Task.Delay(5000);

            await _icon.CloseBalloonTipAsync();
        }
    }
}

