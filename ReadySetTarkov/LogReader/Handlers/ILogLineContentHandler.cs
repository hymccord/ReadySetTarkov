namespace ReadySetTarkov.LogReader.Handlers;

public interface ILogLineContentHandler
{
    bool Handle(string lineContent);
}
