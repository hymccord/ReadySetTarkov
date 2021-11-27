
using Windows.Win32;
using Windows.Win32.Foundation;

namespace ReadySetTarkov.Utility
{
    public class User32 : IUser32
    {
        public nint GetForegroundWindow() => PInvoke.GetForegroundWindow();

        public bool SetForegroundWindow(nint hWnd) => PInvoke.SetForegroundWindow((HWND)hWnd);

        public int GetWindowLong(nint hWnd) => PInvoke.GetWindowLong((HWND)hWnd, Windows.Win32.UI.WindowsAndMessaging.WINDOW_LONG_PTR_INDEX.GWL_STYLE);

        public void SetWindowLong()
        {
            //PInvoke.SetWindowLong(new HWND())
        }
        //public void GetClassName()
        //{
        //    PInvoke.GetClassName()
        //}

        public bool FlashWindow(nint hWnd, bool invert) => PInvoke.FlashWindow((HWND)hWnd, invert);

        public bool ShowWindow(nint hWnd, bool restore) => PInvoke.ShowWindow((HWND)hWnd, restore ? Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_RESTORE : Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_SHOW);
        public nint FindWindow(string className, string windowName) => PInvoke.FindWindow(className, windowName);

        public bool IsWindow(nint hWnd) => PInvoke.IsWindow((HWND)hWnd);

        //public void keybd_event()
        //{
        //    PInvoke.keybd_event
        //}
        public unsafe uint GetWindowThreadProcessId(nint hWnd)
        {
            uint processId = 0;
            PInvoke.GetWindowThreadProcessId((HWND)hWnd, &processId);

            return processId;
        }
    }
}
