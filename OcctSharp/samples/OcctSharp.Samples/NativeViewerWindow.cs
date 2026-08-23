using System.ComponentModel;
using System.Runtime.InteropServices;

namespace OcctSharp.Samples;

internal sealed class NativeViewerWindow : IDisposable
{
    private const uint ClassOwnDeviceContext = 0x0020;
    private const uint OverlappedWindow = 0x00CF0000;
    private const uint Visible = 0x10000000;
    private const int UseDefault = unchecked((int)0x80000000);
    private const uint MessageClose = 0x0010;
    private const uint MessageDestroy = 0x0002;
    private const uint MessageSize = 0x0005;
    private const uint MessagePaint = 0x000F;
    private const uint MessageEraseBackground = 0x0014;
    private const uint MessageMouseMove = 0x0200;
    private const uint MessageLeftButtonDown = 0x0201;
    private const uint MessageNonClientDestroy = 0x0082;
    private const int ArrowCursor = 32512;

    private static readonly WindowProcedureDelegateType WindowProcedureDelegate = WindowProcedure;
    private static readonly Dictionary<nint, NativeViewerWindow> Windows = [];
    private static readonly string WindowClassName = $"OcctSharp.Viewer.{Environment.ProcessId}";
    private static readonly ushort WindowClassAtom = RegisterWindowClass();

    private OcctViewer? _viewer;
    private Exception? _callbackFailure;

    private NativeViewerWindow(nint handle) => Handle = handle;

    public nint Handle { get; private set; }

    public static NativeViewerWindow Create(string title, int width, int height)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("窗口标题不能为空。", nameof(title));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        _ = WindowClassAtom;

        nint handle = NativeMethods.CreateWindowEx(
            0,
            WindowClassName,
            title,
            OverlappedWindow | Visible,
            UseDefault,
            UseDefault,
            width,
            height,
            0,
            0,
            NativeMethods.GetModuleHandle(null),
            0);
        if (handle == 0) throw new Win32Exception(Marshal.GetLastPInvokeError());

