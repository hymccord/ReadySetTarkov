﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

using ReadySetTarkov.Settings;

namespace ReadySetTarkov
{
    internal class Tray : ITray
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly SynchronizationContext _syncContext;
        private readonly ISettingsProvider _settingsProvider;
        private readonly ToolStripMenuItem _menuItemExit;
        private readonly ToolStripLabel _menuItemStatus;
        private readonly ToolStripMenuItem _menuItemFlashTaskbar;
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
            _menuItemSoundsMatchStart = new ToolStripMenuItem("Match Starting", null, ToggleMatchStart) { Checked = settingsProvider.Settings.Sounds.MatchStart };
            _menuItemSoundsMatchAbort = new ToolStripMenuItem("Matchmaking Aborted", null, ToggleMatchStart) { Checked = settingsProvider.Settings.Sounds.MatchAbort };
            var contextMenuStrip = new ContextMenuStrip();

            contextMenuStrip.Items.Add(_menuItemFlashTaskbar);

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
                System.Windows.Application.Current.Shutdown();
            }
        }
    }
}

