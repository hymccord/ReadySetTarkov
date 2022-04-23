namespace ReadySetTarkov.Utility;

public interface INativeMethods
{
    void BringTarkovToForeground();
    void FlashTarkov();
    string GetProcessFilename(uint processId);
    uint GetTarkovProcId();
    nint GetTarkovWindow();
    bool IsTarkovInForeground();
}
