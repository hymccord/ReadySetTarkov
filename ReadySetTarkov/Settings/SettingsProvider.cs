﻿namespace ReadySetTarkov.Settings;

internal class SettingsProvider : ISettingsProvider
{
    public SettingsProvider() => Settings = AppSettings<ReadySetTarkovSettings>.Load();

    public ReadySetTarkovSettings Settings { get; }

    public void Save() => AppSettings<ReadySetTarkovSettings>.Save(Settings);
}
