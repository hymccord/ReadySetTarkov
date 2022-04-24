using ReadySetTarkov.Tarkov;

namespace ReadySetTarkov.LogReader.Handlers.Application.LineHandlers;

internal class SelectProfileLineHandler : IApplicationLogLineContentHandler
{
    private readonly ITarkovStateManager _gameStateManager;

    public SelectProfileLineHandler(ITarkovStateManager gameStateManager)
    {
        _gameStateManager = gameStateManager;
    }

    public bool Handle(string lineContent)
    {
        if (!lineContent.StartsWith("SelectProfile"))
        {
            return false;
        }

        _gameStateManager.SetGameState(GameState.Lobby);

        return true;
    }
}
