namespace ReadySetTarkov.Settings
{
    class ReadySetTarkovSettings
    {
        public bool FlashTaskbar { get; set; } = true;
        public SoundSettings Sounds { get; } = new SoundSettings();
    }

    class SoundSettings
    {
        public bool MatchStart { get; set; } = true;
        public bool MatchAbort { get; set; } = true;
    }
}
