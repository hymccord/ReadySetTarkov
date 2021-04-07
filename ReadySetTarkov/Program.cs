using System;
using System.Windows;
using ReadySetTarkov.Settings;

namespace ReadySetTarkov
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            App application = new App
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown
            };
            application.Run();
        }
    }

    class App : Application
    {
        private bool _exitHandled = false;
        private ISettingsProvider settingsProvider;

        public App()
        {
            settingsProvider = new SettingsProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            RegisterExitEvents();

            _ = Core.Initialize(settingsProvider);
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
                return;

            settingsProvider.Save();

            _exitHandled = true;
        }
    }
}

