using ReadySetTarkov.Settings;

namespace ReadySetTarkov.Tarkov
{
    internal class TarkovStateManager : ITarkovStateManager
    {
        private readonly ITarkovGame _game;
        private readonly ITray _tray;

        public TarkovStateManager(ITarkovGame game, ITray tray)
        {
            _game = game;
            _tray = tray;
        }

        public void SetGameState(GameState gameState)
        {
            _game.GameState = gameState;

            if (gameState == GameState.Lobby)
            {
                _tray.SetIcon("RST_red");
            }

            if (gameState == GameState.InGame)
            {
                _tray.SetStatus("In a match. GLHF!");
            }
        }

        public void SetMatchmakingState(MatchmakingState matchmakingState)
        {
            _game.MatchmakingState = matchmakingState;

            _tray.SetStatus(matchmakingState switch
            {
                MatchmakingState.LoadingMap => "Loading Map",
                MatchmakingState.LoadingData => "Loading Data",
                MatchmakingState.Matching => "Matching...",
                MatchmakingState.CreatingPools => "Creating loot pools...",
                MatchmakingState.Waiting => "Waiting for players...",
                MatchmakingState.Starting => "Match starting!",
                _ => "N/A"
            });

            if (matchmakingState == MatchmakingState.Starting)
                _tray.SetIcon("RST_green");
        }
    }

    internal interface ITarkovStateManager
    {
        void SetGameState(GameState gameState);
        void SetMatchmakingState(MatchmakingState matchmakingState);
    }
}