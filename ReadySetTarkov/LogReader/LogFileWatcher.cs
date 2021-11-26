using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReadySetTarkov.LogReader
{
    public class LogFileWatcher
    {
        internal readonly LogWatcherInfo Info;
        private string? _filePath;
        private ConcurrentQueue<LogLine> _lines = new();
        private bool _logFileExists;
        private long _offset;
        private DateTime _startingPoint;

        public LogFileWatcher(LogWatcherInfo info)
        {
            Info = info;
        }

        public event Action<string>? OnLogFileFound;

        public void Start(DateTime startingPoint, string logDirectory, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var files = Directory.GetFiles(logDirectory, "*" + Info.Name + ".log");
            _filePath = Path.Combine(logDirectory, files[0]);
            _startingPoint = startingPoint;
            _offset = 0;
            _lines = new ConcurrentQueue<LogLine>();
            _logFileExists = false;
            Task.Factory.StartNew(async () => await ReadLogFileAsync(cancellationToken), cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public IEnumerable<LogLine> Collect()
        {
            var count = _lines.Count;
            for (var i = 0; i < count; i++)
            {
                if (_lines.TryDequeue(out var line))
                {
                    yield return line;
                }
            }
        }

        private async Task ReadLogFileAsync(CancellationToken cancellationToken)
        {
            await FindInitialOffsetAsync(cancellationToken);
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrEmpty(_filePath))
                {
                    throw new ArgumentNullException("Start did not find a suitable file to watch.");
                }

                var fileInfo = new FileInfo(_filePath);
                if (fileInfo.Exists)
                {
                    if (!_logFileExists)
                    {
                        _logFileExists = true;
                        OnLogFileFound?.Invoke(Info.Name);
                    }

                    using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    fs.Seek(_offset, SeekOrigin.Begin);
                    if (fs.Length <= _offset)
                    {
                        await Task.Delay(LogWatcher.UpdateDelay, cancellationToken);
                        continue;
                    }

                    using var sr = new StreamReader(fs);
                    string? line;
                    while (!sr.EndOfStream && (line = await sr.ReadLineAsync()) is not null)
                    {
                        //if (!sr.EndOfStream)
                        //    break;
                        var logLine = new LogLine(Info.Name, line);
                        if (logLine.Time >= _startingPoint)
                        {
                            _lines.Enqueue(logLine);
                        }

                        _offset += Encoding.UTF8.GetByteCount(line + Environment.NewLine);
                    }
                }

                await Task.Delay(LogWatcher.UpdateDelay, cancellationToken);
            }
        }

        private async Task FindInitialOffsetAsync(CancellationToken cancellationToken = default)
        {
            // Scan the log file backwards by taking 4k chunks and finding logs lines
            // until we get a time that is less than the starting point.
            if (string.IsNullOrEmpty(_filePath))
            {
                throw new ArgumentNullException("Start did not find a suitable file to watch.");
            }

            var fileInfo = new FileInfo(_filePath);
            if (fileInfo.Exists)
            {
                using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var sr = new StreamReader(fs, Encoding.ASCII);
                var offsetFromEndOfStream = 0;
                while (offsetFromEndOfStream < fs.Length)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    using var buffer = MemoryPool<char>.Shared.Rent(4096);
                    offsetFromEndOfStream += buffer.Memory.Length;
                    fs.Seek(Math.Max(fs.Length - offsetFromEndOfStream, 0), SeekOrigin.Begin);
                    var bytesRead = await sr.ReadBlockAsync(buffer.Memory, cancellationToken);
                    var bufferOverSize = buffer.Memory.Length - bytesRead;
                    var possiblePartialLineBytesIndex = Math.Max(buffer.Memory.Span[..bytesRead].IndexOf('\n'), 0);
                    offsetFromEndOfStream -= possiblePartialLineBytesIndex;
                    var lines = new string(buffer.Memory.Span[(possiblePartialLineBytesIndex + 1)..])
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    for (var i = lines.Length - 1; i > 0; i--)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i].Trim('\0')))
                        {
                            continue;
                        }

                        var logLine = new LogLine(Info.Name, lines[i]);
                        if (logLine.Time < _startingPoint)
                        {
                            var bytesUntilExpiredLogLine = lines[..(i + 1)].Sum(x => Encoding.UTF8.GetByteCount(x + Environment.NewLine));
                            _offset = Math.Max(fs.Length - offsetFromEndOfStream + bytesUntilExpiredLogLine + bufferOverSize, 0);
                            return;
                        }
                    }
                }
            }

            _offset = 0;
        }
    }
}
