using Microsoft.Extensions.Logging;

using ReadySetTarkov.Tarkov;

namespace ReadySetTarkov.LogReader.Handlers.Application.LineHandlers;

internal class LocationLoadedLineHandler : IApplicationLogLineContentHandler
{
    private readonly ILogger<LocationLoadedLineHandler> _logger;
    private readonly ITarkovStateManager _gameStateManager;

    public LocationLoadedLineHandler(ILogger<LocationLoadedLineHandler> logger, ITarkovStateManager gameStateManager)
    {
        _logger = logger;
        _gameStateManager = gameStateManager;
    }

    public bool Handle(string lineContent)
    {
        if (!lineContent.StartsWith("LocationLoaded"))
        {
            return false;
        }

        _logger.LogTrace("Handling LocationLoaded");
        _gameStateManager.SetMatchmakingState(MatchmakingState.Matching);

        return true;
    }
}
