namespace ReadySetTarkov.Utility
{
    public interface IUser32
    {
        nint FindWindow(string className, string windowName);
        void FlashWindow(nint hWnd, bool invert);
        nint GetForegroundWindow();
        int GetWindowLong(nint hWnd);
        uint GetWindowThreadProcessId(nint hWnd);
        bool IsWindow(nint hWnd);
        nint SetActiveWindow(nint hWnd);
        nint SetFocus(nint hWnd);
        bool SetForegroundWindow(nint hWnd);
        bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "PInvoke method name")]
        bool ShowWindowAsync(nint hWnd, bool showDefault);
    }
}
