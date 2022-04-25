using Microsoft.Extensions.Logging;

using ReadySetTarkov.Tarkov;

namespace ReadySetTarkov.LogReader.Handlers.Application.LineHandlers;

internal class NetworkGameAbortedLineHandler : IApplicationLogLineContentHandler
{
    private readonly ILogger<NetworkGameAbortedLineHandler> _logger;
    private readonly ITarkovStateManager _gameStateManager;

    public NetworkGameAbortedLineHandler(ILogger<NetworkGameAbortedLineHandler> logger, ITarkovStateManager gameStateManager)
    {
        _logger = logger;
        _gameStateManager = gameStateManager;
    }

    public bool Handle(string lineContent)
    {
        if (!lineContent.Contains("Network game matching aborted"))
        {
            return false;
        }

        _logger.LogTrace("Handling 'Network game matching aborted'");
        _gameStateManager.SetGameState(GameState.Lobby);
        _gameStateManager.SetMatchmakingState(MatchmakingState.Aborted);

        return true;
    }
}
