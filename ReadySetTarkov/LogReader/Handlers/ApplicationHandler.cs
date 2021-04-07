using System.Media;
using System.Text.RegularExpressions;

using ReadySetTarkov.Tarkov;
using ReadySetTarkov.Utility;

namespace ReadySetTarkov.LogReader.Handlers
{
    internal class ApplicationHandler
    {
        private static readonly Regex GameRegex = new(@"^Game(?<action>\w+)");
        private static readonly Regex TraceNetwork = new(@"^TRACE-NetworkGame(?<action>\w+)\s(?<arg>\w)");
        public ApplicationHandler()
        {
        }

        internal void Handle(LogLine line, ITarkovStateManager stateManager)
        {
            if (string.IsNullOrEmpty(line.LineContent))
                return;

            if (GameRegex.IsMatch(line.LineContent))
            {
                var match = GameRegex.Match(line.LineContent);
                switch (match.Groups["action"].Value)
                {
                    case "Spawn":
                        stateManager.SetMatchmakingState(MatchmakingState.Waiting);
                        break;
                    case "Starting":
                        stateManager.SetMatchmakingState(MatchmakingState.Starting);
                        break;
                    case "Started":
                        stateManager.SetGameState(GameState.InGame);
                        stateManager.SetMatchmakingState(MatchmakingState.None);
                        break;
                    default:
                        break;
                }
            }
            else if (line.LineContent.StartsWith("LocationLoaded"))
            {
                stateManager.SetMatchmakingState(MatchmakingState.Matching);
            }
            else if (TraceNetwork.IsMatch(line.LineContent))
            {
                var match = TraceNetwork.Match(line.LineContent);
                var action = match.Groups["action"].Value;
                var arg = match.Groups["arg"].Value;
                switch (action)
                {
                    // TRACE-NetworkGameMatching [GHI]
                    case "Matching":
                        stateManager.SetGameState(GameState.Matchmaking);
                        if (arg == "G")
                        {
                            stateManager.SetMatchmakingState(MatchmakingState.LoadingData);
                        }
                        else if (arg == "I")
                        {
                            stateManager.SetMatchmakingState(MatchmakingState.LoadingMap);
                        }
                        break;
                    default:
                        break;
                }
            }
            else if (line.LineContent.StartsWith("SelectProfile"))
            {
                stateManager.SetGameState(GameState.Lobby);
            }
        }
    }
}