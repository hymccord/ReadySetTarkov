using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReadySetTarkov
{
    internal class TrayApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _notifyIcon;

        public TrayApplicationContext()
        {
            var toolStripItem = new ToolStripMenuItem("E&xit");
            toolStripItem.Click += TrayClosing;
            var contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.Items.Add(toolStripItem);
            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon(typeof(Form), "wfc"),
                ContextMenuStrip = contextMenuStrip,
                Visible = true,
            };

            _ = Core.Initialize();
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
                Application.Exit();
            }
        }
    }
}

