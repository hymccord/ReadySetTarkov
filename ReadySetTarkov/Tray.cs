using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using ReadySetTarkov.Settings;

namespace ReadySetTarkov
{
    class Tray : ITray
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly SynchronizationContext _syncContext;
        private readonly ISettingsProvider _settingsProvider;
        private ToolStripMenuItem _menuItemExit;
        private ToolStripLabel _menuItemStatus;
        private ToolStripMenuItem _menuItemFlashTaskbar;
        private ToolStripMenuItem _menuItemPlaySound;

        //private Timer _timer;

        public Tray(ISettingsProvider _settingsProvider)
        {
            if (SynchronizationContext.Current == null)
                throw new InvalidOperationException("This class must be created on the UI thread.");

            _syncContext = SynchronizationContext.Current;

            _menuItemExit = new ToolStripMenuItem("Exit", null, TrayClosing);
            _menuItemStatus = new ToolStripLabel("Status: N/A") { Enabled = true };
            _menuItemFlashTaskbar = new ToolStripMenuItem("Flash Taskbar", null, ToggleFlashTaskbar) { Checked = _settingsProvider.Settings.FlashTaskbar };
            _menuItemPlaySound = new ToolStripMenuItem("Play Sound", null, ToggleSound) { Checked = _settingsProvider.Settings.PlaySound };
            var contextMenuStrip = new ContextMenuStrip();

            contextMenuStrip.Items.Add(_menuItemFlashTaskbar);
            contextMenuStrip.Items.Add(_menuItemPlaySound);
            contextMenuStrip.Items.Add(new ToolStripSeparator());
            contextMenuStrip.Items.Add(_menuItemStatus);
            contextMenuStrip.Items.Add(new ToolStripSeparator());
            contextMenuStrip.Items.Add(_menuItemExit);

            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon(GetType(), "RST_red"),
                ContextMenuStrip = contextMenuStrip,
                Visible = true,
            };
            this._settingsProvider = _settingsProvider;
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
            _syncContext.Post(new SendOrPostCallback(o =>
            {
                _menuItemStatus.Text = $"Status: {o}";

            }), text);
        }

        private void ToggleFlashTaskbar(object? sender, EventArgs e)
        {
            _settingsProvider.Settings.FlashTaskbar = !_settingsProvider.Settings.FlashTaskbar;

            _syncContext.Post(new SendOrPostCallback(o =>
            {
                _menuItemFlashTaskbar.Checked = _settingsProvider.Settings.FlashTaskbar;
            }), null);
        }

        private void ToggleSound(object? sender, EventArgs e)
        {
            _settingsProvider.Settings.PlaySound = !_settingsProvider.Settings.PlaySound;

            _syncContext.Post(new SendOrPostCallback(o =>
            {
                _menuItemPlaySound.Checked = _settingsProvider.Settings.PlaySound;
            }), null);
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

