﻿using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

using ReadySetTarkov.Tarkov;

namespace ReadySetTarkov.LogReader.Handlers.Application.LineHandlers;

internal class TraceNetworkLineHandler : IApplicationLogLineContentHandler
{
    private static readonly Regex s_traceNetwork = new(@"^TRACE-NetworkGame(?<action>\w+)\s(?<arg>\w)", RegexOptions.Compiled);
    private readonly ILogger<TraceNetworkLineHandler> _logger;
    private readonly ITarkovStateManager _gameStateManager;

    public TraceNetworkLineHandler(ILogger<TraceNetworkLineHandler> logger, ITarkovStateManager gameStateManager)
    {
        _logger = logger;
        _gameStateManager = gameStateManager;
    }

    public bool Handle(string lineContent)
    {
        Match? match = s_traceNetwork.Match(lineContent);
        if (!match.Success)
        {
            return false;
        }

        _logger.LogTrace("Handling TRACE-NetworkGame");
        string? action = match.Groups["action"].Value;
        string? arg = match.Groups["arg"].Value;
        _logger.LogTrace("Regex Groups: @{Groups}", match.Groups.Values);
        bool handled = true;
        switch (action)
        {
            // TRACE-NetworkGameMatching [GHI]
            case "Matching":
                _gameStateManager.SetGameState(GameState.Matchmaking);
                switch (arg)
                {
                    case "G":
                        _gameStateManager.SetMatchmakingState(MatchmakingState.LoadingData);
                        break;
                    case "I":
                        _gameStateManager.SetMatchmakingState(MatchmakingState.LoadingMap);
                        break;
                    default:
                        handled = false;
                        break;
                }
                break;
            case "Create":
                match = Regex.Match(lineContent, @"Sid: ([A-Z]{2}-[A-Z]*)");
                if (match.Success)
                {
                    _gameStateManager.SetServer(match.Groups[1].Value);
                }
                break;
            default:
                handled = false;
                break;
        }

        return handled;
    }
}
