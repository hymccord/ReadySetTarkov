using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReadySetTarkov.LogReader;

public class LogFileWatcher
{
    private readonly string _logName;
    private string? _filePath;
    private ConcurrentQueue<LogLine> _lines = new();
    private bool _logFileExists;
    private long _offset;
    private DateTime _startingPoint;

    public LogFileWatcher(string name) => _logName = name;

    public event Action<string>? OnLogFileFound;

    public void Start(DateTime startingPoint, string logDirectory, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        string[]? files = Directory.GetFiles(logDirectory, $"*{_logName}.log");
        _filePath = Path.Combine(logDirectory, files[0]);
        _startingPoint = startingPoint;
        _offset = 0;
        _lines = new ConcurrentQueue<LogLine>();
        _logFileExists = false;
        _ = Task.Factory.StartNew(async ()
            => await ReadLogFileAsync(cancellationToken), cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    public IEnumerable<LogLine> Collect()
    {
        int count = _lines.Count;
        for (int i = 0; i < count; i++)
        {
            if (_lines.TryDequeue(out LogLine? line))
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
                throw new InvalidOperationException("Start did not find a suitable file to watch.");
            }

            var fileInfo = new FileInfo(_filePath);
            if (fileInfo.Exists)
            {
                if (!_logFileExists)
                {
                    _logFileExists = true;
                    OnLogFileFound?.Invoke(_logName);
                }

                using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                _ = fs.Seek(_offset, SeekOrigin.Begin);
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
                    var logLine = new LogLine(_logName, line);
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
            throw new InvalidOperationException("Start did not find a suitable file to watch.");
        }

        var fileInfo = new FileInfo(_filePath);
        if (fileInfo.Exists)
        {
            using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var sr = new StreamReader(fs, Encoding.ASCII);
            int offsetFromEndOfStream = 0;
            while (offsetFromEndOfStream < fs.Length)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using IMemoryOwner<char>? buffer = MemoryPool<char>.Shared.Rent(4096);
                offsetFromEndOfStream += buffer.Memory.Length;
                _ = fs.Seek(Math.Max(fs.Length - offsetFromEndOfStream, 0), SeekOrigin.Begin);
                int bytesRead = await sr.ReadBlockAsync(buffer.Memory, cancellationToken);
                int bufferOverSize = buffer.Memory.Length - bytesRead;
                int possiblePartialLineBytesIndex = Math.Max(buffer.Memory.Span[..bytesRead].IndexOf('\n'), 0);
                offsetFromEndOfStream -= possiblePartialLineBytesIndex;
                string[]? lines = new string(buffer.Memory.Span[(possiblePartialLineBytesIndex + 1)..])
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                for (int i = lines.Length - 1; i > 0; i--)
                {
                    if (string.IsNullOrWhiteSpace(lines[i].Trim('\0')))
                    {
                        continue;
                    }

                    var logLine = new LogLine(_logName, lines[i]);
                    if (logLine.Time < _startingPoint)
                    {
                        int bytesUntilExpiredLogLine = lines[..(i + 1)].Sum(x => Encoding.UTF8.GetByteCount(x + Environment.NewLine));
                        _offset = Math.Max(fs.Length - offsetFromEndOfStream + bytesUntilExpiredLogLine + bufferOverSize, 0);
                        return;
                    }
                }
            }
        }

        _offset = 0;
    }
}
