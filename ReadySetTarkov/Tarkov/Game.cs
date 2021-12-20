using System;
using System.IO;

namespace ReadySetTarkov.Tarkov
{
    internal class Game : ITarkovGame, IGameEvents
    {
        private MatchmakingState _matchmakingState;

        public Game()
        { }

        public GameState GameState { get; set; }
        public MatchmakingState MatchmakingState
        {
            get => _matchmakingState;
            set
            {
                if (_matchmakingState == value)
                {
                    return;
                }

                _matchmakingState = value;

                if (_matchmakingState == MatchmakingState.Starting)
                {
                    GameStarting?.Invoke(this, EventArgs.Empty);
                }

                if (_matchmakingState == MatchmakingState.Started)
                {
                    GameStarted?.Invoke(this, EventArgs.Empty);
                }

                if (_matchmakingState == MatchmakingState.Aborted)
                {
                    MatchmakingAborted?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public string? GameDirectory { get; set; }

        public string? LogsDirectory => string.IsNullOrEmpty(GameDirectory)
                    ? null
                    : Path.Combine(GameDirectory, "Logs");

        public event EventHandler? GameStarting;
        public event EventHandler? GameStarted;
        public event EventHandler? MatchmakingAborted;
    }

    internal interface IGameEvents
    {
        event EventHandler? GameStarting;
        event EventHandler? GameStarted;
        event EventHandler? MatchmakingAborted;
    }

    internal interface ITarkovGame
    {
        GameState GameState { get; set; }
        MatchmakingState MatchmakingState { get; set; }
        string? GameDirectory { get; set; }
        string? LogsDirectory { get; }
    }
}
