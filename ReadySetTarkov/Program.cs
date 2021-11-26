using System;
using System.Windows;
using ReadySetTarkov.Settings;

namespace ReadySetTarkov
{
    internal class Program
    {
        [STAThread]
        private static void Main()
        {
            var application = new App
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown
            };
            application.Run();
        }
    }

    internal class App : Application
    {
        private bool _exitHandled = false;
        private readonly ISettingsProvider _settingsProvider;

        public App()
        {
            _settingsProvider = new SettingsProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            RegisterExitEvents();

            Core.Initialize(_settingsProvider);
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

            _settingsProvider.Save();

            _exitHandled = true;
        }
    }
}

