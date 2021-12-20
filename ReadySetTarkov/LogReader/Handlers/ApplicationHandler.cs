using System.Text.RegularExpressions;

using ReadySetTarkov.Tarkov;

namespace ReadySetTarkov.LogReader.Handlers
{
    internal class ApplicationHandler
    {
        private static readonly Regex s_gameRegex = new(@"^Game(?<action>\w+)");
        private static readonly Regex s_traceNetwork = new(@"^TRACE-NetworkGame(?<action>\w+)\s(?<arg>\w)");
        private readonly ITarkovStateManager _gameStateManager;

        public ApplicationHandler(ITarkovStateManager gameStateManager)
        {
            _gameStateManager = gameStateManager;
        }

        internal void Handle(LogLine line)
        {
            if (string.IsNullOrEmpty(line.LineContent))
            {
                return;
            }

            if (s_gameRegex.IsMatch(line.LineContent))
            {
                var match = s_gameRegex.Match(line.LineContent);
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
                        break;
                }
            }
            else if (line.LineContent.StartsWith("LocationLoaded"))
            {
                _gameStateManager.SetMatchmakingState(MatchmakingState.Matching);
            }
            else if (s_traceNetwork.IsMatch(line.LineContent))
            {
                var match = s_traceNetwork.Match(line.LineContent);
                var action = match.Groups["action"].Value;
                var arg = match.Groups["arg"].Value;
                switch (action)
                {
                    // TRACE-NetworkGameMatching [GHI]
                    case "Matching":
                        _gameStateManager.SetGameState(GameState.Matchmaking);
                        if (arg == "G")
                        {
                            _gameStateManager.SetMatchmakingState(MatchmakingState.LoadingData);
                        }
                        else if (arg == "I")
                        {
                            _gameStateManager.SetMatchmakingState(MatchmakingState.LoadingMap);
                        }

                        break;
                    default:
                        break;
                }
            }
            else if (line.LineContent.StartsWith("SelectProfile"))
            {
                _gameStateManager.SetGameState(GameState.Lobby);
            }
            else if (line.LineContent.Contains("Network game matching aborted"))
            {
                _gameStateManager.SetGameState(GameState.Lobby);
                _gameStateManager.SetMatchmakingState(MatchmakingState.Aborted);
            }
        }
    }
}
