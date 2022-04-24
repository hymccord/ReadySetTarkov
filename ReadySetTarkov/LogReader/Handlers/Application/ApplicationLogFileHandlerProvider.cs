using System.Collections.Generic;

using Microsoft.Extensions.Logging;

namespace ReadySetTarkov.LogReader.Handlers.Application;

internal class ApplicationLogFileHandlerProvider : ILogFileHandlerProvider
{
    private readonly ApplicationHandler _applicationHandler;

    public ApplicationLogFileHandlerProvider(ApplicationHandler applicationHandler)
    {
        _applicationHandler = applicationHandler;
        LogFileWatcherInfo = new LogWatcherInfo("application");
    }
    public ILogFileLineHandler LogFileLineHandler => _applicationHandler;
    public LogWatcherInfo LogFileWatcherInfo { get; }
}

internal sealed class ApplicationHandler : BaseLogFileLineHandler
{
    private readonly IEnumerable<IApplicationLogLineContentHandler> _applicationLogLineHandlers;

    public ApplicationHandler(ILogger<ApplicationHandler> logger, IEnumerable<IApplicationLogLineContentHandler> applicationLogLineHandlers)
        : base(logger)
    {
        _applicationLogLineHandlers = applicationLogLineHandlers;
    }

    protected sealed override void Handle(string lineContent)
    {
        Logger.LogTrace("Application: {LineContent}", lineContent);
        foreach (IApplicationLogLineContentHandler? applicationLogLineHandler in _applicationLogLineHandlers)
        {
            applicationLogLineHandler.Handle(lineContent);
        }
    }
}
