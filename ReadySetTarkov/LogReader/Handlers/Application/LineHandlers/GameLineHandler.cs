using System.Text.RegularExpressions;

using ReadySetTarkov.Tarkov;

namespace ReadySetTarkov.LogReader.Handlers.Application.LineHandlers;

internal class GameLineHandler : IApplicationLogLineContentHandler
{
    private static readonly Regex s_gameRegex = new(@"^Game(?<action>\w+)");
    private readonly ITarkovStateManager _gameStateManager;

    public GameLineHandler(ITarkovStateManager gameStateManager)
    {
        _gameStateManager = gameStateManager;
    }

    public bool Handle(string lineContent)
    {
        Match? match = s_gameRegex.Match(lineContent);
        if (!match.Success)
        {
            return false;
        }

        bool handled = true;
        // Game[Pooled|Runned|Spawn|Spawned|Starting|Started]
        switch (match.Groups["action"].Value)
        {
            case "Spawn":
                _gameStateManager.SetMatchmakingState(MatchmakingState.Waiting);
                break;
            case "Starting":
                _gameStateManager.SetMatchmakingState(MatchmakingState.Starting);
                break;
            case "Started":
                _gameStateManager.SetMatchmakingState(MatchmakingState.Started);
                _gameStateManager.SetGameState(GameState.InGame);
                _gameStateManager.SetMatchmakingState(MatchmakingState.None);
                break;
            default:
                handled = false;
                break;
        }

        return handled;
    }
}
