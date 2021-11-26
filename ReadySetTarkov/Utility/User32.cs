using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace ReadySetTarkov.Utility
{
    public class User32
    {
        [Flags]
        public enum MouseEventFlags : uint
        {
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            RightDown = 0x00000008,
            RightUp = 0x00000010,
            Wheel = 0x00000800
        }

        [Flags]
        private enum ProcessAccessFlags : uint
        {
            QueryLimitedInformation = 0x00001000
        }

        public const int WsExTransparent = 0x00000020;
        public const int WsExToolWindow = 0x00000080;
        public const int WsExTopmost = 0x00000008;
        public const int WsExNoActivate = 0x08000000;
        private const int GwlExstyle = -20;
        private const int GwlStyle = -16;
        private const int WsMinimize = 0x20000000;
        private const int WsMaximize = 0x1000000;
        public const int SwRestore = 9;
        public const int SwShow = 5;
        private const int Alt = 0xA4;
        private const int ExtendedKey = 0x1;
        private const int KeyUp = 0x2;
        private static DateTime _lastCheck;
        private static IntPtr _tarkWindow;

        private static readonly Dictionary<IntPtr, string> WindowNameCache = new Dictionary<IntPtr, string>();

        private static readonly string[] WindowNames = { "EscapeFromTarkov" };

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

        [DllImport("user32.dll")]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryFullProcessImageName(IntPtr hProcess, int dwFlags, [Out] StringBuilder lpExeName, ref int lpdwSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

        public static bool IsTarkovInForeground() => GetForegroundWindow() == GetTarkovWindow();


        public static WindowState GetTarkovWindowState()
        {
            var tWindow = GetTarkovWindow();
            var state = GetWindowLong(tWindow, GwlStyle);
            if ((state & WsMaximize) == WsMaximize)
            {
                return WindowState.Maximized;
            }

            if ((state & WsMinimize) == WsMinimize)
            {
                return WindowState.Minimized;
            }

            return WindowState.Normal;
        }

        public static IntPtr GetTarkovWindow()
        {
            if (DateTime.Now - _lastCheck < new TimeSpan(0, 0, 5) && _tarkWindow == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            if (_tarkWindow != IntPtr.Zero)
            {
                if (IsWindow(_tarkWindow))
                {
                    return _tarkWindow;
                }

                _tarkWindow = IntPtr.Zero;
                WindowNameCache.Clear();
            }

            _tarkWindow = FindWindow("UnityWndClass", "EscapeFromTarkov");

            if (_tarkWindow != IntPtr.Zero)
            {
                return _tarkWindow;
            }

            foreach (var windowName in WindowNames)
            {
                _tarkWindow = FindWindow("UnityWndClass", windowName);
                if (_tarkWindow == IntPtr.Zero)
                {
                    continue;
                }
                //if (Config.Instance.TarkovWindowName != windowName)
                //{
                //	Config.Instance.TarkovWindowName = windowName;
                //	Config.Save();
                //}
                break;
            }

            _lastCheck = DateTime.Now;
            return _tarkWindow;
        }

        public static Process? GetTarkovProc()
        {
            if (_tarkWindow == IntPtr.Zero)
            {
                return null;
            }

            try
            {
                GetWindowThreadProcessId(_tarkWindow, out var procId);
                return Process.GetProcessById((int)procId);
            }
            catch
            {
                return null;
            }
        }

        public static void BringTarkovToForeground()
        {
            var tHandle = GetTarkovWindow();
            if (tHandle == IntPtr.Zero)
            {
                return;
            }

            ActivateWindow(tHandle);
            SetForegroundWindow(tHandle);
        }

        public static void FlashTarkov() => FlashWindow(GetTarkovWindow(), false);

        //http://www.roelvanlisdonk.nl/?p=4032
        public static void ActivateWindow(IntPtr mainWindowHandle)
        {
            // Guard: check if window already has focus.
            if (mainWindowHandle == GetForegroundWindow())
            {
                return;
            }

            // Show window maximized.
            ShowWindow(mainWindowHandle, GetTarkovWindowState() == WindowState.Minimized ? SwRestore : SwShow);

            // Simulate an "ALT" key press.
            keybd_event(Alt, 0x45, ExtendedKey | 0, 0);

            // Simulate an "ALT" key release.
            keybd_event(Alt, 0x45, ExtendedKey | KeyUp, 0);

            // Show window in forground.
            SetForegroundWindow(mainWindowHandle);
        }

        public static string GetProcessFilename(Process p)
        {
            var capacity = 2000;
            var builder = new StringBuilder(capacity);
            var ptr = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, p.Id);
            if (!QueryFullProcessImageName(ptr, 0, builder, ref capacity))
            {
                return string.Empty;
            }

            return builder.ToString();
        }
    }
}
