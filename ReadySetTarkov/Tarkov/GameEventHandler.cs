using System;
using System.Media;

using ReadySetTarkov.Settings;
using ReadySetTarkov.Utility;

using static ReadySetTarkov.Properties.Resources;

namespace ReadySetTarkov.Tarkov
{
    internal class GameEventHandler
    {
        private readonly ISettingsProvider _settingsProvider;
        private readonly IGameEvents _tarkov;

        public GameEventHandler(ISettingsProvider settingsProvider, IGameEvents tarkov)
        {
            _settingsProvider = settingsProvider;
            _tarkov = tarkov;

            _tarkov.GameStarting += GameStartingEventHandler;
            _tarkov.MatchmakingAborted += MatchmakingAbortedHandler;
        }

        private void GameStartingEventHandler(object? sender, EventArgs e)
        {
            if (_settingsProvider.Settings.FlashTaskbar)
            {
                User32.FlashTarkov();
            }

            if (_settingsProvider.Settings.Sounds.MatchStart)
            {
                var player = new SoundPlayer(ready);
                player.Play();
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
    }
}

