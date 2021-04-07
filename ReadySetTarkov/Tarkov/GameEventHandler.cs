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
        }

        private void GameStartingEventHandler(object? sender, EventArgs e)
        {
            if (_settingsProvider.Settings.FlashTaskbar)
                User32.FlashTarkov();

            if (_settingsProvider.Settings.PlaySound)
            {
                var player = new SoundPlayer(ready);
                player.Play();
            }

        }
    }
}

