using System;
using System.Threading;
using System.Threading.Tasks;

using ReadySetTarkov.LogReader;
using ReadySetTarkov.Settings;
using ReadySetTarkov.Tarkov;
using ReadySetTarkov.Utility;

namespace ReadySetTarkov
{
    internal static class Core
    {
        private static LogWatcherManager? s_logWatcherManager;

        public static Game? Game { get; set; }
        private static CancellationTokenSource? CancellationTokenSource { get; set; }

        private static CancellationToken? MainCancellationToken => CancellationTokenSource?.Token;
#pragma warning disable IDE0052 // Remove unread private members
        private static ISettingsProvider? s_settingsProvider;
        private static GameEventHandler? s_gameEventHandler;
#pragma warning restore IDE0052 // Remove unread private members
        private static Tray? s_tray;
        private static Task? s_gameFinderTask;
        private static Task? s_logManagerTask;

        public static void Initialize(ISettingsProvider settingsProvider)
        {
            s_settingsProvider = settingsProvider;
            s_tray = new Tray(settingsProvider);
            Game = new Game();
            s_gameEventHandler = new GameEventHandler(settingsProvider, Game);
            s_logWatcherManager = new LogWatcherManager(Game, s_tray);
            s_logWatcherManager.LogDirectoryCreated += HandleLogDirectoryCreated;

            InitializeWatcherTasks();
        }

        public static async Task Shutdown()
        {
            CancellationTokenSource?.Cancel();
            await (s_logManagerTask ?? Task.CompletedTask).ConfigureAwait(false);
        }

        private static async Task UpdateAsync(CancellationToken cancellationToken)
        {
            while (Game is not null && !cancellationToken.IsCancellationRequested)
            {
                if (User32.GetTarkovWindow() != IntPtr.Zero)
                {
                    Game.IsRunning = true;
                }
                else if (Game.IsRunning)
                {
                    Game.IsRunning = false;
                    await Reset().ConfigureAwait(false);
                }

                await Task.Delay(500, cancellationToken).ConfigureAwait(false);
            }
        }

        private static void InitializeWatcherTasks()
        {
            CancellationTokenSource = new CancellationTokenSource();
            s_gameFinderTask = UpdateAsync(CancellationTokenSource.Token);
            s_logManagerTask = s_logWatcherManager?.Start(CancellationTokenSource.Token);
        }

        internal static async Task Reset()
        {
            if (MainCancellationToken.HasValue && MainCancellationToken.Value.IsCancellationRequested)
            {
                return;
            }

            CancellationTokenSource?.Cancel();

            await Task.WhenAll(new Task[]
            {
                s_gameFinderTask ?? Task.CompletedTask,
                s_logManagerTask ?? Task.CompletedTask
            }).ContinueWith(
                (t) =>
                InitializeWatcherTasks(),
                TaskContinuationOptions.None)
            .ConfigureAwait(false);
        }

        private static async void HandleLogDirectoryCreated(object? sender, EventArgs e)
        {
            await Reset().ConfigureAwait(false);
        }
    }
}

