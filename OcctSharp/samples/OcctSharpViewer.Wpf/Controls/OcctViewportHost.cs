using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using OcctSharp;
using OcctSharpViewer.Wpf.Services;

namespace OcctSharpViewer.Wpf.Controls;

/// <summary>
/// Owns the native child HWND required by OCCT and forwards Win32 pointer input into
/// the parent-bound <see cref="ViewerInputController"/>. Application behavior remains
/// exposed to the view model through <see cref="IViewerService"/>.
/// </summary>
public sealed class OcctViewportHost : HwndHost, IViewerService
{
    private const uint ClassOwnDeviceContext = 0x0020;
    private const uint StyleChild = 0x40000000;
    private const uint StyleVisible = 0x10000000;
    private const uint StyleClipSiblings = 0x04000000;
    private const uint StyleClipChildren = 0x02000000;
    private const uint MessageSize = 0x0005;
    private const uint MessagePaint = 0x000F;
    private const uint MessageEraseBackground = 0x0014;
    private const uint MessageMouseMove = 0x0200;
    private const uint MessageLeftButtonDown = 0x0201;
    private const uint MessageLeftButtonUp = 0x0202;
    private const uint MessageRightButtonDown = 0x0204;
    private const uint MessageRightButtonUp = 0x0205;
    private const uint MessageMiddleButtonDown = 0x0207;
    private const uint MessageMiddleButtonUp = 0x0208;
    private const uint MessageMouseWheel = 0x020A;
    private const uint MessageCaptureChanged = 0x0215;
    private const uint MouseKeyLeft = 0x0001;
    private const uint MouseKeyRight = 0x0002;
    private const uint MouseKeyMiddle = 0x0010;
    private const int VirtualKeyShift = 0x10;
    private const int VirtualKeyControl = 0x11;
    private const int VirtualKeyMenu = 0x12;
    private const int ArrowCursor = 32512;

    private static readonly WindowProcedureDelegate WindowProcedureRoot = WindowProcedure;
    private static readonly Dictionary<nint, OcctViewportHost> Hosts = [];
    private static readonly string WindowClassName = $"OcctSharpViewer.Wpf.Viewport.{Environment.ProcessId}";
    private static readonly ushort WindowClassAtom = RegisterWindowClass();

    private nint windowHandle;
    private OcctViewer? viewer;
    private readonly List<ViewerPresentation> presentations = [];
    private ViewerDisplayMode displayMode = ViewerDisplayMode.Shaded;
    private ViewerPointerButton? capturedButton;

    public event EventHandler? ViewerReady;
    public event Action<object?, int>? SelectionChanged;
    public event Action<object?, string>? ViewerError;

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        _ = WindowClassAtom;
        windowHandle = NativeMethods.CreateWindowEx(
            0,
            WindowClassName,
            string.Empty,
            StyleChild | StyleVisible | StyleClipSiblings | StyleClipChildren,
            0,
            0,
            1,
            1,
            hwndParent.Handle,
            0,
            NativeMethods.GetModuleHandle(null),
            0);
        if (windowHandle == 0) throw new Win32Exception(Marshal.GetLastPInvokeError());

        Hosts.Add(windowHandle, this);
        try
        {
            viewer = OcctViewer.Create(windowHandle);
            viewer.SetBackgroundColor(new ViewerColor(0.055, 0.075, 0.095));
            viewer.SetProjection(ViewerProjection.Axonometric);
            viewer.ShowTrihedron(ViewerTrihedronPosition.LeftLower, new ViewerColor(0.9, 0.9, 0.9), 0.08);
            viewer.Resize();
        }
        catch
        {
            Hosts.Remove(windowHandle);
            _ = NativeMethods.DestroyWindow(windowHandle);
            windowHandle = 0;
            throw;
        }

