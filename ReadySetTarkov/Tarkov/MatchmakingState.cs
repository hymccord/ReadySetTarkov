namespace ReadySetTarkov.Tarkov
{
    internal enum MatchmakingState
    {
        None,
        LoadingData,
        LoadingMap,
        Matching,
        CreatingPools,
        Waiting,
        Starting,
        Started,
        Aborted,
        Canceled
    }
}
