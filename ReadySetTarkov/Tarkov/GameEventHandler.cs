using System;
using System.Media;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using ReadySetTarkov.Settings;
using ReadySetTarkov.Utility;

using static ReadySetTarkov.Properties.Resources;

namespace ReadySetTarkov.Tarkov;

internal class GameEventHandler : IHostedService
{
    private readonly ISettingsProvider _settingsProvider;
    private readonly IGameEvents _tarkov;
    private readonly INativeMethods _nativeMethods;

    public GameEventHandler(ISettingsProvider settingsProvider, IGameEvents tarkov, INativeMethods nativeMethods)
    {
        _settingsProvider = settingsProvider;
        _tarkov = tarkov;
        _nativeMethods = nativeMethods;
    }

#pragma warning disable VSTHRD100 // Avoid async void methods
    private async void GameStartingEventHandler(object? sender, EventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
    {
        if (_settingsProvider.Settings.FlashTaskbar)
        {
            _nativeMethods.FlashTarkov();
        }

        if (_settingsProvider.Settings.Sounds.MatchStart)
        {
            var player = new SoundPlayer(ready);
            player.Play();
        }

        if (_settingsProvider.Settings.WithSecondsLeft > 0)
        {
            // Going to assume 10 seconds, there's no log way to get the amount of time remaining.
            await Task.Delay((10 * 1000) - (_settingsProvider.Settings.WithSecondsLeft * 1000));
            await _nativeMethods.BringTarkovToForegroundAsync();
        }
    }

    private void GameStartedEventHandler(object? sender, EventArgs e)
    {
        if (_settingsProvider.Settings.WithSecondsLeft == 0)
        {
            _ = _nativeMethods.BringTarkovToForegroundAsync();
        }
    }

    private void MatchmakingAbortedHandler(object? sender, EventArgs e)
    {
        if (_settingsProvider.Settings.Sounds.MatchAbort)
        {
            var player = new SoundPlayer(error);
            player.Play();
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _tarkov.GameStarting += GameStartingEventHandler;
        _tarkov.GameStarted += GameStartedEventHandler;
        _tarkov.MatchmakingAborted += MatchmakingAbortedHandler;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _tarkov.GameStarting -= GameStartingEventHandler;
        _tarkov.GameStarted -= GameStartedEventHandler;
        _tarkov.MatchmakingAborted -= MatchmakingAbortedHandler;

        return Task.CompletedTask;
    }
}

