using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ReadySetTarkov.LogReader;
using ReadySetTarkov.Utility;

namespace ReadySetTarkov;

internal class Core : ICoreService
{
    private readonly ILogger<Core> _logger;
    private readonly INativeMethods _nativeMethods;
    private readonly LogWatcherManager _logWatcherManager;

    private CancellationTokenSource? CancellationTokenSource { get; set; }

    private CancellationToken _hostCancellationToken = default;
    private CancellationToken? MainCancellationToken => CancellationTokenSource?.Token;
    private Task? _gameFinderTask;
    private Task? _logManagerTask;

    public Core(ILogger<Core> logger,
        INativeMethods nativeMethods,
        LogWatcherManager logWatcherManager
        )
    {
        _logger = logger;
        _nativeMethods = nativeMethods;
        _logWatcherManager = logWatcherManager;
        _logWatcherManager.LogDirectoryCreated += HandleLogDirectoryCreated;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _hostCancellationToken = cancellationToken;
        InitializeWatcherTasks();

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        CancellationTokenSource?.Cancel();
        await (_logManagerTask ?? Task.CompletedTask).ConfigureAwait(false);
    }

    private async Task MonitorForGameAsync(CancellationToken cancellationToken)
    {
        bool gameRunning = false;
        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_nativeMethods.GetTarkovWindow() != IntPtr.Zero)
                {
                    gameRunning = true;
                }
                else if (gameRunning)
                {
                    _logger.LogDebug("Game has stopped");
                    // Dont await this one. Can get a cycle of awaiting.
                    // s_gameFinderTask (MonitorForGame()) -> await Reset() -> await s_gameFinderTask()
                    _ = ResetAsync().ConfigureAwait(false);
                }

                await Task.Delay(500, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogDebug("Monitoring for game was cancelled.");
        }
    }

    private void InitializeWatcherTasks()
    {
        _logger.LogDebug("Starting watchers");

        CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_hostCancellationToken);
        _gameFinderTask = Task.Run(async () => await MonitorForGameAsync(CancellationTokenSource.Token), CancellationTokenSource.Token);
        _logManagerTask = Task.Run(async () => await _logWatcherManager.StartAsync(CancellationTokenSource.Token), CancellationTokenSource.Token);
    }

    public async Task ResetAsync()
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
    }

#pragma warning disable VSTHRD100 // Avoid async void methods
    private async void HandleLogDirectoryCreated(object? sender, EventArgs e) => await ResetAsync().ConfigureAwait(false);
}

public interface ICoreService : IHostedService
{
    Task ResetAsync();
}

