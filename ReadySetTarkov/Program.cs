using System;
using System.Windows;

namespace ReadySetTarkov;

internal class Program
{
    [STAThread]
    private static void Main()
    {
        var application = new App();
        _ = application.Run();
    }
}

