using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using ReadySetTarkov.LogReader.Handlers;
using ReadySetTarkov.Utility;

namespace ReadySetTarkov.LogReader
{
    internal class LogWatcherManager
    {
        private readonly ApplicationHandler _applicationLineHandler = new ApplicationHandler();
        private readonly LogWatcher _logWatcher;
        private FileSystemWatcher? _fileSystemWatcher;
        private bool _stop;
        private Game? _game;
        private string? _currentGameLogDir;

        public static LogWatcherInfo ApplicationLogWatcherInfo => new LogWatcherInfo("application");
        public LogWatcherManager()
        {
            _logWatcher = new LogWatcher(new[]
            {
                ApplicationLogWatcherInfo
            });
            _logWatcher.OnNewLines += OnNewLines;
            _logWatcher.OnLogFileFound += OnLogFileFound;
        }
        private void OnLogFileFound(string msg)
        {

        }

        public async Task Start(Game game)
        {
            await FindTarkov();
            InitializeGameState(game);
            // Even though the game has started, the latest logs folder could not be created yet
            StartDirectoryWatcher(game);
            _stop = false;

            if (string.IsNullOrEmpty(_currentGameLogDir))
                throw new InvalidOperationException("Could not find tarkov's logs directory.");

            _logWatcher.Start(_currentGameLogDir);
        }

        private static async Task FindTarkov()
        {
            //Log.Warn("Tarkov not found, waiting for process...");
            while (User32.GetTarkovProc() == null)
                await Task.Delay(500);
        }

        public async Task<bool> Stop(bool force = false)
        {
            _stop = true;
            if (_fileSystemWatcher != null)
                _fileSystemWatcher.EnableRaisingEvents = false;
            return await _logWatcher.Stop(force);
        }

        private void InitializeGameState(Game game)
        {
            _game = game;
            Process? proc = User32.GetTarkovProc();

            if (proc == null)
                return;

            var dir = new FileInfo(User32.GetProcessFilename(proc)).Directory?.FullName;

            if (dir == null)
            {
                return;
            }

            game.GameDirectory = dir;
            _currentGameLogDir = GetNewestSubdirectory(game.LogsDirectory!);
        }

        private void OnNewLines(List<LogLine> lines)
        {
            foreach (var line in lines)
            {
                if (_stop)
                    break;
                //_game.GameTime.Time = line.Time;
                switch (line.Namespace)
                {
                    case "application":
                        _applicationLineHandler.Handle(line, _game!);
                        break;
                }
            }
            //Helper.UpdateEverything(_game);
        }

        private static string GetNewestSubdirectory(string directory)
        {
            return Directory.GetDirectories(directory).Select(s => new DirectoryInfo(s)).OrderByDescending(di => di.CreationTime).First().FullName;
        }

        private void StartDirectoryWatcher(Game game)
        {
            _fileSystemWatcher = new FileSystemWatcher(game.LogsDirectory!)
            {
                NotifyFilter = NotifyFilters.DirectoryName
            };
            _fileSystemWatcher.Created += async (s, e) =>
            {
                _fileSystemWatcher.EnableRaisingEvents = false;

                if (_game != null)
                {
                    await Stop();
                    await Start(game);
                }
            };
            _fileSystemWatcher.EnableRaisingEvents = true;
        }
    }
}

