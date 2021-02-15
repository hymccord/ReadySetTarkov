namespace ReadySetTarkov.LogReader
{
    public class LogWatcherInfo
    {
        public string Name { get; set; }
        public bool Reset { get; set; } = true;

        public LogWatcherInfo(string name)
        {
            Name = name;
        }
    }
}