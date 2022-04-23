using System.Threading.Tasks;

using Hardcodet.Wpf.TaskbarNotification;

using Microsoft.VisualStudio.Threading;

namespace ReadySetTarkov;

/// <summary>
/// Interaction logic for Tray.xaml
/// </summary>
public partial class Tray : INotifyIcon
{
    private readonly JoinableTaskFactory _joinableTaskFactory;

    public Tray(JoinableTaskFactory joinableTaskFactory, ITray viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
        _joinableTaskFactory = joinableTaskFactory;
    }

    async Task INotifyIcon.CloseBalloonTipAsync()
    {
        await _joinableTaskFactory.SwitchToMainThreadAsync();

        CloseBalloon();
    }

    async Task INotifyIcon.ShowBalloonTipAsync(string title, string message, BalloonIcon icon)
    {
        await _joinableTaskFactory.SwitchToMainThreadAsync();

        ShowBalloonTip(title, message, icon);
    }
}
