global using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OcctSharp.Tests.Shared;

// Acceptance observer: executes the original complete app diagnostics, observes
// actual loaded modules, and records composited desktop pixels including native
// airspace. It does not substitute a host scene or inject app assertions.
internal static class Program
{
    [DllImport("user32.dll")] private static extern bool GetClientRect(nint hwnd, out Rect rectangle);
    [DllImport("user32.dll")] private static extern bool ClientToScreen(nint hwnd, ref Point point);
    [StructLayout(LayoutKind.Sequential)] private struct Rect { public int Left, Top, Right, Bottom; }
    [StructLayout(LayoutKind.Sequential)] private struct Point { public int X, Y; }
    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static void Write(string path, object value) => File.WriteAllText(path, JsonSerializer.Serialize(value, JsonOptions));
    [STAThread]
    public static int Main(string[] args)
    {
        if (args.Length == 1 && args[0] == "--test-capture-plan") return CapturePlanFixtures.Run();
        string executable = Path.GetFullPath(args[0]), output = Path.GetFullPath(args[1]);
        Directory.CreateDirectory(output);
        var start = new ProcessStartInfo(executable) { WorkingDirectory = Path.GetDirectoryName(executable)!, UseShellExecute = false };
        foreach (string argument in args.Skip(2)) start.ArgumentList.Add(argument);
        start.Environment["CADORYX_ASSET_STORE"] = "memory";
        start.Environment["CADORYX_APPLICATION_PROFILE"] = Path.Combine(output, "profile");
        var modules = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        var frames = new List<object>(); var errors = new List<object>(); var noNewDesktopFrames = new List<object>();
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Application did not start.");
        bool captureWindow = Environment.GetEnvironmentVariable("OCCTSHARP_APPLICATION_CAPTURE") != "desktop";
        string captureMethod = captureWindow ? "WindowsGraphicsCapture-HWND" : "DXGI-desktop";
        using var desktop = new ApplicationDesktopCapture();
        using var windowCapture = new ApplicationWindowCapture();
        var elapsed = Stopwatch.StartNew(); int sample = 0;
        while (!process.WaitForExit(1000))
        {
            try
            {
                process.Refresh();
                foreach (ProcessModule module in process.Modules)
                {
                    string path = module.FileName;
                    if (!modules.ContainsKey(path) && (path.Contains("\\occt\\", StringComparison.OrdinalIgnoreCase) || Path.GetFileName(path).StartsWith("OcctSharp", StringComparison.Ordinal)))
                        modules[path] = new { path, sha256 = Hash(File.ReadAllBytes(path)) };
                }
            }
            catch (InvalidOperationException) when (process.HasExited) { break; }
            catch (System.ComponentModel.Win32Exception) when (process.HasExited) { break; }
            nint window = process.MainWindowHandle;
            bool fullSceneWritten = args.Skip(2).Any(argument => File.Exists(Path.Combine(argument, "full-scene.png")));
            if (window != 0 && sample < 96 && (fullSceneWritten || (int)elapsed.Elapsed.TotalSeconds < 8 || (int)elapsed.Elapsed.TotalSeconds % 10 == 0))
            {
                try
                {
                    if (!GetClientRect(window, out Rect rectangle)) throw new InvalidOperationException("Missing application client rectangle.");
                    Point origin = default;
                    if (!ClientToScreen(window, ref origin)) throw new InvalidOperationException("Missing application desktop position.");
                    int width = rectangle.Right, height = rectangle.Bottom;
                    if (width < 100 || height < 100) continue;
                    byte[] pixels = captureWindow ? windowCapture.Capture(window, origin.X, origin.Y, width, height) : desktop.Capture(origin.X, origin.Y, width, height);
                    var bitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixels, width * 4);
                    var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    string name = $"{(captureWindow ? "window" : "desktop")}-{sample++:D2}.png";
                    using (var stream = File.Create(Path.Combine(output, name))) encoder.Save(stream);
                    frames.Add(new { name, width, height, origin.X, origin.Y, captureMethod, frameTimestampTicks = captureWindow ? windowCapture.LastFrameTicks : (long?)null, sourceFrameAgeAtAcquireMs = captureWindow ? windowCapture.LastFrameAgeMs : (double?)null, elapsedMs = elapsed.Elapsed.TotalMilliseconds, pixelSha256 = Hash(pixels) });
                }
                catch (NoNewCaptureFrameException exception)
                {
                    noNewDesktopFrames.Add(new { elapsedMs = elapsed.Elapsed.TotalMilliseconds, captureMethod, reason = exception.Message });
                }
                catch (COMException exception) when (exception.HResult == unchecked((int)0x887A0027))
                {
                    // An unchanged desktop produces no new duplication frame.
                    // Record no observation; never substitute a cached image.
                    // No actual frames at all still fails the run below.
                    noNewDesktopFrames.Add(new { elapsedMs = elapsed.Elapsed.TotalMilliseconds, exception.HResult });
                }
                catch (Exception exception) { errors.Add(new { elapsedMs = elapsed.Elapsed.TotalMilliseconds, error = exception.ToString(), requested = desktop.LastRequested, outputs = desktop.LastOutputs }); }
            }
            Write(Path.Combine(output, "observed-native.json"), modules.Values.ToArray());
            Write(Path.Combine(output, "desktop-frames.json"), frames);
        }
        Write(Path.Combine(output, "observed-native.json"), modules.Values.ToArray());
        Write(Path.Combine(output, "desktop-frames.json"), frames);
        Write(Path.Combine(output, "observer-result.json"), new { executable, args = start.ArgumentList.ToArray(), captureMethod, process.ExitCode,
            elapsedMs = elapsed.Elapsed.TotalMilliseconds, modules = modules.Count, desktopFrames = frames.Count, errors, noNewDesktopFrames });
        if (process.ExitCode != 0 || frames.Count == 0 || errors.Count > 0) return 1;
        return 0;
    }
}
