using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ReadySetTarkov.LogReader.Handlers;

namespace ReadySetTarkov.LogReader;

public class LogWatcher
{
    internal const int UpdateDelay = 100;
    private readonly Dictionary<string, ILogFileLineHandler> _handlers = new();
    private readonly List<LogFileWatcher> _logWatchers = new();
    private readonly ILogger<LogWatcher> _logger;

    public LogWatcher(ILogger<LogWatcher> logger, IEnumerable<ILogFileHandlerProvider> logFileWatcherProviders)
    {
        _logger = logger;

        foreach (ILogFileHandlerProvider? provider in logFileWatcherProviders)
        {
            _handlers[provider.LogFileWatcherInfo.Name] = provider.LogFileLineHandler;
            _logger.LogTrace("LogWatcher assigning {LogNamespace} to {Type}", provider.LogFileWatcherInfo.Name, provider.LogFileLineHandler.GetType());
        }

        _logger.LogDebug("Watching {Count} namespaces. @{Namespaces}", _handlers.Count, _handlers.Keys);

        _logWatchers.AddRange(_handlers.Keys.Select(x => new LogFileWatcher(x)));
    }

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
                OnNewLines(new List<LogLine>(newLines.Values.SelectMany(x => x)));
            }

            newLines.Clear();
            await Task.Delay(UpdateDelay, cancellationToken);
        }
    }

    private void OnNewLines(List<LogLine> lines)
    {
        foreach (LogLine? line in lines)
        {
            if (_handlers.TryGetValue(line.Namespace, out ILogFileLineHandler? handler))
            {
                handler.Handle(line);
            }
        }
    }
}
