using ReadySetTarkov.LogReader;

namespace ReadySetTarkov.Tarkov;

public interface ITrigger
{
    string TriggerKey { get; }
    bool OnLogLine(LogLine line);
}
