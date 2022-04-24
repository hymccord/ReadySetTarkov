namespace ReadySetTarkov.LogReader.Handlers;

public interface ILogFileLineHandler
{
    void Handle(LogLine line);
}
