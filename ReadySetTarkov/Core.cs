using System;
using System.Threading.Tasks;

using ReadySetTarkov.LogReader;
using ReadySetTarkov.Settings;
using ReadySetTarkov.Tarkov;
using ReadySetTarkov.Utility;

namespace ReadySetTarkov
{
    static class Core
    {
        private static LogWatcherManager? s_logWatcherManager;
        private static bool s_resetting;

        internal static bool Running { get; set; } = true;
        public static Game? s_game { get; set; }

        private static ISettingsProvider? s_settingsProvider;
        private static GameEventHandler? s_gameEventHandler;
        private static Tray? s_tray;

        public static bool CanShutdown { get; private set; }

#pragma warning disable 1998
        public static async Task Initialize(ISettingsProvider settingsProvider)
#pragma warning restore 1998
        {
            s_settingsProvider = settingsProvider;
            s_tray = new Tray(settingsProvider);
            s_game = new Game();
            s_gameEventHandler = new GameEventHandler(settingsProvider, s_game);
            s_logWatcherManager = new LogWatcherManager(s_game, s_tray);
            
            // Logging
            _ = UpdateAsync().ConfigureAwait(false);
            _ = s_logWatcherManager.Start();
        }

        private static async Task UpdateAsync()
        {
            while (s_game != null && Running)
            {
                if (User32.GetTarkovWindow() != IntPtr.Zero)
                {
                    s_game.IsRunning = true;
                }
                else if (s_game.IsRunning)
                {
                    s_game.IsRunning = false;
                    await Reset();
                }
                await Task.Delay(500);
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
            
            if (s_logWatcherManager != null)
            {
                var stoppedReader = await s_logWatcherManager.Stop();
                await Task.Delay(1000);
                if (stoppedReader)
                    _ = s_logWatcherManager.Start();
            }

            s_resetting = false;
        }
    }
}