        _ = Dispatcher.BeginInvoke(() => ViewerReady?.Invoke(this, EventArgs.Empty));
        return new HandleRef(this, windowHandle);
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        DisposePresentations(presentations);
        viewer?.Dispose();
        viewer = null;
        if (hwnd.Handle != 0)
        {
            Hosts.Remove(hwnd.Handle);
            _ = NativeMethods.DestroyWindow(hwnd.Handle);
        }
        windowHandle = 0;
    }

    public void LoadModel(string filePath)
    {
        OcctViewer activeViewer = GetViewer();
        string extension = Path.GetExtension(filePath).ToUpperInvariant();
        List<ViewerPresentation> nextPresentations = extension switch
        {
            ".STEP" or ".STP" => DisplayStep(activeViewer, filePath),
            ".IGES" or ".IGS" => DisplayGeometry(activeViewer, ShapeExchange.ReadIges(filePath)),
            _ => throw new NotSupportedException("Only STEP (.step/.stp) and IGES (.iges/.igs) files are supported."),
        };

        try
        {
            foreach (ViewerPresentation nextPresentation in nextPresentations)
                nextPresentation.SetDisplayMode(displayMode);

            DisposePresentations(presentations);
            presentations.AddRange(nextPresentations);
            nextPresentations.Clear();
            activeViewer.SetProjection(ViewerProjection.Axonometric);
            activeViewer.FitAll();
            activeViewer.Redraw();
        }
        finally
        {
            DisposePresentations(nextPresentations);
        }
    }

    public void FitAll()
    {
        GetViewer().FitAll();
        GetViewer().Redraw();
    }

    public void SetProjection(ViewerProjection projection)
    {
        GetViewer().SetProjection(projection);
        GetViewer().FitAll();
        GetViewer().Redraw();
    }

    public void SetDisplayMode(ViewerDisplayMode displayMode)
    {
        this.displayMode = displayMode;
        foreach (ViewerPresentation presentation in presentations)
            presentation.SetDisplayMode(displayMode);
        GetViewer().Redraw();
    }

    public void ClearSelection()
    {
        GetViewer().ClearSelection();
        GetViewer().Redraw();
        SelectionChanged?.Invoke(this, 0);
    }

    private OcctViewer GetViewer() => viewer ?? throw new InvalidOperationException("The OCCT viewport is not initialized.");

    private static List<ViewerPresentation> DisplayStep(OcctViewer viewer, string filePath)
    {
        using XdeDocument document = XdeDocument.ReadStep(filePath, new XdeStepReadOptions(ReadColors: true));
        List<ViewerPresentation> result = [];
        try
        {
            foreach (XdeLabel root in document.GetFreeShapes())
            {
                if (!root.IsAssembly)
                {
                    using Shape shape = root.Shape;
                    result.Add(DisplayStyled(viewer, shape, GetColor(root)));
                    continue;
                }

                IReadOnlyList<XdeOccurrence> occurrences = root.GetOccurrences(recursive: true);
                int countBeforeRoot = result.Count;
                try
                {
                    foreach (XdeOccurrence occurrence in occurrences)
                    {
                        // An assembly occurrence already contains all descendants, so displaying
                        // it together with its leaf occurrences would draw the same parts twice.
                        if (occurrence.IsAssembly) continue;
                        ViewerPresentation presentation = viewer.Display(occurrence);
                        ApplyColor(presentation, GetColor(occurrence));
                        result.Add(presentation);
                    }
                }
                finally
                {
                    foreach (XdeOccurrence occurrence in occurrences) occurrence.Dispose();
                }

                // Preserve unusual empty/root-only XDE shapes instead of showing a blank viewport.
                if (result.Count == countBeforeRoot)
                {
                    using Shape shape = root.Shape;
                    result.Add(DisplayStyled(viewer, shape, GetColor(root)));
                }
            }

            if (result.Count == 0)
            {
                using Shape fallback = ShapeExchange.ReadStep(filePath);
                result.Add(DisplayStyled(viewer, fallback, null));
            }

            return result;
        }
        catch
        {
            DisposePresentations(result);
            throw;
        }
    }

    private static List<ViewerPresentation> DisplayGeometry(OcctViewer viewer, Shape shape)
    {
        using (shape)
            return [DisplayStyled(viewer, shape, null)];
    }

    private static ViewerPresentation DisplayStyled(OcctViewer viewer, Shape shape, XdeColor? color)
    {
        ViewerPresentation presentation = viewer.Display(shape);
        try
        {
            ApplyColor(presentation, color);
            return presentation;
        }
        catch
        {
            presentation.Dispose();
            throw;
        }
    }

    private static void ApplyColor(ViewerPresentation presentation, XdeColor? color)
    {
        XdeColor effective = color ?? new XdeColor(0.72, 0.78, 0.88);
        presentation.SetColor(new ViewerColor(effective.Red, effective.Green, effective.Blue));
    }

    private static XdeColor? GetColor(XdeLabel label) =>
        label.Color ?? label.VisualMaterial?.BaseColor;

    private static XdeColor? GetColor(XdeOccurrence occurrence) =>
        GetColor(occurrence.OccurrenceLabel) ?? GetColor(occurrence.ReferredLabel);

    private static void DisposePresentations(List<ViewerPresentation> values)
    {
        foreach (ViewerPresentation value in values) value.Dispose();
        values.Clear();
    }

    private nint ProcessWindowMessage(uint message, nuint wordParameter, nint longParameter)
    {
        OcctViewer? activeViewer = viewer;
        if (activeViewer is null) return NativeMethods.DefWindowProc(windowHandle, message, wordParameter, longParameter);

        switch (message)
        {
            case MessageSize:
                activeViewer.Resize();
                return 0;
            case MessagePaint:
                activeViewer.Redraw();
                break;
            case MessageEraseBackground:
                return 1;
            case MessageMouseMove:
                activeViewer.Input.PointerMoved(
                    GetX(longParameter),
                    GetY(longParameter),
                    GetButtons(wordParameter),
                    GetModifiers());
                return 0;
            case MessageLeftButtonDown:
                BeginPointer(ViewerPointerButton.Left, longParameter);
                return 0;
            case MessageRightButtonDown:
                BeginPointer(ViewerPointerButton.Right, longParameter);
                return 0;
            case MessageMiddleButtonDown:
                BeginPointer(ViewerPointerButton.Middle, longParameter);
                return 0;
            case MessageLeftButtonUp:
                EndPointer(ViewerPointerButton.Left, longParameter);
                return 0;
            case MessageRightButtonUp:
                EndPointer(ViewerPointerButton.Right, longParameter);
                return 0;
            case MessageMiddleButtonUp:
                EndPointer(ViewerPointerButton.Middle, longParameter);
                return 0;
            case MessageMouseWheel:
                Point point = new(GetX(longParameter), GetY(longParameter));
                _ = NativeMethods.ScreenToClient(windowHandle, ref point);
                activeViewer.Input.MouseWheel(unchecked((short)((wordParameter >> 16) & 0xFFFF)), point.X, point.Y, GetModifiers());
                return 0;
            case MessageCaptureChanged:
                capturedButton = null;
                break;
        }

        return NativeMethods.DefWindowProc(windowHandle, message, wordParameter, longParameter);
    }

    private void BeginPointer(ViewerPointerButton button, nint coordinates)
    {
        _ = NativeMethods.SetFocus(windowHandle);
        _ = NativeMethods.SetCapture(windowHandle);
        capturedButton = button;
        GetViewer().Input.PointerPressed(button, GetX(coordinates), GetY(coordinates), GetModifiers());
    }

    private void EndPointer(ViewerPointerButton button, nint coordinates)
    {
        IReadOnlyList<ViewerPresentation> selection = GetViewer().Input.PointerReleased(
            button, GetX(coordinates), GetY(coordinates), GetModifiers());
        if (capturedButton == button)
        {
            capturedButton = null;
            _ = NativeMethods.ReleaseCapture();
        }
        SelectionChanged?.Invoke(this, selection.Count);
    }

    private static nint WindowProcedure(nint handle, uint message, nuint wordParameter, nint longParameter)
    {
        if (!Hosts.TryGetValue(handle, out OcctViewportHost? host))
            return NativeMethods.DefWindowProc(handle, message, wordParameter, longParameter);

        try
        {
            return host.ProcessWindowMessage(message, wordParameter, longParameter);
        }
        catch (Exception error)
        {
            _ = host.Dispatcher.BeginInvoke(() => host.ViewerError?.Invoke(host, error.Message));
            return NativeMethods.DefWindowProc(handle, message, wordParameter, longParameter);
        }
    }

    private static ushort RegisterWindowClass()
    {
        WindowClass windowClass = new()
        {
            Size = (uint)Marshal.SizeOf<WindowClass>(),
            Style = ClassOwnDeviceContext,
            WindowProcedure = Marshal.GetFunctionPointerForDelegate(WindowProcedureRoot),
            Instance = NativeMethods.GetModuleHandle(null),
            Cursor = NativeMethods.LoadCursor(0, ArrowCursor),
            ClassName = WindowClassName,
        };
        ushort atom = NativeMethods.RegisterClassEx(in windowClass);
        return atom != 0 ? atom : throw new Win32Exception(Marshal.GetLastPInvokeError());
    }

    private static int GetX(nint parameter) => unchecked((short)((long)parameter & 0xFFFF));
    private static int GetY(nint parameter) => unchecked((short)(((long)parameter >> 16) & 0xFFFF));

    private static ViewerPointerButtons GetButtons(nuint wordParameter)
    {
        uint keys = unchecked((uint)wordParameter);
        ViewerPointerButtons buttons = ViewerPointerButtons.None;
        if ((keys & MouseKeyLeft) != 0) buttons |= ViewerPointerButtons.Left;
        if ((keys & MouseKeyMiddle) != 0) buttons |= ViewerPointerButtons.Middle;
        if ((keys & MouseKeyRight) != 0) buttons |= ViewerPointerButtons.Right;
        return buttons;
    }

    private static ViewerModifierKeys GetModifiers()
    {
        ViewerModifierKeys modifiers = ViewerModifierKeys.None;
        if (NativeMethods.GetKeyState(VirtualKeyShift) < 0) modifiers |= ViewerModifierKeys.Shift;
        if (NativeMethods.GetKeyState(VirtualKeyControl) < 0) modifiers |= ViewerModifierKeys.Control;
        if (NativeMethods.GetKeyState(VirtualKeyMenu) < 0) modifiers |= ViewerModifierKeys.Alt;
        return modifiers;
    }

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate nint WindowProcedureDelegate(nint windowHandle, uint message, nuint wordParameter, nint longParameter);

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
    private struct Point
    {
        public Point(int x, int y) { X = x; Y = y; }
        public int X;
        public int Y;
    }

    private static class NativeMethods
    {
        [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern nint GetModuleHandle(string? moduleName);

        [DllImport("user32.dll", EntryPoint = "RegisterClassExW", SetLastError = true)]
        internal static extern ushort RegisterClassEx(in WindowClass windowClass);

        [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern nint CreateWindowEx(uint extendedStyle, string className, string windowName, uint style,
            int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);

        [DllImport("user32.dll", EntryPoint = "DefWindowProcW")]
        internal static extern nint DefWindowProc(nint windowHandle, uint message, nuint wordParameter, nint longParameter);

        [DllImport("user32.dll", EntryPoint = "DestroyWindow")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DestroyWindow(nint windowHandle);

        [DllImport("user32.dll", EntryPoint = "LoadCursorW")]
        internal static extern nint LoadCursor(nint instance, int cursorName);

        [DllImport("user32.dll")]
        internal static extern nint SetFocus(nint windowHandle);

        [DllImport("user32.dll")]
        internal static extern nint SetCapture(nint windowHandle);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ScreenToClient(nint windowHandle, ref Point point);

        [DllImport("user32.dll")]
        internal static extern short GetKeyState(int virtualKey);
    }
}