        NativeViewerWindow window = new(handle);
        Windows.Add(handle, window);
        _ = NativeMethods.ShowWindow(handle, 5);
        _ = NativeMethods.UpdateWindow(handle);
        return window;
    }

    public void Attach(OcctViewer viewer)
    {
        ArgumentNullException.ThrowIfNull(viewer);
        ObjectDisposedException.ThrowIf(Handle == 0, this);
        if (_viewer is not null) throw new InvalidOperationException("窗口已经绑定 Viewer。");
        _viewer = viewer;
        viewer.Resize();
    }

    public void RunMessageLoop()
    {
        ObjectDisposedException.ThrowIf(Handle == 0, this);
        while (true)
        {
            int result = NativeMethods.GetMessage(out WindowMessage message, 0, 0, 0);
            if (result == 0) break;
            if (result < 0) throw new Win32Exception(Marshal.GetLastPInvokeError());
            _ = NativeMethods.TranslateMessage(in message);
            _ = NativeMethods.DispatchMessage(in message);
        }

        if (_callbackFailure is not null)
        {
            throw new InvalidOperationException("Viewer 窗口消息处理失败。", _callbackFailure);
        }
    }

    public void Dispose()
    {
        nint handle = Handle;
        if (handle == 0) return;
        _viewer = null;
        _ = NativeMethods.DestroyWindow(handle);
        Windows.Remove(handle);
        Handle = 0;
    }

    private static ushort RegisterWindowClass()
    {
        WindowClass windowClass = new()
        {
            Size = (uint)Marshal.SizeOf<WindowClass>(),
            Style = ClassOwnDeviceContext,
            WindowProcedure = Marshal.GetFunctionPointerForDelegate(WindowProcedureDelegate),
            Instance = NativeMethods.GetModuleHandle(null),
            Cursor = NativeMethods.LoadCursor(0, ArrowCursor),
            ClassName = WindowClassName,
        };
        ushort atom = NativeMethods.RegisterClassEx(in windowClass);
        return atom != 0 ? atom : throw new Win32Exception(Marshal.GetLastPInvokeError());
    }

    private static nint WindowProcedure(nint windowHandle, uint message, nuint wordParameter, nint longParameter)
    {
        if (!Windows.TryGetValue(windowHandle, out NativeViewerWindow? window))
        {
            return NativeMethods.DefWindowProc(windowHandle, message, wordParameter, longParameter);
        }

        try
        {
            switch (message)
            {
                case MessageSize:
                    window._viewer?.Resize();
                    return 0;
                case MessageMouseMove:
                    _ = window._viewer?.MoveTo(GetX(longParameter), GetY(longParameter));
                    return 0;
                case MessageLeftButtonDown:
                    int selectionCount = window._viewer?.SelectAt(GetX(longParameter), GetY(longParameter)).Count ?? 0;
                    _ = NativeMethods.SetWindowText(windowHandle, $"OcctSharp Viewer Sample - selected: {selectionCount}");
                    return 0;
                case MessagePaint:
                    window._viewer?.Redraw();
                    break;
                case MessageEraseBackground when window._viewer is not null:
                    return 1;
                case MessageClose:
                    _ = NativeMethods.DestroyWindow(windowHandle);
                    return 0;
                case MessageDestroy:
                    NativeMethods.PostQuitMessage(0);
                    return 0;
                case MessageNonClientDestroy:
                    Windows.Remove(windowHandle);
                    window.Handle = 0;
                    break;
            }
        }
        catch (Exception error)
        {
            window._callbackFailure ??= error;
            _ = NativeMethods.PostMessage(windowHandle, MessageClose, 0, 0);
        }

        return NativeMethods.DefWindowProc(windowHandle, message, wordParameter, longParameter);
    }

    private static int GetX(nint parameter) => unchecked((short)((long)parameter & 0xFFFF));
    private static int GetY(nint parameter) => unchecked((short)(((long)parameter >> 16) & 0xFFFF));

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate nint WindowProcedureDelegateType(nint windowHandle, uint message, nuint wordParameter, nint longParameter);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WindowClass
    {
        public uint Size;
        public uint Style;
        public nint WindowProcedure;
        public int ClassExtra;
        public int WindowExtra;
        public nint Instance;
        public nint Icon;
        public nint Cursor;
        public nint Background;
        public string? MenuName;
        public string ClassName;
        public nint SmallIcon;
    }

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct WindowMessage
    {
        public readonly nint WindowHandle;
        public readonly uint Message;
        public readonly nuint WordParameter;
        public readonly nint LongParameter;
        public readonly uint Time;
        public readonly int PointX;
        public readonly int PointY;
        public readonly uint Private;
    }

    private static class NativeMethods
    {
        [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern nint GetModuleHandle(string? moduleName);

        [DllImport("user32.dll", EntryPoint = "RegisterClassExW", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern ushort RegisterClassEx(in WindowClass windowClass);

        [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern nint CreateWindowEx(uint extendedStyle, string className, string windowName, uint style,
            int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowWindow(nint windowHandle, int command);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UpdateWindow(nint windowHandle);

        [DllImport("user32.dll", EntryPoint = "DefWindowProcW")]
        internal static extern nint DefWindowProc(nint windowHandle, uint message, nuint wordParameter, nint longParameter);

        [DllImport("user32.dll", EntryPoint = "GetMessageW", SetLastError = true)]
        internal static extern int GetMessage(out WindowMessage message, nint windowHandle, uint minimum, uint maximum);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool TranslateMessage(in WindowMessage message);

        [DllImport("user32.dll", EntryPoint = "DispatchMessageW")]
        internal static extern nint DispatchMessage(in WindowMessage message);

        [DllImport("user32.dll", EntryPoint = "SetWindowTextW", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetWindowText(nint windowHandle, string text);

        [DllImport("user32.dll", EntryPoint = "PostMessageW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool PostMessage(nint windowHandle, uint message, nuint wordParameter, nint longParameter);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DestroyWindow(nint windowHandle);

        [DllImport("user32.dll")]
        internal static extern void PostQuitMessage(int exitCode);

        [DllImport("user32.dll", EntryPoint = "LoadCursorW")]
        internal static extern nint LoadCursor(nint instance, int cursorName);
    }
}
