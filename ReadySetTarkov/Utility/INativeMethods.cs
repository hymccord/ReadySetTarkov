using System;
using System.Windows;

namespace ReadySetTarkov.Utility
{
    public interface INativeMethods
    {
        void ActivateWindow(IntPtr mainWindowHandle);
        void BringTarkovToForeground();
        void FlashTarkov();
        string GetProcessFilename(uint processId);
        uint GetTarkovProcId();
        nint GetTarkovWindow();
        WindowState GetTarkovWindowState();
        bool IsTarkovInForeground();
    }
}
