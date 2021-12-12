namespace ReadySetTarkov.Utility
{
    public interface IUser32
    {
        nint FindWindow(string className, string windowName);
        bool FlashWindow(nint hWnd, bool invert);
        nint GetForegroundWindow();
        int GetWindowLong(nint hWnd);
        uint GetWindowThreadProcessId(nint hWnd);
        bool IsWindow(nint hWnd);
        bool SetForegroundWindow(nint hWnd);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "PInvoke method name")]
        bool ShowWindowAsync(nint hWnd, bool showDefault);
    }
}
