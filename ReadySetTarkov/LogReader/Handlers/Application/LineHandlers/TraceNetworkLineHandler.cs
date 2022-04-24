using System.Text.RegularExpressions;

using ReadySetTarkov.Tarkov;

namespace ReadySetTarkov.LogReader.Handlers.Application.LineHandlers;

internal class TraceNetworkLineHandler : IApplicationLogLineContentHandler
{
    private static readonly Regex s_traceNetwork = new(@"^TRACE-NetworkGame(?<action>\w+)\s(?<arg>\w)", RegexOptions.Compiled);
    private readonly ITarkovStateManager _gameStateManager;

    public TraceNetworkLineHandler(ITarkovStateManager gameStateManager)
    {
        _gameStateManager = gameStateManager;
    }

    public bool Handle(string lineContent)
    {
        Match? match = s_traceNetwork.Match(lineContent);
        if (!match.Success)
        {
            return false;
        }

        string? action = match.Groups["action"].Value;
        string? arg = match.Groups["arg"].Value;
        bool handled = true;
        switch (action)
        {
            // TRACE-NetworkGameMatching [GHI]
            case "Matching":
                _gameStateManager.SetGameState(GameState.Matchmaking);
                switch (arg)
                {
                    case "G":
                        _gameStateManager.SetMatchmakingState(MatchmakingState.LoadingData);
                        break;
                    case "I":
                        _gameStateManager.SetMatchmakingState(MatchmakingState.LoadingMap);
                        break;
                    default:
                        handled = false;
                        break;
                }
                break;
            default:
                handled = false;
                break;
        }

        return handled;
    }
}
