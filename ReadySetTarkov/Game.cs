using System.IO;

namespace ReadySetTarkov
{
    public class Game
    {
        public bool IsRunning { get; internal set; }

        public string? GameDirectory { get; internal set; }

        public string? LogsDirectory => string.IsNullOrEmpty(GameDirectory)
                    ? null
                    : Path.Combine(GameDirectory, "Logs");
    }
}

