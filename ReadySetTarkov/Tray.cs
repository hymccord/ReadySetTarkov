using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReadySetTarkov
{
    public class Tray : ITray
    {
        private readonly NotifyIcon _notifyIcon;
        private int _index;
        private ToolStripMenuItem _menuItemExit;
        private ToolStripLabel _menuItemStatus;

        //private Timer _timer;

        public Tray()
        {
            _menuItemExit = new ToolStripMenuItem("Exit", null, TrayClosing);
            _menuItemStatus = new ToolStripLabel("Status: N/A") { Enabled = false };
            var contextMenuStrip = new ContextMenuStrip();

            contextMenuStrip.Items.Add(_menuItemStatus);
            contextMenuStrip.Items.Add(new ToolStripSeparator());
            contextMenuStrip.Items.Add(_menuItemExit);

            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon(GetType(), "RST_red"),
                ContextMenuStrip = contextMenuStrip,
                Visible = true,
            };
        }

        public bool Visible
        {
            get => _notifyIcon.Visible;
            set => _notifyIcon.Visible = value;
        }

        public void SetIcon(string resource)
        {
            try
            {
                _notifyIcon.Icon = new Icon(GetType(), resource);
            }
            catch (ArgumentException)
            {
                Debug.WriteLine($"{nameof(SetIcon)}: That resource could not be found: {resource}");
            }
        }

        public void SetStatus(string text)
        {
            _menuItemStatus.Text = $"Status: {text}";
        }

        private async void TrayClosing(object? sender, EventArgs e)
        {
            try
            {
                Core.Running = false;

                for (var i = 0; i < 100; i++)
                {
                    if (Core.CanShutdown)
                        break;
                    await Task.Delay(50);
                }
            }
            finally
            {
                _notifyIcon.Visible = false;
                System.Windows.Application.Current.Shutdown();
            }
        }
    }
}

