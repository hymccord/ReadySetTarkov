using Windows.Win32;
using Windows.Win32.Foundation;

namespace ReadySetTarkov.Utility;

internal interface IUser32
{
    void AttachThreadInput(uint attachId, uint attachToId, bool attach);
    nint FindWindow(string className, string windowName);
    void FlashWindow(nint hWnd, bool invert);
    int GetWindowLong(nint hWnd);
    (uint threadId, uint processId) GetWindowThreadProcessId(nint hWnd);
    bool IsWindow(nint hWnd);
    nint SetActiveWindow(nint hWnd);
    nint SetFocus(nint hWnd);
    bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "PInvoke method name")]
    bool ShowWindowAsync(nint hWnd, bool showDefault);
    bool ShowWindow(nint hWnd, Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD nCmdShow);
    bool IsIconic(nint hWnd);

    ISystemParametersInfo SystemParametersInfo { get; }
    IWindowForegrounding WindowForegrounding { get; }
}

internal interface ISystemParametersInfo
{
    uint GetForegroundLockTimeout();
    void SetForegroundLockTimeout(uint timeout);
}

class SystemParameters : ISystemParametersInfo
{
    public unsafe uint GetForegroundLockTimeout()
    {
        uint timeout = 0;
        PInvoke.SystemParametersInfo(Windows.Win32.UI.WindowsAndMessaging.SYSTEM_PARAMETERS_INFO_ACTION.SPI_GETFOREGROUNDLOCKTIMEOUT, 0, &timeout, 0);
        return timeout;
    }
    public unsafe void SetForegroundLockTimeout(uint timeout)
    {
        PInvoke.SystemParametersInfo(Windows.Win32.UI.WindowsAndMessaging.SYSTEM_PARAMETERS_INFO_ACTION.SPI_SETFOREGROUNDLOCKTIMEOUT, 0, &timeout, 0);
    }
}

internal interface IWindowForegrounding
{
    void AllowSetForegroundWindow();
    nint GetForegroundWindow();
    bool SetForegroundWindow(nint hWnd);
    void SetForegroundWindowLock();
    void SetForegroundWindowUnlock();
}

/// <summary>
/// User32 invokes related to foreground window methods
/// </summary>
class WindowForegrounding : IWindowForegrounding
{
    public void AllowSetForegroundWindow() => PInvoke.AllowSetForegroundWindow(0xFFFF_FFFF /*ASFW_ANY*/);
    public nint GetForegroundWindow() => PInvoke.GetForegroundWindow();
    public bool SetForegroundWindow(nint hWnd) => PInvoke.SetForegroundWindow((HWND)hWnd);
    public void SetForegroundWindowLock() => PInvoke.LockSetForegroundWindow(Windows.Win32.UI.WindowsAndMessaging.FOREGROUND_WINDOW_LOCK_CODE.LSFW_LOCK);
    public void SetForegroundWindowUnlock() => PInvoke.LockSetForegroundWindow(Windows.Win32.UI.WindowsAndMessaging.FOREGROUND_WINDOW_LOCK_CODE.LSFW_UNLOCK);
}
