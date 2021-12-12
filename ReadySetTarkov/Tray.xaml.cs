using System.Threading.Tasks;
using Hardcodet.Wpf.TaskbarNotification;
using ReadySetTarkov.Utility;

namespace ReadySetTarkov
{
    /// <summary>
    /// Interaction logic for Tray.xaml
    /// </summary>
    public partial class Tray : INotifyIcon
    {
        public Tray(ITray viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        async Task INotifyIcon.CloseBalloonTipAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            CloseBalloon();
        }

        async Task INotifyIcon.ShowBalloonTipAsync(string title, string message, BalloonIcon icon)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            ShowBalloonTip(title, message, icon);
        }
    }
}
