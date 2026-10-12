using System.Runtime.InteropServices;
using OcctSharp.Tests.Shared;

internal readonly record struct DesktopRegion(int X, int Y, int Width, int Height)
{
    internal long Area => (long)Width * Height;
    internal DesktopRegion Intersect(DesktopRegion other)
    {
        int left = Math.Max(X, other.X), top = Math.Max(Y, other.Y);
        int right = Math.Min(checked(X + Width), checked(other.X + other.Width));
        int bottom = Math.Min(checked(Y + Height), checked(other.Y + other.Height));
        return new(left, top, Math.Max(0, right - left), Math.Max(0, bottom - top));
    }
}

// Observer-only readback. Each part consists of actual fresh pixels from the
// corresponding DXGI output; missing/offscreen pixels are never invented.
internal sealed class ApplicationDesktopCapture : IDisposable
{
    private readonly Dictionary<DesktopRegion, BatchAUDesktopCapture> captures = [];
    internal DesktopRegion? LastRequested { get; private set; }
    internal DesktopRegion[] LastOutputs { get; private set; } = [];
    internal static DesktopRegion[] Plan(DesktopRegion requested, IEnumerable<DesktopRegion> outputs)
    {
        if (requested.Width <= 0 || requested.Height <= 0) throw new ArgumentOutOfRangeException(nameof(requested));
        var regions = outputs.Distinct().Select(requested.Intersect).Where(x => x.Area > 0).ToArray();
        for (int i = 0; i < regions.Length; i++)
            for (int j = i + 1; j < regions.Length; j++)
                if (regions[i].Intersect(regions[j]).Area > 0) throw new InvalidOperationException("Overlapping desktop output coverage.");
        if (regions.Sum(x => x.Area) != requested.Area)
            throw new InvalidOperationException($"Full application client is not on active desktop outputs: {requested}; outputs={string.Join(';', outputs)}");
        return regions;
    }
    internal byte[] Capture(int x, int y, int width, int height)
    {
        var requested = new DesktopRegion(x, y, width, height);
        LastRequested = requested;
        var outputs = ActiveOutputs();
        LastOutputs = outputs;
        var parts = Plan(requested, outputs);
        var pixels = new byte[checked(width * height * 4)];
        foreach (var part in parts)
        {
            var output = outputs.First(o => o.Intersect(part) == part);
            if (!captures.TryGetValue(output, out var capture))
                captures[output] = capture = new BatchAUDesktopCapture(output.X + output.Width / 2, output.Y + output.Height / 2);
            var bytes = capture.Capture(part.X, part.Y, part.Width, part.Height);
            CopyPart(pixels, requested, bytes, part);
        }
        // A topology change starts fresh sessions on the next capture.
        foreach (var stale in captures.Keys.Except(outputs).ToArray()) { captures[stale].Dispose(); captures.Remove(stale); }
        return pixels;
    }
    internal static void CopyPart(byte[] destination, DesktopRegion requested, byte[] source, DesktopRegion part)
    {
        if (requested.Intersect(part) != part || source.Length != checked(part.Width * part.Height * 4)
            || destination.Length != checked(requested.Width * requested.Height * 4)) throw new ArgumentException("Capture part size/bounds mismatch.");
        for (int row = 0; row < part.Height; row++)
            Buffer.BlockCopy(source, row * part.Width * 4, destination,
                ((part.Y - requested.Y + row) * requested.Width + part.X - requested.X) * 4, part.Width * 4);
    }
    private static DesktopRegion[] ActiveOutputs()
    {
        var outputs = new List<DesktopRegion>();
        MonitorCallback callback = (nint monitor, nint dc, ref NativeRect bounds, nint data) =>
        { outputs.Add(new(bounds.Left, bounds.Top, bounds.Right - bounds.Left, bounds.Bottom - bounds.Top)); return true; };
        if (!EnumDisplayMonitors(0, 0, callback, 0)) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        return outputs.Distinct().ToArray();
    }
    public void Dispose() { foreach (var capture in captures.Values) capture.Dispose(); captures.Clear(); }
    [StructLayout(LayoutKind.Sequential)] private struct NativeRect { public int Left, Top, Right, Bottom; }
    private delegate bool MonitorCallback(nint monitor, nint dc, ref NativeRect bounds, nint data);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumDisplayMonitors(nint dc, nint clip, MonitorCallback callback, nint data);
}
