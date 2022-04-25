using Microsoft.Extensions.Logging;

namespace ReadySetTarkov.Tarkov;

internal class TarkovStateManager : ITarkovStateManager
{
    private readonly ILogger<TarkovStateManager> _logger;
    private readonly ITarkovGame _game;
    private readonly ITray _tray;

    public TarkovStateManager(ILogger<TarkovStateManager> logger, ITarkovGame game, ITray tray)
    {
        _logger = logger;
        _game = game;
        _tray = tray;
    }

    public void SetGameState(GameState gameState)
    {
        _logger.LogTrace("Setting gamestate to {state}", gameState);
        _game.GameState = gameState;

        if (gameState is GameState.None or GameState.Lobby)
        {
            _tray.SetIcon("RST_red.ico");
        }

        if (gameState == GameState.InGame)
        {
            _tray.SetStatus("In a match. GLHF!");
        }
    }

    public void SetMatchmakingState(MatchmakingState matchmakingState)
    {
        _logger.LogTrace("Setting matchmaking state to {state}", matchmakingState);
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
        {
            _tray.SetIcon("RST_green.ico");
        }
    }
}

internal interface ITarkovStateManager
{
    void SetGameState(GameState gameState);
    void SetMatchmakingState(MatchmakingState matchmakingState);
}
