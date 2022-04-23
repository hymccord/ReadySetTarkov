using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReadySetTarkov.LogReader;

public class LogWatcher
{
    internal const int UpdateDelay = 100;
    private readonly List<LogFileWatcher> _logWatchers = new();

    public LogWatcher(IEnumerable<LogWatcherInfo> logReaderInfos)
    {
        _logWatchers.AddRange(logReaderInfos.Select(x => new LogFileWatcher(x)));
        foreach (LogFileWatcher? watcher in _logWatchers)
        {
            watcher.OnLogFileFound += (msg) => OnLogFileFound?.Invoke(msg);
        }
    }

    public event Action<List<LogLine>>? OnNewLines;
    public event Action<string>? OnLogFileFound;

    public async Task WatchAsync(string logDirectory, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        DateTime startingPoint = DateTime.Now;
        foreach (LogFileWatcher? logReader in _logWatchers)
        {
            logReader.Start(startingPoint, logDirectory, cancellationToken);
        }

        var newLines = new SortedList<DateTime, List<LogLine>>();
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (LogFileWatcher? logReader in _logWatchers)
            {
                IEnumerable<LogLine>? lines = logReader.Collect();
                foreach (LogLine? line in lines)
                {
                    if (!newLines.TryGetValue(line.Time, out List<LogLine>? logLines))
                    {
                        newLines.Add(line.Time, logLines = new List<LogLine>());
                    }

                    logLines.Add(line);
                }
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                OnNewLines?.Invoke(new List<LogLine>(newLines.Values.SelectMany(x => x)));
            }

            newLines.Clear();
            await Task.Delay(UpdateDelay, cancellationToken);
        }
    }
}
