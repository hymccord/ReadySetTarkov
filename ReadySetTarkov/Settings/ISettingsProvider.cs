namespace ReadySetTarkov.Settings
{
    internal interface ISettingsProvider
    {
        ReadySetTarkovSettings Settings { get; }
        void Save();
    }
}
