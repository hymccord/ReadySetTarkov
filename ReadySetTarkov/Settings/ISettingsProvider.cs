namespace ReadySetTarkov.Settings
{
    public interface ISettingsProvider
    {
        ReadySetTarkovSettings Settings { get; }
        void Save();
    }
}
