global using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OcctSharp;
using OcctSharp.Tests.Shared;
using OcctSharpViewer.Wpf.Controls;

namespace OcctSharp.Viewer.Recovery.Validation;

internal static class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly List<object> Frames = [], Faults = [];
    private static BatchAUDesktopCapture? Desktop;
    private static string Output = "";
    private static int Errors;
    [STAThread]
    private static int Main(string[] args)
    {
        Output = Path.GetFullPath(args[0]); Directory.CreateDirectory(Output);
        _ = OcctRuntime.Info; // Load the product's selected native dependency set before instrumentation.
        nint shim = NativeLibrary.Load(Path.GetFullPath(args[1]));
        Begin = Marshal.GetDelegateForFunctionPointer<BeginFault>(NativeLibrary.GetExport(shim, "cg_context_fault_begin"));
        Observe = Marshal.GetDelegateForFunctionPointer<ObserveFault>(NativeLibrary.GetExport(shim, "cg_context_fault_observe"));
        End = Marshal.GetDelegateForFunctionPointer<EndFault>(NativeLibrary.GetExport(shim, "cg_context_fault_end"));
        using var desktop = new BatchAUDesktopCapture(40, 40); Desktop = desktop;
        var app = new Application(); var left = NewHost(); var right = NewHost(); var grid = new Grid();
        grid.ColumnDefinitions.Add(new()); grid.ColumnDefinitions.Add(new()); grid.Children.Add(left); grid.Children.Add(right); Grid.SetColumn(right, 1);
        var window = new Window { Title = "OcctSharp CG actual SDK context recovery", Content = grid, Width = 900, Height = 520,
            Left = 40, Top = 40, Topmost = true, WindowStartupLocation = WindowStartupLocation.Manual };
        int result = 1;
        window.ContentRendered += async (_, _) => {
            try { await Task.Delay(250); await Run(grid, left, right); result = 0; }
            catch (Exception e) { File.WriteAllText(Path.Combine(Output, "failure.txt"), e.ToString()); Console.Error.WriteLine(e); }
            finally { window.Close(); app.Shutdown(); }
        };
        app.Run(window); NativeLibrary.Free(shim); return result;
    }
    private static OcctViewportHost NewHost()
    {
        var host = new OcctViewportHost(); host.ViewerError += (_, message) => { Errors++; Console.Error.WriteLine(message); }; return host;
    }
    private static OcctViewer Viewer(OcctViewportHost host) => (OcctViewer)typeof(OcctViewportHost).GetField("viewer", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(host)!;
    private static nint Pointer(OcctViewer viewer)
    {
        var handle = (SafeHandle)typeof(OcctViewer).GetProperty("Handle", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(viewer)!;
        return handle.DangerousGetHandle(); // Test-only reflection, live on creating thread.
    }
    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
    private static void Setup(OcctViewer viewer)
    {
        viewer.SetViewCubeVisible(false); viewer.SetProjection(ViewerProjection.Axonometric);
        viewer.FitAll(); viewer.Zoom(.4); viewer.Redraw();
    }
    private static async Task Run(Grid grid, OcctViewportHost left, OcctViewportHost right)
    {
        using var model = ShapeFactory.CreateBox(10, 10, 10);
        var viewer = Viewer(left); var other = Viewer(right);
        var presentation = viewer.Display(model); using var otherPresentation = other.Display(model);
        presentation.SetDisplayMode(ViewerDisplayMode.Shaded); otherPresentation.SetDisplayMode(ViewerDisplayMode.Shaded);
        Setup(viewer); Setup(other); await Task.Delay(150);
        Require(viewer.GetRenderingHealth() == ViewerRenderingHealth.Healthy && other.GetRenderingHealth() == ViewerRenderingHealth.Healthy, "Initial contexts are not healthy.");
        var otherCamera = other.Rendering.GetCamera(); var baseline = Capture(left, right, "baseline"); var otherBaseline = baseline.Other;
        var material = new ViewerAppearanceProfile { Front = new(new(.15, .6, .95)) { Metallic = .6, Roughness = .25 }, Shading = ViewerShading.Phong };
        viewer.Rendering.SetAppearance(presentation, material); viewer.Redraw(); await Task.Delay(150);
        var shaded = Capture(left, right, "material"); Require(Changed(baseline.Left, shaded.Left) > 500, "Material has no observed visible effect.");
        byte[] pixels = new byte[16 * 16 * 4];
        for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++) { int i = (y * 16 + x) * 4; bool tile = ((x / 4 + y / 4) & 1) == 0; pixels[i] = tile ? (byte)240 : (byte)35; pixels[i + 1] = tile ? (byte)40 : (byte)210; pixels[i + 2] = 45; pixels[i + 3] = 255; }
        var retained = new ViewerPixelImage(16, 16, pixels);
        byte[] encodedPixels = (byte[])pixels.Clone();
        for (int i = 0; i < encodedPixels.Length; i += 4) (encodedPixels[i], encodedPixels[i + 2]) = (encodedPixels[i + 2], encodedPixels[i]);
        string path = Path.Combine(Output, "材质源纹理.png"); Save(path, encodedPixels, 16, 16, PixelFormats.Bgra32);
        var texture = viewer.Rendering.CreateTextureFromFile(path); File.Delete(path); Array.Fill(pixels, (byte)0);
        Require(!File.Exists(path), "Encoded texture source was not removed.");
        var appearance = material with { Shading = ViewerShading.Unlit, Front = new(new(1, 1, 1)), Mapping = new() { Planar = true, ScaleS = .25, ScaleT = .25 } };
        viewer.Rendering.SetAppearance(presentation, appearance, texture); viewer.Redraw(); await Task.Delay(150);
        var textured = Capture(left, right, "texture-source-deleted"); Require(Changed(shaded.Left, textured.Left) > 500, "Deleted-file texture is not actually consumed by the visible material.");
        Require(Changed(otherBaseline, textured.Other) == 0, "Material/texture setup affected another viewport.");
        var camera = viewer.Rendering.GetCamera(); var nativeIdentity = Pointer(viewer);
        viewer.InvalidateGpuResources(); Require(viewer.GetRenderingHealth() == ViewerRenderingHealth.PresentationDataLost, "Explicit reupload flag missing.");
        Require(viewer.GetRenderingHealth() == ViewerRenderingHealth.PresentationDataLost, "Query unexpectedly cleared reupload flag.");
        viewer.Redraw(); await Task.Delay(150); var reupload = Capture(left, right, "reupload");
        Require(Changed(textured.Left, reupload.Left) == 0 && viewer.GetRenderingHealth() == ViewerRenderingHealth.Healthy, "Reupload changed resources or failed to clear SDK data-lost state.");
        for (int cycle = 0; cycle < 2; cycle++) foreach (int mode in new[] { 2, 4 })
        {
            string name = "cycle" + cycle + "-fault" + mode;
            var before = Capture(left, right, name + "-before"); nint pointer = Pointer(viewer); nint lease = Begin!(pointer, mode);
            Require(lease != 0, "SDK context fault instrumentation refused a valid live viewer.");
            ViewerRenderingHealth observed; (byte[] Left, byte[] Other) during;
            try
            {
                Require(Observe!(pointer) == mode, "Independent SDK context observation differs from actual fault.");
                observed = viewer.GetRenderingHealth(); Require((int)observed == mode, "Product health did not report the actual absent/uninitialized SDK context.");
                Require(viewer.GetRenderingHealth() == observed && Pointer(viewer) == pointer && viewer.Rendering.GetCamera() == camera, "Read-only health query mutated owner or camera.");
                Require(other.GetRenderingHealth() == ViewerRenderingHealth.Healthy && other.Rendering.GetCamera() == otherCamera, "Fault crossed viewer context boundary.");
                other.Redraw(); _ = DwmFlush(); during = Capture(left, right, name + "-during");
                Require(Changed(before.Left, during.Left) == 0 && Changed(otherBaseline, during.Other) == 0, "Fault observation altered pixels or the other viewport.");
            }
            finally { End!(lease); } // Restore only for safe old-owner cleanup, not the recovery assertion.
            Require(Observe!(pointer) == 0, "Old context could not be restored for safe destruction.");
            texture.Dispose(); presentation.Dispose(); grid.Children.Remove(left); left.Dispose();
            bool staleRejected = false; try { _ = viewer.GetRenderingHealth(); } catch (ObjectDisposedException) { staleRejected = true; }
            Require(staleRejected, "Disposed viewer accepted a health query.");
            left = NewHost(); grid.Children.Add(left); Grid.SetColumn(left, 0); grid.UpdateLayout(); await Task.Delay(200);
            viewer = Viewer(left); presentation = viewer.Display(model); presentation.SetDisplayMode(ViewerDisplayMode.Shaded); Setup(viewer);
            texture = viewer.Rendering.CreateTexture(retained); viewer.Rendering.SetAppearance(presentation, appearance, texture); viewer.Rendering.SetCamera(camera); viewer.Redraw();
            await Task.Delay(150); var recovered = Capture(left, right, name + "-recreated");
            Require(viewer.GetRenderingHealth() == ViewerRenderingHealth.Healthy, "Explicitly recreated context is unhealthy.");
            Require(viewer.Rendering.GetCamera() == camera && Changed(textured.Left, recovered.Left) == 0, "Recreated viewer did not restore retained model/camera/material/texture pixels.");
            Require(Changed(otherBaseline, recovered.Other) == 0 && other.Rendering.GetCamera() == otherCamera, "Explicit recreation changed the other viewport.");
            Faults.Add(new { cycle, mode, independentSdkState = mode, productFlags = (int)observed, oldOwnerDisposed = true, retainedSourceFileAbsent = !File.Exists(path), recoveredPixelDifferences = Changed(textured.Left, recovered.Left), otherPixelDifferences = Changed(otherBaseline, recovered.Other) });
        }
        texture.Dispose(); presentation.Dispose(); Require(Errors == 0, "Hidden production-host UI error occurred.");
        Write("result.json", new { state = "PASS", faults = Faults, frames = Frames, errors = Errors, originalNativeIdentity = (long)nativeIdentity, physicalGpuRemoval = "NOT RUN", faultMechanism = "Actual OpenGl_Window SDK handle slot absent or real uninitialized OpenGl_Context; no flag-field mutation. Parked old context restored only for teardown before explicit production-host recreation." });
        Write("loaded-native.json", Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Where(m => Path.GetFileName(m.FileName).StartsWith("TK", StringComparison.Ordinal) || Path.GetFileName(m.FileName) is "OcctSharp.Native.dll" or "ContextFaultShim.dll" || m.FileName.Contains("\\occt\\", StringComparison.OrdinalIgnoreCase)).Select(m => new { path = m.FileName, sha256 = Hash(File.ReadAllBytes(m.FileName)) }).ToArray());
    }
    private static (byte[] Left, byte[] Other) Capture(OcctViewportHost left, OcctViewportHost right, string name)
    {
        _ = DwmFlush(); var a = left.PointToScreen(new(left.ActualWidth / 2 - 130, left.ActualHeight / 2 - 130)); var b = right.PointToScreen(new(right.ActualWidth / 2 - 130, right.ActualHeight / 2 - 130));
        int offset = (int)b.X - (int)a.X, width = offset + 260; Require(offset >= 260 && a.Y == b.Y, "Invalid paired desktop capture domain.");
        byte[] both = Desktop!.Capture((int)a.X, (int)a.Y, width, 260), first = new byte[260 * 260 * 4], second = new byte[first.Length];
        for (int y = 0; y < 260; y++) { Buffer.BlockCopy(both, y * width * 4, first, y * 260 * 4, 260 * 4); Buffer.BlockCopy(both, (y * width + offset) * 4, second, y * 260 * 4, 260 * 4); }
        Require(Enumerable.Range(0, first.Length / 4).Count(i => Math.Max(first[i * 4], Math.Max(first[i * 4 + 1], first[i * 4 + 2])) > 100) > 1000, "Model area is blank or occluded.");
        Save(Path.Combine(Output, name + ".png"), first, 260, 260, PixelFormats.Bgra32); Save(Path.Combine(Output, name + "-other.png"), second, 260, 260, PixelFormats.Bgra32);
        Frames.Add(new { name, width = 260, height = 260, leftHash = Hash(first), otherHash = Hash(second), leftHwnd = (long)left.Handle, rightHwnd = (long)right.Handle }); return (first, second);
    }
    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
    private static int Changed(byte[] a, byte[] b) => Enumerable.Range(0, a.Length / 4).Count(i => !a.AsSpan(i * 4, 3).SequenceEqual(b.AsSpan(i * 4, 3)));
    private static void Save(string path, byte[] bytes, int width, int height, PixelFormat format)
    {
        var bitmap = BitmapSource.Create(width, height, 96, 96, format, null, bytes, width * 4); var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap)); using var stream = File.Create(path); encoder.Save(stream);
    }
    private static void Write(string path, object value) => File.WriteAllText(Path.Combine(Output, path), JsonSerializer.Serialize(value, JsonOptions));
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate nint BeginFault(nint viewer, int mode);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int ObserveFault(nint viewer);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void EndFault(nint token);
    private static BeginFault? Begin; private static ObserveFault? Observe; private static EndFault? End;
    [DllImport("dwmapi.dll")] private static extern int DwmFlush();
}
