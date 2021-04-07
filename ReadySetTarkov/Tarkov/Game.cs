using System.IO;

namespace ReadySetTarkov.Tarkov
{
    internal class Game : ITarkovGame
    {
        public GameState GameState { get; set; }
        public MatchmakingState MatchmakingState { get; set; }
        public bool IsRunning { get; set; }

        public string? GameDirectory { get; set; }

        public string? LogsDirectory => string.IsNullOrEmpty(GameDirectory)
                    ? null
                    : Path.Combine(GameDirectory, "Logs");
    }

    internal interface ITarkovGame
    {
        GameState GameState { get; set; }
        MatchmakingState MatchmakingState { get; set; }
        string? GameDirectory { get; set; }
        string? LogsDirectory { get; }
    }
}
