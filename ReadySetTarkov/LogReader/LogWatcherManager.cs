﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using ReadySetTarkov.LogReader.Handlers;
using ReadySetTarkov.Tarkov;
using ReadySetTarkov.Utility;

namespace ReadySetTarkov.LogReader
{
    internal class LogWatcherManager
    {
        private readonly ApplicationHandler _applicationLineHandler = new ApplicationHandler();
        private readonly LogWatcher _logWatcher;
        private readonly ITarkovGame _game;
        private readonly ITray _tray;
        private FileSystemWatcher? _fileSystemWatcher;
        private bool _stop;
        private string? _currentGameLogDir;
        private ITarkovStateManager _gameStateManager;

        public static LogWatcherInfo ApplicationLogWatcherInfo => new LogWatcherInfo("application");
        public LogWatcherManager(ITarkovGame game, ITray tray)
        {
            _logWatcher = new LogWatcher(new[]
            {
                ApplicationLogWatcherInfo
            });
            _logWatcher.OnNewLines += OnNewLines;
            _logWatcher.OnLogFileFound += OnLogFileFound;
            _game = game;
            _tray = tray;

            _gameStateManager = new TarkovStateManager(game, tray);
        }
        private void OnLogFileFound(string msg)
        {

        }

        public async Task Start()
        {
            await FindTarkov();
            InitializeGameState();
            // Even though the game has started, the latest logs folder could not be created yet
            StartDirectoryWatcher();
            _stop = false;

            _tray.SetStatus("Parsing logs...");

            if (string.IsNullOrEmpty(_currentGameLogDir))
                throw new InvalidOperationException("Could not find tarkov's logs directory.");

            _logWatcher.Start(_currentGameLogDir);
        }

        private async Task FindTarkov()
        {
            _tray.SetStatus("Waiting for Tarkov to start");
            //Log.Warn("Tarkov not found, waiting for process...");
            while (User32.GetTarkovProc() == null)
                await Task.Delay(500);
        }

        public async Task<bool> Stop(bool force = false)
        {
            _stop = true;
            if (_fileSystemWatcher != null)
                _fileSystemWatcher.EnableRaisingEvents = false;

            // The process may have exited or a new directory might have popped up
            _gameStateManager.SetGameState(GameState.None);

            return await _logWatcher.Stop(force);
        }

        private void InitializeGameState()
        {
            Process? proc = User32.GetTarkovProc();

            if (proc == null)
                return;

            var dir = new FileInfo(User32.GetProcessFilename(proc)).Directory?.FullName;

            if (dir == null)
            {
                return;
            }

            _game.GameDirectory = dir;
            _currentGameLogDir = GetNewestSubdirectory(_game.LogsDirectory!);
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
                        _applicationLineHandler.Handle(line, _gameStateManager);
                        break;
                }
            }
        }

        private static string GetNewestSubdirectory(string directory)
        {
            return Directory.GetDirectories(directory).Select(s => new DirectoryInfo(s)).OrderByDescending(di => di.CreationTime).First().FullName;
        }

        private void StartDirectoryWatcher()
        {
            _fileSystemWatcher = new FileSystemWatcher(_game.LogsDirectory!)
            {
                NotifyFilter = NotifyFilters.DirectoryName
            };
            _fileSystemWatcher.Created += async (s, e) =>
            {
                _fileSystemWatcher.EnableRaisingEvents = false;

                await Stop();
                await Start();
            };
            _fileSystemWatcher.EnableRaisingEvents = true;
        }
    }
}

