namespace ReadySetTarkov.Settings
{
    internal class ReadySetTarkovSettings
    {
        public bool FlashTaskbar { get; set; } = true;
        public bool SetTopMost { get; set; } = true;
        public int WithSecondsLeft { get; set; } = 0;
        public SoundSettings Sounds { get; } = new SoundSettings();
    }

    internal class SoundSettings
    {
        public bool MatchStart { get; set; } = true;
        public bool MatchAbort { get; set; } = true;
    }
}
