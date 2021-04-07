using System;
using System.Windows;

namespace ReadySetTarkov
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);

            //Application.Run(new TrayApplicationContext());

            App application = new App
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown
            };
            application.Run();
        }
    }

    class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _ = Core.Initialize();
        }
    }
}

