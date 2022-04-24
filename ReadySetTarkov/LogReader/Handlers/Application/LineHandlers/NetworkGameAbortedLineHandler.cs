using ReadySetTarkov.Tarkov;

namespace ReadySetTarkov.LogReader.Handlers.Application.LineHandlers;

internal class NetworkGameAbortedLineHandler : IApplicationLogLineContentHandler
{
    private readonly ITarkovStateManager _gameStateManager;

    public NetworkGameAbortedLineHandler(ITarkovStateManager gameStateManager)
    {
        _gameStateManager = gameStateManager;
    }

    public bool Handle(string lineContent)
    {
        if (!lineContent.Contains("Network game matching aborted"))
        {
            return false;
        }

        _gameStateManager.SetGameState(GameState.Lobby);
        _gameStateManager.SetMatchmakingState(MatchmakingState.Aborted);

        return true;
    }
}
