using ReadySetTarkov.LogReader.Handlers;

namespace ReadySetTarkov.LogReader;

public interface ILogFileHandlerProvider
{
    LogWatcherInfo LogFileWatcherInfo { get; }
    ILogFileLineHandler LogFileLineHandler { get; }
}
