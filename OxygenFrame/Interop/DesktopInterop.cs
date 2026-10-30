using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace OxygenFrame.Interop;

internal static class DesktopInterop
{
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_APPWINDOW = 0x00040000;
    private const uint WM_SHELLDLL_LOADED = 0x052C;

    [DllImport("user32.dll", CharSet = CharSet.Auto)] private static extern IntPtr FindWindow(string? cls, string? title);
    [DllImport("user32.dll", CharSet = CharSet.Auto)] private static extern IntPtr FindWindowEx(IntPtr parent, IntPtr child, string? cls, string? title);
    [DllImport("user32.dll")] private static extern IntPtr SendMessageTimeout(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam, uint flags, uint timeout, out IntPtr result);
    [DllImport("user32.dll", SetLastError = true)] private static extern IntPtr SetParent(IntPtr child, IntPtr newParent);
    [DllImport("user32.dll", EntryPoint = "GetWindowLong")] private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll", EntryPoint = "SetWindowLong")] private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);
    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")] private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")] private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    private static IntPtr GetExStyle(IntPtr h) => IntPtr.Size == 8 ? GetWindowLongPtr64(h, GWL_EXSTYLE) : new(GetWindowLong32(h, GWL_EXSTYLE));
    private static void SetExStyle(IntPtr h, IntPtr v) { if (IntPtr.Size == 8) SetWindowLongPtr64(h, GWL_EXSTYLE, v); else SetWindowLong32(h, GWL_EXSTYLE, v.ToInt32()); }

    public static void MakeToolWindow(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        var style = GetExStyle(hwnd).ToInt64();
        style = (style | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW;
        SetExStyle(hwnd, new IntPtr(style));
    }

    /// <summary>
    /// Best-effort WorkerW attachment. Windows shell implementation varies by build and multi-monitor setup.
    /// Do not call repeatedly; reparenting can affect DPI and z-order.
    /// </summary>
    public static bool AttachToWallpaper(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        var progman = FindWindow("Progman", null);
        if (progman != IntPtr.Zero)
        {
            SendMessageTimeout(progman, WM_SHELLDLL_LOADED, IntPtr.Zero, IntPtr.Zero, 0, 1000, out _);
        }

        var worker = IntPtr.Zero;
        var top = IntPtr.Zero;
        while ((top = FindWindowEx(IntPtr.Zero, top, "WorkerW", null)) != IntPtr.Zero)
        {
            var view = FindWindowEx(top, IntPtr.Zero, "SHELLDLL_DefView", null);
            if (view != IntPtr.Zero)
            {
                worker = FindWindowEx(IntPtr.Zero, top, "WorkerW", null);
                if (worker != IntPtr.Zero) break;
            }
        }
        if (worker == IntPtr.Zero) worker = progman;
        if (worker == IntPtr.Zero) return false;
        SetParent(hwnd, worker);
        return true;
    }
}
