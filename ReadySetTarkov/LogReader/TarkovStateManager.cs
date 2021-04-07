using ReadySetTarkov.Tarkov;

namespace ReadySetTarkov.LogReader
{
    internal class TarkovStateManager : ITarkovStateManager
    {
        private ITarkovGame game;
        private ITray tray;

        public TarkovStateManager(ITarkovGame game, ITray tray)
        {
            this.game = game;
            this.tray = tray;
        }

        public void SetGameState(GameState gameState)
        {
            game.GameState = gameState;

            if (gameState == GameState.Matchmaking)
            {
                tray.SetIcon("RST_red");
            }
        }

        public void SetMatchmakingState(MatchmakingState matchmakingState)
        {
            game.MatchmakingState = matchmakingState;

            tray.SetStatus(matchmakingState switch
            {
                MatchmakingState.Loading => "Loading...",
                MatchmakingState.Matching => "Looking for match...",
                MatchmakingState.Waiting => "Waiting for players...",
                MatchmakingState.Starting => "Match starting!",
                _ => "N/A"
            });

            if (matchmakingState == MatchmakingState.Starting)
                tray.SetIcon("RST_green");
        }
    }

    internal interface ITarkovStateManager
    {
        void SetGameState(GameState gameState);
        void SetMatchmakingState(MatchmakingState matchmakingState);
    }
}