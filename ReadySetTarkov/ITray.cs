using System.Threading.Tasks;
using Hardcodet.Wpf.TaskbarNotification;

namespace ReadySetTarkov
{
    public interface ITray
    {
        bool Visible { get; set; }
        void SetIcon(string resource);
        void SetStatus(string text);
    }

    public interface INotifyIcon
    {
        Task ShowBalloonTipAsync(string title, string text, BalloonIcon icon);
        Task CloseBalloonTipAsync();
    }
}
