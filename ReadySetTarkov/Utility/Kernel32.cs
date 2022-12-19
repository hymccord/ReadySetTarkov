using System;
using System.Runtime.InteropServices;
using System.Text;

using Windows.Win32;
using Windows.Win32.System.Threading;

namespace ReadySetTarkov.Utility;

public interface IKernel32
{
    SafeHandle OpenProcess(bool inheritHandle, uint processId);
    string QueryFullProcessImageName(SafeHandle process);
    uint GetCurrentThreadId();
}

public class Kernel32 : IKernel32
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool QueryFullProcessImageName(IntPtr hProcess, int dwFlags, [Out] StringBuilder lpExeName, ref int lpdwSize);

    public string QueryFullProcessImageName(SafeHandle process)
    {
        int capacity = 2000;
        var builder = new StringBuilder(capacity);
        return !QueryFullProcessImageName(process.DangerousGetHandle(), 0, builder, ref capacity) ? string.Empty : builder.ToString();
        //var capacity = 2000u;
        //var lpExeName = new Windows.Win32.Foundation.PWSTR();
        //var success = PInvoke.QueryFullProcessImageName(process, PROCESS_NAME_FORMAT.PROCESS_NAME_WIN32, lpExeName, ref capacity);

        //return success.Value != 0
        //    ? string.Empty
        //    : lpExeName.ToString();
    }

    public SafeHandle OpenProcess(bool inheritHandle, uint processId)
    {
        var inherit = new Windows.Win32.Foundation.BOOL(inheritHandle);
        Windows.Win32.Foundation.HANDLE handle = PInvoke.OpenProcess(PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_LIMITED_INFORMATION, inherit, processId);
        return new Microsoft.Win32.SafeHandles.SafeProcessHandle(handle.Value, true);
    }

    public uint GetCurrentThreadId() => PInvoke.GetCurrentThreadId();
}
