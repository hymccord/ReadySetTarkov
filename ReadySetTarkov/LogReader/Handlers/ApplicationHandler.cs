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
        private static readonly Regex SelectProfile = new(@"^SelectProfile");
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
                    case "Starting":
                        User32.FlashTarkov();
                        var player = new SoundPlayer(Properties.Resources.ready);
                        player.Play();
                        break;
                    case "Start":
                        //User32.BringTarkovToForeground();
                        break;
                    default:
                        break;
                }
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
                            stateManager.SetMatchmakingState(MatchmakingState.Loading);
                        }
                        else if (arg == "I")
                        {
                            stateManager.SetMatchmakingState(MatchmakingState.Matching);
                        }
                        break;
                    // TRACE-NetworkGameCreate [0-5]
                    case "Create":
                        if (arg == "5")
                        {
                            stateManager.SetMatchmakingState(MatchmakingState.Waiting);
                        }
                        else if (arg == "6")
                        {
                            stateManager.SetGameState(GameState.InGame);
                            stateManager.SetMatchmakingState(MatchmakingState.None);
                        }
                        break;
                    default:
                        break;
                }
            }
            else if (SelectProfile.IsMatch(line.LineContent))
            {
                stateManager.SetGameState(GameState.Lobby);
            }
        }
    }
}