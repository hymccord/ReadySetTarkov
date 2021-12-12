using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using ReadySetTarkov.LogReader;
using ReadySetTarkov.Settings;
using ReadySetTarkov.Tarkov;
using ReadySetTarkov.Utility;

namespace ReadySetTarkov
{
    internal class Core
    {
#pragma warning disable IDE0052 // Remove unread private members
        private readonly ISettingsProvider _settingsProvider;
        private readonly INativeMethods _nativeMethods;
        private readonly LogWatcherManager _logWatcherManager;
        private readonly GameEventHandler _gameEventHandler;
#pragma warning restore IDE0052 // Remove unread private members

        private CancellationTokenSource? CancellationTokenSource { get; set; }

        private CancellationToken? MainCancellationToken => CancellationTokenSource?.Token;
        private Task? _gameFinderTask;
        private Task? _logManagerTask;

        public Core(
            Game game,
            ISettingsProvider settingsProvider,
            INativeMethods nativeMethods,
            ITray tray)
        {
            _settingsProvider = settingsProvider;
            _nativeMethods = nativeMethods;
            _gameEventHandler = new GameEventHandler(settingsProvider, game, nativeMethods);
            _logWatcherManager = new LogWatcherManager(game, tray, nativeMethods);
            _logWatcherManager.LogDirectoryCreated += HandleLogDirectoryCreated;   
        }

        public void Start()
        {
            InitializeWatcherTasks();
        }

        public async Task ShutdownAsync()
        {
            CancellationTokenSource?.Cancel();
            await (_logManagerTask ?? Task.CompletedTask).ConfigureAwait(false);
        }

        private async Task MonitorForGameAsync(CancellationToken cancellationToken)
        {
            var gameRunning = false;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_nativeMethods.GetTarkovWindow() != IntPtr.Zero)
                {
                    gameRunning = true;
                }
                else if (gameRunning)
                {
                    // Dont await this one. Can get a cycle of awaiting.
                    // s_gameFinderTask (MonitorForGame())6 -> await Reset() -> await s_gameFinderTask()
                    _ = ResetAsync().ConfigureAwait(false);
                }

                await Task.Delay(500, cancellationToken).ConfigureAwait(false);
            }
        }

        private void InitializeWatcherTasks()
        {
            CancellationTokenSource = new CancellationTokenSource();
            _gameFinderTask = MonitorForGameAsync(CancellationTokenSource.Token);
            _logManagerTask = _logWatcherManager.StartAsync(CancellationTokenSource.Token);
        }

        internal async Task ResetAsync()
        {
            if (MainCancellationToken.HasValue && MainCancellationToken.Value.IsCancellationRequested)
            {
                return;
            }

            CancellationTokenSource?.Cancel();

            await Task.WhenAll(new Task[]
            {
                _gameFinderTask ?? Task.CompletedTask,
                _logManagerTask ?? Task.CompletedTask
            }).ContinueWith(
                (t) =>
                InitializeWatcherTasks(),
                default,
                TaskContinuationOptions.None,
                TaskScheduler.Default)
            .ConfigureAwait(false);
            Debug.WriteLine("");
        }

#pragma warning disable VSTHRD100 // Avoid async void methods
        private async void HandleLogDirectoryCreated(object? sender, EventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
        {
            await ResetAsync().ConfigureAwait(false);
        }
    }
}

