using System.Threading.Tasks;

namespace ReadySetTarkov.Utility;

public interface INativeMethods
{
    Task BringTarkovToForegroundAsync();
    void FlashTarkov();
    string GetProcessFilename(uint processId);
    uint GetTarkovProcId();
    nint GetTarkovWindow();
    bool IsTarkovInForeground();
}
