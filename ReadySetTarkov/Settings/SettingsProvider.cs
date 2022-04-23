using Microsoft.Extensions.Hosting;

namespace ReadySetTarkov.Settings;

internal class SettingsProvider : ISettingsProvider
{
    public SettingsProvider(IHostApplicationLifetime applicationLifetime)
    {
        Settings = AppSettings<ReadySetTarkovSettings>.Load();

        applicationLifetime.ApplicationStopping.Register(static s =>
        {
            (s as SettingsProvider)!.Save();
        }, this);
    }

    public ReadySetTarkovSettings Settings { get; }

    public void Save() => AppSettings<ReadySetTarkovSettings>.Save(Settings);
}
