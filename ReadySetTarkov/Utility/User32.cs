
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace ReadySetTarkov.Utility;

internal class User32 : IUser32
{
    public User32(ISystemParametersInfo systemParametersInfo, IWindowForegrounding windowForegrounding)
    {
        SystemParametersInfo = systemParametersInfo;
        WindowForegrounding = windowForegrounding;
    }

    public ISystemParametersInfo SystemParametersInfo { get; }
    public IWindowForegrounding WindowForegrounding { get; }

    public void AttachThreadInput(uint idAttach, uint to, bool attach) => PInvoke.AttachThreadInput(idAttach, to, attach);

    public bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags) => PInvoke.SetWindowPos((HWND)hWnd, (HWND)hWndInsertAfter, x, y, cx, cy, (SET_WINDOW_POS_FLAGS)uFlags);

    public nint SetActiveWindow(nint hWnd) => PInvoke.SetActiveWindow((HWND)hWnd);

    public nint SetFocus(nint hWnd) => PInvoke.SetFocus((HWND)hWnd);

    public int GetWindowLong(nint hWnd)
        => PInvoke.GetWindowLong(
            (HWND)hWnd,
            WINDOW_LONG_PTR_INDEX.GWL_STYLE);

    public void FlashWindow(nint hWnd, bool invert)
        => PInvoke.FlashWindow(
            (HWND)hWnd,
            invert);

    public bool ShowWindowAsync(nint hWnd, bool showDefault)
        => PInvoke.ShowWindowAsync(
            (HWND)hWnd,
            showDefault
            ? SHOW_WINDOW_CMD.SW_SHOWDEFAULT
            : SHOW_WINDOW_CMD.SW_SHOW);

    public nint FindWindow(string className, string windowName) => PInvoke.FindWindow(className, windowName);

    public bool IsWindow(nint hWnd) => PInvoke.IsWindow((HWND)hWnd);

    public unsafe (uint threadId, uint processId) GetWindowThreadProcessId(nint hWnd)
    {
        uint processId = 0;
        var threadId = PInvoke.GetWindowThreadProcessId((HWND)hWnd, &processId);
        return (threadId, processId);
    }

    public bool ShowWindow(nint hWnd, SHOW_WINDOW_CMD nCmdShow) => PInvoke.ShowWindow((HWND)hWnd, nCmdShow);

    public bool IsIconic(nint hWnd) => PInvoke.IsIconic((HWND)hWnd);
}
