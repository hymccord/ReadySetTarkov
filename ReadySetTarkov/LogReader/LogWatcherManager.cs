using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using ReadySetTarkov.Utility;

namespace ReadySetTarkov.LogReader
{
    internal class LogWatcherManager
    {
        private readonly ApplicationHandler _applicationLineHandler = new ApplicationHandler();
        private readonly LogWatcher _logWatcher;
        private bool _stop;
        private Game? _game;
        private string? _logsDir;

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
            //if (!Helper.TarkovDirExists)
            await FindTarkov();
            InitializeGameState(game);
            _stop = false;
            //var logDirectory = Path.Combine(Config.Instance.TarkovDirectory, Config.Instance.TarkovLogsDirectoryName);
            _logWatcher.Start(_logsDir);
        }

        private async Task FindTarkov()
        {
            //Log.Warn("Tarkov not found, waiting for process...");
            Process? proc;
            while ((proc = User32.GetTarkovProc()) == null)
                await Task.Delay(500);
            var dir = new FileInfo(User32.GetProcessFilename(proc)).Directory?.FullName;

            if (dir == null)
            {
                //const string msg = "Could not find Tarkov installation";
                //Log.Error(msg);
                //ErrorManager.AddError(msg, "Please point HDT to your Tarkov installation via 'options > tracker > settings > set Tarkov path'.");
                return;
            }
            _logsDir = Directory.GetDirectories(Path.Combine(dir, "Logs")).Select(s => new DirectoryInfo(s)).OrderByDescending(di => di.CreationTime).First().FullName;
            //Config.Instance.TarkovDirectory = dir;
            //Config.Save();
        }

        public async Task<bool> Stop(bool force = false)
        {
            _stop = true;
            return await _logWatcher.Stop(force);
        }

        private void InitializeGameState(Game game)
        {
            _game = game;
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
    }
}

