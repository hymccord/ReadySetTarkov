using Microsoft.Extensions.Logging;

namespace ReadySetTarkov.LogReader.Handlers;

internal abstract class BaseLogFileLineHandler : ILogFileLineHandler
{
    protected ILogger Logger { get; }

    protected BaseLogFileLineHandler(ILogger logger) => Logger = logger;

    void ILogFileLineHandler.Handle(LogLine line)
    {
        if (string.IsNullOrEmpty(line.LineContent))
        {
            return;
        }

        Handle(line.LineContent);
    }

    protected abstract void Handle(string lineContent);
}
