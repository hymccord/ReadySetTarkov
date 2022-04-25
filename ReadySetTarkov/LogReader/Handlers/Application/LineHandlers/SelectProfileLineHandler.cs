using Microsoft.Extensions.Logging;

using ReadySetTarkov.Tarkov;

namespace ReadySetTarkov.LogReader.Handlers.Application.LineHandlers;

internal class SelectProfileLineHandler : IApplicationLogLineContentHandler
{
    private readonly ILogger<SelectProfileLineHandler> _logger;
    private readonly ITarkovStateManager _gameStateManager;

    public SelectProfileLineHandler(ILogger<SelectProfileLineHandler> logger, ITarkovStateManager gameStateManager)
    {
        _logger = logger;
        _gameStateManager = gameStateManager;
    }

    public bool Handle(string lineContent)
    {
        if (!lineContent.StartsWith("SelectProfile"))
        {
            return false;
        }

        _logger.LogTrace("Handling SelectProfile");
        _gameStateManager.SetGameState(GameState.Lobby);

        return true;
    }
}
