using System;
using System.Threading.Tasks;

using ReadySetTarkov.LogReader;
using ReadySetTarkov.Utility;

namespace ReadySetTarkov
{
    static class Core
    {
        private static LogWatcherManager s_logWatcherManager = new LogWatcherManager();
        private static bool s_resetting;

        internal static bool Running { get; set; } = true;
        public static Game Game { get; set; } = new Game();
        public static bool CanShutdown { get; private set; }

#pragma warning disable 1998
        public static async Task Initialize()
#pragma warning restore 1998
        {
            // Logging
            _ = UpdateAsync();
            _ = s_logWatcherManager.Start(Game);
        }

        private static async Task UpdateAsync()
        {
            while (Running)
            {
                if (User32.GetTarkovWindow() != IntPtr.Zero)
                {
                    Game.IsRunning = true;
                }
                else if (Game.IsRunning)
                {
                    Game.IsRunning = false;
                    await Reset();
                }
                await Task.Delay(100);
            }

            CanShutdown = true;
        }

        private static async Task Reset()
        {
            if (s_resetting)
            {
                return;
            }
            s_resetting = true;
            var stoppedReader = await s_logWatcherManager.Stop();
            //Game.Reset();
            await Task.Delay(1000);
            if (stoppedReader)
                _ = s_logWatcherManager.Start(Game);

            s_resetting = false;
        }
    }
}

