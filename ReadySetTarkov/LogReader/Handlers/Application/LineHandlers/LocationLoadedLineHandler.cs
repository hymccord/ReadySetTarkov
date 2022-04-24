using ReadySetTarkov.Tarkov;

namespace ReadySetTarkov.LogReader.Handlers.Application.LineHandlers;

internal class LocationLoadedLineHandler : IApplicationLogLineContentHandler
{
    private readonly ITarkovStateManager _gameStateManager;

    public LocationLoadedLineHandler(ITarkovStateManager gameStateManager)
    {
        _gameStateManager = gameStateManager;
    }

    public bool Handle(string lineContent)
    {
        if (!lineContent.StartsWith("LocationLoaded"))
        {
            return false;
        }

        _gameStateManager.SetMatchmakingState(MatchmakingState.Matching);

        return true;
    }
}
