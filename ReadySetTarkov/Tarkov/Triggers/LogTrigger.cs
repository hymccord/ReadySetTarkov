using System.Text.RegularExpressions;
using ReadySetTarkov.LogReader;

namespace ReadySetTarkov.Tarkov;

internal abstract class LogTrigger : ITrigger
{
    public abstract string TriggerKey { get; }
    public abstract bool OnLogLine(LogLine line);
}

internal abstract class ApplicationLogTrigger : LogTrigger
{
    public override bool OnLogLine(LogLine line)
    {
        if (string.IsNullOrEmpty(line.LineContent))
        {
            return false;
        }

        if (line.Namespace != "Application")
        {
            return false;
        }

        return OnLogLineImpl(line);
    }

    protected abstract bool OnLogLineImpl(LogLine line);
}

internal abstract class GameAppLogTrigger : ApplicationLogTrigger
{
    private static readonly Regex s_gameRegex = new(@"^Game(?<action>\w+)");

    protected sealed override bool OnLogLineImpl(LogLine line)
    {
        var match = s_gameRegex.Match(line.LineContent!);
        if (match.Success)
        {
            return OnGameLine(match);
        }

        return false;
    }

    protected abstract bool OnGameLine(Match match);
}

internal sealed class GameSpawnTrigger : GameAppLogTrigger
{
    public override string TriggerKey => "GameSpawn";

    protected sealed override bool OnGameLine(Match match)
    {
        return match.Groups["action"].Value == "Spawn";
    }
}

internal sealed class GameStartingTrigger : GameAppLogTrigger
{
    public override string TriggerKey => "GameStarting";
    protected sealed override bool OnGameLine(Match match)
    {
        return match.Groups["action"].Value == "Starting";
    }
}

internal sealed class GameStartedTrigger : GameAppLogTrigger
{
    public override string TriggerKey => "GameStarted";
    protected sealed override bool OnGameLine(Match match)
    {
        return match.Groups["action"].Value == "Started";
    }
}

internal sealed class LocationLoadedTrigger : ApplicationLogTrigger
{
    public override string TriggerKey => "LocationLoaded";
    protected override bool OnLogLineImpl(LogLine line)
    {
        return line.LineContent!.StartsWith("LocationLoaded");
    }
}

internal abstract class TraceNetworkTrigger : ApplicationLogTrigger
{
    private static readonly Regex s_traceNetwork = new(@"^TRACE-NetworkGame(?<action>\w+)\s(?<arg>\w)");
    protected sealed override bool OnLogLineImpl(LogLine line)
    {
        var match = s_traceNetwork.Match(line.LineContent!);
        if (match.Success)
        {
            return OnTraceNetwork(match);
        }

        return false;
    }

    protected abstract bool OnTraceNetwork(Match match);
}

internal sealed class MatchLoadingDataTrigger : TraceNetworkTrigger
{
    public override string TriggerKey => "LoadingData";
    protected override bool OnTraceNetwork(Match match)
    {
        return match.Groups["arg"].Value == "G";
    }
}

internal sealed class MatchLoadingMapTrigger : TraceNetworkTrigger
{
    public override string TriggerKey => "LoadingMap";
    protected override bool OnTraceNetwork(Match match)
    {
        return match.Groups["arg"].Value == "I";
    }
}

internal sealed class BackMainMenuTrigger : ApplicationLogTrigger
{
    public override string TriggerKey => "DoneWithPostRaidScreen";
    protected override bool OnLogLineImpl(LogLine line)
    {
        return line.LineContent!.StartsWith("SelectProfile");
    }
}

internal sealed class MatchmakingAbortedTrigger : ApplicationLogTrigger
{
    public override string TriggerKey => "MatchmakingAborted";
    protected override bool OnLogLineImpl(LogLine line)
    {
        return line.LineContent!.Contains("Network game matching aborted");
    }
}
