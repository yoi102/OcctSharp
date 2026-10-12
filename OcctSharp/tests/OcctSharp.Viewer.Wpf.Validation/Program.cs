global using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OcctSharp;
using OcctSharp.Tests.Shared;
using OcctSharpViewer.Wpf.Controls;

namespace OcctSharp.Viewer.Wpf.Validation;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        string output = Path.GetFullPath(args[0]); Directory.CreateDirectory(output);
        using var capture = new BatchAUDesktopCapture(40, 40);
        Desktop = capture;
        var app = new Application();
        var left = new OcctViewportHost(); var right = new OcctViewportHost();
        var grid = new Grid(); grid.ColumnDefinitions.Add(new()); grid.ColumnDefinitions.Add(new());
        grid.Children.Add(left); grid.Children.Add(right); Grid.SetColumn(right, 1);
        var window = new Window { Title = "OcctSharp CF actual WPF input validation", Content = grid,
            Width = 900, Height = 520, Left = 40, Top = 40, WindowStartupLocation = WindowStartupLocation.Manual, Topmost = true };
        int result = 1;
        window.ContentRendered += async (_, _) =>
        {
            try
            {
                await Task.Delay(200);
                await Run(left, right, new WindowInteropHelper(window).Handle, output, args.Contains("--full"));
                result = 0;
            }
            catch (Exception error) { File.WriteAllText(Path.Combine(output, "failure.txt"), error.ToString()); Console.Error.WriteLine(error); }
            finally { window.Close(); app.Shutdown(); }
        };
        app.Run(window); return result;
    }

    private static OcctViewer Viewer(OcctViewportHost host) => (OcctViewer)typeof(OcctViewportHost)
        .GetField("viewer", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(host)!;
    private static readonly List<object> Events = [];
    private static void Mouse(OcctViewportHost host, uint message, int x, int y, nuint buttons = 0)
    {
        Events.Add(new { hwnd = (long)host.Handle, message, x, y, buttons = (long)buttons });
        SendMessage(host.Handle, message, buttons, (nint)((y << 16) | (x & 65535)));
    }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static async Task Run(OcctViewportHost left, OcctViewportHost right, nint parent, string output, bool full)
    {
        left.DisplaySnapshotSmokeShape(); right.DisplaySnapshotSmokeShape();
        var v = Viewer(left); var other = Viewer(right); v.SetViewCubeVisible(false); other.SetViewCubeVisible(false);
        v.Zoom(.55); other.Zoom(.55); // Keep the complete model and its edges inside the desktop evidence region.
        v.Redraw(); other.Redraw();
        Write(output, "initial-state.json", new { left.ActualWidth, left.ActualHeight, camera = v.Rendering.GetCamera(), screen = left.PointToScreen(new Point(0, 0)) });
        Require(IsWindowVisible(left.Handle) && IsWindowVisible(right.Handle), "Both real WPF child HWNDs must be visible.");
        int errors = 0; left.ViewerError += (_, _) => errors++;
        var p = v.WorldToScreen(new(10, 7.5, 5));
        var otherCamera = other.Rendering.GetCamera();
        Mouse(left, 0x201, p.X, p.Y, 1);
        Require(GetCapture() == left.Handle, "Production left-down must establish Win32 capture.");
        SetCapture(parent); // Win32 itself dispatches WM_CAPTURECHANGED to the production child.
        Require(GetCapture() == parent, "Capture must actually move to another HWND.");
        Mouse(left, 0x202, p.X, p.Y);
        ReleaseCapture();
        bool staleClick = v.GetSelection().Count != 0;
        Write(output, "capture-loss-reproduction.json", new { state = staleClick ? "FAIL" : "PASS", staleClick,
            originalCapture = (long)left.Handle, transferredCapture = (long)parent, events = Events.ToArray(), errors });
        Require(!staleClick, "Actual WPF capture loss incorrectly turned a later release into a selection click.");
        Require(errors == 0 && other.GetSelection().Count == 0 && other.Rendering.GetCamera() == otherCamera, "Capture loss affected another viewport or raised a hidden UI error.");
        if (!full) return;
        v.Input.RectangleSelectionEnabled = true;
        Mouse(left, 0x201, 5, 5, 1); Mouse(left, 0x200, 420, 450, 1);
        SetCapture(parent); Mouse(left, 0x202, 420, 450); ReleaseCapture();
        Require(v.GetSelection().Count == 0, "Capture loss must suppress a completed rectangle as well as a click.");
        Mouse(left, 0x201, 5, 5, 1); Mouse(left, 0x200, 420, 450, 1); Mouse(left, 0x202, 420, 450);
        Require(v.GetSelection().Count == 1, "A fresh rectangle gesture after capture loss must work.");
        Mouse(left, 0x100, 0, 0, 0x1B);
        Require(v.GetSelection().Count == 0, "Real child HWND Escape must clear selection.");
        Mouse(left, 0x201, p.X, p.Y, 1); Mouse(left, 0x100, 0, 0, 0x1B); Mouse(left, 0x202, p.X, p.Y);
        Require(v.GetSelection().Count == 0 && GetCapture() != left.Handle, "Escape must cancel the active gesture and Win32 capture.");
        Mouse(left, 0x201, p.X, p.Y, 1); Mouse(left, 0x1F, 0, 0); Mouse(left, 0x202, p.X, p.Y);
        Require(v.GetSelection().Count == 0 && GetCapture() != left.Handle, "WM_CANCELMODE must cancel the active gesture.");
        Mouse(left, 0x201, p.X, p.Y, 1); Mouse(left, 0x202, p.X, p.Y);
        var selected = v.GetSelection(); Require(selected.Count == 1, "Actual WPF click must select the box.");
        var owned = (List<ViewerPresentation>)typeof(OcctViewportHost).GetField("presentations", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(left)!;
        Require(ReferenceEquals(selected[0], owned.Single()), "Picking must preserve the exact managed presentation identity.");
        v.ClearSelection();
        using var volume = v.SelectionQueries.CreatePointVolume(p.X, p.Y);
        Require(volume.IsCurrent, "A new selection workspace must match its camera.");
        var normal = await Capture(left, output, "normal", right, "other-before");
        v.SaveScreenshot(Path.Combine(output, "native-normal.png"));
        Point screen = left.PointToScreen(new Point(p.X, p.Y));
        Mouse(left, 0x20a, (int)screen.X, (int)screen.Y, (nuint)(120 << 16));
        Require(!volume.IsCurrent, "Actual wheel must invalidate an existing camera-bound selection workspace.");
        bool cameraRejected = false; try { volume.ProbePoint(new(10, 7.5, 5)); } catch (InvalidOperationException) { cameraRejected = true; }
        Require(cameraRejected, "A stale selection workspace must reject probing.");
        Write(output, "wheel-state.json", new { camera = v.Rendering.GetCamera(), screen, errors });
        var zoom = await Capture(left, output, "zoom"); Require(Changed(normal, zoom) > 50, "Actual wheel zoom must change model-region desktop pixels.");
        var camera = v.GetCamera(); Mouse(left, 0x207, p.X, p.Y, 16); Mouse(left, 0x200, p.X + 30, p.Y + 20, 16); Mouse(left, 0x208, p.X + 30, p.Y + 20);
        Require(v.GetCamera() != camera, "Actual middle drag must pan the camera.");
        var pan = await Capture(left, output, "pan"); Require(Changed(zoom, pan) > 50, "Pan must change actual desktop model pixels.");
        camera = v.GetCamera(); Mouse(left, 0x204, p.X, p.Y, 2); Mouse(left, 0x200, p.X + 55, p.Y + 30, 2); Mouse(left, 0x205, p.X + 55, p.Y + 30);
        Require(v.GetCamera() != camera, "Actual right drag must rotate the camera.");
        var rotate = await Capture(left, output, "rotate"); Require(Changed(pan, rotate) > 50, "Rotation must change desktop model pixels.");
        v.FitAll(); v.SetProjection(ViewerProjection.Axonometric); v.Redraw();
        var beforeClip = await Capture(left, output, "before-clip");
        var clip = v.CreateClipPlane(new(1, 0, 0, -10)); v.Redraw();
        var clipped = await Capture(left, output, "clipped"); Require(Changed(beforeClip, clipped) > 50, "Clipping must change actual desktop model pixels.");
        clip.Dispose(); bool removedRejected = false; try { clip.Update(new(1, 0, 0, -5)); } catch (ObjectDisposedException) { removedRejected = true; }
        Require(removedRejected, "Removed clip plane must reject mutation.");
        v.Redraw(); var restored = await Capture(left, output, "restored", right, "other-after"); Require(Changed(clipped, restored) > 50, "Clip removal must restore desktop model pixels.");
        int otherPixelsChanged = Changed(OtherFrames["other-before"], OtherFrames["other-after"]);
        Require(other.Rendering.GetCamera() == otherCamera && other.GetSelection().Count == 0 && errors == 0 && otherPixelsChanged == 0, "Input/clip policy must stay isolated to its viewport, including scale and desktop pixels.");
        Write(output, "input-workflow.json", new { state = "PASS", actualWpfChildHwnd = (long)left.Handle, identity = true, captureLoss = true,
            rectangleCancel = true, freshRectangle = true, nativeEscape = true, nativeCancelMode = true, cameraRejected, otherPixelsChanged,
            zoomPixels = Changed(normal, zoom), panPixels = Changed(zoom, pan), rotationPixels = Changed(pan, rotate), clipPixels = Changed(beforeClip, clipped), clipRestorePixels = Changed(clipped, restored), removedRejected, isolation = true, events = Events });
        Write(output, "loaded-native.json", Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Where(m => m.FileName.Contains("\\occt\\", StringComparison.OrdinalIgnoreCase))
            .Select(m => new { path = m.FileName, sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(m.FileName))) }).ToArray());
    }
    private static readonly Dictionary<string, byte[]> OtherFrames = [];
    private static async Task<byte[]> Capture(OcctViewportHost host, string output, string name, OcctViewportHost? other = null, string? otherName = null)
    {
        await Task.Delay(150); _ = DwmFlush(); var point = host.PointToScreen(new Point(host.ActualWidth / 2 - 130, host.ActualHeight / 2 - 130));
        byte[] bytes;
        if (other is not null)
        {
            var otherPoint = other.PointToScreen(new Point(other.ActualWidth / 2 - 130, other.ActualHeight / 2 - 130));
            int offset = (int)otherPoint.X - (int)point.X; int width = offset + 260;
            Require(otherPoint.Y == point.Y && offset >= 260, "Isolation regions must share one desktop-frame row domain.");
            var both = Desktop!.Capture((int)point.X, (int)point.Y, width, 260);
            bytes = new byte[260 * 260 * 4]; var otherBytes = new byte[bytes.Length];
            for (int y = 0; y < 260; y++)
            {
                Buffer.BlockCopy(both, y * width * 4, bytes, y * 260 * 4, 260 * 4);
                Buffer.BlockCopy(both, (y * width + offset) * 4, otherBytes, y * 260 * 4, 260 * 4);
            }
            SavePixels(output, otherName!, otherBytes, 260, 260); OtherFrames.Add(otherName!, otherBytes);
        }
        else bytes = Desktop!.Capture((int)point.X, (int)point.Y, 260, 260);
        Require(Enumerable.Range(0, bytes.Length / 4).Count(i => bytes[i * 4] + bytes[i * 4 + 1] + bytes[i * 4 + 2] > 30) > 500, "Desktop model region is blank; frame cannot establish visual acceptance.");
        SavePixels(output, name, bytes, 260, 260); return bytes;
    }
    private static void SavePixels(string output, string name, byte[] bytes, int width, int height)
    {
        var bitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, bytes, width * 4);
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(Path.Combine(output, name + ".png")); encoder.Save(stream);
    }
    private static int Changed(byte[] a, byte[] b) => Enumerable.Range(0, a.Length / 4).Count(i => !a.AsSpan(i * 4, 3).SequenceEqual(b.AsSpan(i * 4, 3)));
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static BatchAUDesktopCapture? Desktop;
    private static void Write(string output, string name, object value) => File.WriteAllText(Path.Combine(output, name), JsonSerializer.Serialize(value, JsonOptions));
    [DllImport("user32.dll", EntryPoint = "SendMessageW")] private static extern nint SendMessage(nint hwnd, uint message, nuint wp, nint lp);
    [DllImport("user32.dll")] private static extern nint SetCapture(nint hwnd);
    [DllImport("user32.dll")] private static extern nint GetCapture();
    [DllImport("user32.dll")] private static extern bool ReleaseCapture();
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(nint hwnd);
    [DllImport("dwmapi.dll")] private static extern int DwmFlush();
}
