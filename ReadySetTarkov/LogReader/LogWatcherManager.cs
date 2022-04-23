using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ReadySetTarkov.Tarkov;
using ReadySetTarkov.Utility;

namespace ReadySetTarkov.LogReader;

internal class LogWatcherManager
{
    private readonly ILogger<LogWatcherManager> _logger;
    private readonly LogWatcher _logWatcher;
    private readonly ITarkovGame _game;
    private readonly ITray _tray;
    private readonly INativeMethods _nativeMethods;
    private readonly ITarkovStateManager _gameStateManager;
    private FileSystemWatcher? _fileSystemWatcher;
    private string? _currentGameLogDir;

    public event EventHandler? LogDirectoryCreated;

    public LogWatcherManager(
        ILogger<LogWatcherManager> logger,
        LogWatcher logWatcher,
        ITarkovStateManager gameStateManager,
        ITarkovGame game,
        ITray tray,
        INativeMethods nativeMethods)
    {
        _logger = logger;
        _logWatcher = logWatcher;
        _game = game;
        _tray = tray;

        _gameStateManager = gameStateManager;
        _nativeMethods = nativeMethods;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await FindTarkovAsync(cancellationToken);
            InitializeGameState();
            // Even though the game has started, the latest logs folder could not be created yet
            StartDirectoryWatcher();

            _tray.SetStatus("Parsing logs...");

            if (string.IsNullOrEmpty(_currentGameLogDir))
            {
                throw new InvalidOperationException("Could not find tarkov's logs directory.");
            }

            await _logWatcher.WatchAsync(_currentGameLogDir, cancellationToken)
                .ContinueWith((t) =>
                {
                    _fileSystemWatcher!.EnableRaisingEvents = false;
                    _gameStateManager.SetGameState(GameState.None);
                },
                CancellationToken.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogDebug("Stopping log watchers");
        }
    }

    private async Task FindTarkovAsync(CancellationToken cancellationToken = default)
    {
        _tray.SetStatus("Waiting for Tarkov to start");
        //Log.Warn("Tarkov not found, waiting for process...");
        while (_nativeMethods.GetTarkovProcId() == 0)
        {
            await Task.Delay(500, cancellationToken);
        }
    }

    private void InitializeGameState()
    {
        uint proc = _nativeMethods.GetTarkovProcId();

        if (proc == 0)
        {
            return;
        }

        string? dir = new FileInfo(_nativeMethods.GetProcessFilename(proc)).Directory?.FullName;

        if (dir is null)
        {
            return;
        }

        _game.GameDirectory = dir;
        _currentGameLogDir = GetNewestSubdirectory(_game.LogsDirectory!);

        _logger.LogInformation("Using {GameLogDir} as latest game log directory", _currentGameLogDir);
    }

    private static string GetNewestSubdirectory(string directory) => Directory.GetDirectories(directory).Select(s => new DirectoryInfo(s)).OrderByDescending(di => di.CreationTime).First().FullName;

    private void StartDirectoryWatcher()
    {
        _fileSystemWatcher = new FileSystemWatcher(_game.LogsDirectory!)
        {
            NotifyFilter = NotifyFilters.DirectoryName
        };
        _fileSystemWatcher.Created += (s, e) =>
        {
            LogDirectoryCreated?.Invoke(this, e);
        };
        _fileSystemWatcher.EnableRaisingEvents = true;
    }
}

