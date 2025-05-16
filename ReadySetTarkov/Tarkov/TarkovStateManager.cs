using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Extensions.Logging;

namespace ReadySetTarkov.Tarkov;

internal class TarkovStateManager : ITarkovStateManager
{
    private readonly ILogger<TarkovStateManager> _logger;
    private readonly ITarkovGame _game;
    private readonly ITray _tray;
    private readonly Queue<string> _serverList = [];

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

    public void SetServer(string gameInfo)
    {
        if (_serverList.Count > 4)
        {
            _serverList.Dequeue();
        }
        _serverList.Enqueue(gameInfo);

        _logger.LogTrace("Setting server info to {info}", gameInfo);

        var arr = _serverList.ToArray();
        var sb = new StringBuilder();
        sb.AppendLine("Most recent server info:");
        for (int i = 0; i < arr.Length; i++)
        {
            sb.AppendLine($"{arr.Length - i}: {arr[^(i+1)]}");
        }

        _tray.SetInfo(sb.ToString());
    }
}

internal interface ITarkovStateManager
{
    void SetGameState(GameState gameState);
    void SetMatchmakingState(MatchmakingState matchmakingState);
    void SetServer(string gameInfo);
}
