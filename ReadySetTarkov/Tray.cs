using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

using ReadySetTarkov.Settings;

namespace ReadySetTarkov
{
    // TODO: Convert to using XAML, this is getting unwieldy
    internal class Tray : ITray
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly SynchronizationContext _syncContext;
        private readonly ISettingsProvider _settingsProvider;
        private readonly ToolStripMenuItem _menuItemExit;
        private readonly ToolStripLabel _menuItemStatus;
        private readonly ToolStripMenuItem _menuItemFlashTaskbar;
        private readonly ToolStripMenuItem _menuItemSetTopMost;
        private readonly ToolStripMenuItem _menuItemSetTopMostAt20;
        private readonly ToolStripMenuItem _menuItemSetTopMostAt10;
        private readonly ToolStripMenuItem _menuItemSetTopMostAt5;
        private readonly ToolStripMenuItem _menuItemSetTopMostAt3;
        private readonly ToolStripMenuItem _menuItemSetTopMostAt0;
        private readonly ToolStripMenuItem _menuItemSoundsMatchStart;
        private readonly ToolStripMenuItem _menuItemSoundsMatchAbort;

        public Tray(ISettingsProvider settingsProvider)
        {
            if (SynchronizationContext.Current == null)
            {
                throw new InvalidOperationException("This class must be created on the UI thread.");
            }

            _syncContext = SynchronizationContext.Current;

            var menuItemReset = new ToolStripMenuItem("Reset", null, Reset);
            _menuItemExit = new ToolStripMenuItem("Exit", null, TrayClosing);
            _menuItemStatus = new ToolStripLabel("Status: N/A") { Enabled = true };
            _menuItemFlashTaskbar = new ToolStripMenuItem("Flash Taskbar", null, ToggleFlashTaskbar) { Checked = settingsProvider.Settings.FlashTaskbar };
            _menuItemSetTopMost = new ToolStripMenuItem("Bring Window to Foreground", null, ToggleTopMost) { Checked = settingsProvider.Settings.SetTopMost };
            _menuItemSetTopMostAt20 = new ToolStripMenuItem("20s left", null, SetTopMostAt) { Tag = 20, Checked = settingsProvider.Settings.WithSecondsLeft == 20 };
            _menuItemSetTopMostAt10 = new ToolStripMenuItem("10s left", null, SetTopMostAt) { Tag = 10, Checked = settingsProvider.Settings.WithSecondsLeft == 10 };
            _menuItemSetTopMostAt5 = new ToolStripMenuItem("5s left", null, SetTopMostAt) { Tag = 5, Checked = settingsProvider.Settings.WithSecondsLeft == 5 };
            _menuItemSetTopMostAt3 = new ToolStripMenuItem("3s left", null, SetTopMostAt) { Tag = 3, Checked = settingsProvider.Settings.WithSecondsLeft == 3 };
            _menuItemSetTopMostAt0 = new ToolStripMenuItem("0s left", null, SetTopMostAt) { Tag = 0, Checked = settingsProvider.Settings.WithSecondsLeft == 0 };
            _menuItemSoundsMatchStart = new ToolStripMenuItem("Match Starting", null, ToggleMatchStart) { Checked = settingsProvider.Settings.Sounds.MatchStart };
            _menuItemSoundsMatchAbort = new ToolStripMenuItem("Matchmaking Aborted", null, ToggleMatchStart) { Checked = settingsProvider.Settings.Sounds.MatchAbort };
            var contextMenuStrip = new ContextMenuStrip();

            contextMenuStrip.Items.Add(_menuItemFlashTaskbar);
            contextMenuStrip.Items.Add(_menuItemSetTopMost);

            // Set foreground with time left
            _menuItemSetTopMost.DropDownItems.AddRange(new ToolStripItem[]
            {
                _menuItemSetTopMostAt20,
                _menuItemSetTopMostAt10,
                _menuItemSetTopMostAt5,
                _menuItemSetTopMostAt3,
                _menuItemSetTopMostAt0,
            });

            // Sounds
            var soundsSubMenu = new ToolStripMenuItem("Sounds");
            contextMenuStrip.Items.Add(soundsSubMenu);
            soundsSubMenu.DropDownItems.AddRange(new ToolStripItem[]
            {
                _menuItemSoundsMatchStart,
                _menuItemSoundsMatchAbort,
            });
            contextMenuStrip.Items.Add(new ToolStripSeparator());
            contextMenuStrip.Items.Add(_menuItemStatus);
            contextMenuStrip.Items.Add(new ToolStripSeparator());
            contextMenuStrip.Items.Add(menuItemReset);
            contextMenuStrip.Items.Add(_menuItemExit);

            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon(GetType(), "RST_red"),
                ContextMenuStrip = contextMenuStrip,
                Visible = true,
            };
            _notifyIcon.MouseUp += ToggleContextMenu;
            _settingsProvider = settingsProvider;
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

        private void ToggleTopMost(object? sender, EventArgs e)
        {
            _settingsProvider.Settings.SetTopMost = !_settingsProvider.Settings.SetTopMost;

            _syncContext.Post(new SendOrPostCallback(o =>
            {
                _menuItemSetTopMost.Checked = _settingsProvider.Settings.SetTopMost;
            }), null);
        }

        private void SetTopMostAt(object? sender, EventArgs e)
        {
            if (sender is not ToolStripMenuItem toolStripMenuItem)
            {
                throw new NotImplementedException();
            }

            _settingsProvider.Settings.WithSecondsLeft = (int)toolStripMenuItem.Tag;

            _syncContext.Post(new SendOrPostCallback(o =>
            {
                foreach (ToolStripMenuItem item in _menuItemSetTopMost.DropDownItems)
                {
                    item.Checked = false;
                }

                toolStripMenuItem.Checked = true;
            }), null);
        }

        private void ToggleMatchStart(object? sender, EventArgs e)
        {
            _settingsProvider.Settings.Sounds.MatchStart = !_settingsProvider.Settings.Sounds.MatchStart;

            _syncContext.Post(new SendOrPostCallback(o =>
            {
                _menuItemSoundsMatchStart.Checked = _settingsProvider.Settings.Sounds.MatchStart;
            }), null);
        }

        private void ToggleMatchAbort(object? sender, EventArgs e)
        {
            _settingsProvider.Settings.Sounds.MatchAbort = !_settingsProvider.Settings.Sounds.MatchAbort;

            _syncContext.Post(new SendOrPostCallback(o =>
            {
                _menuItemSoundsMatchAbort.Checked = _settingsProvider.Settings.Sounds.MatchAbort;
            }), null);
        }

        private void Reset(object? sender, EventArgs e)
        {
            _ = Core.Reset();
        }

        private void ToggleContextMenu(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _notifyIcon.ContextMenuStrip.Close();
            }
        }

        private async void TrayClosing(object? sender, EventArgs e)
        {
            try
            {
                _notifyIcon.Visible = false;
                await Core.Shutdown().ConfigureAwait(false);
            }
            finally
            {
                _syncContext.Post(new SendOrPostCallback(o =>
                {
                    System.Windows.Application.Current.Shutdown();
                }), null);
            }
        }
    }
}

