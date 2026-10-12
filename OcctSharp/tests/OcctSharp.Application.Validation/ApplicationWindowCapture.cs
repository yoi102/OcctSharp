using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;
using WinRT;

internal sealed class NoNewCaptureFrameException(string message) : Exception(message);

// Windows Graphics Capture reads the target HWND, including child native
// rendering surfaces. It does not focus/move/redraw the application or use
// its SaveScreenshot result. All accepted samples are newly acquired frames.
internal sealed class ApplicationWindowCapture : IDisposable
{
    private nint device, context, window;
    private IDirect3DDevice? projectedDevice;
    private GraphicsCaptureItem? item;
    private Direct3D11CaptureFramePool? pool;
    private GraphicsCaptureSession? session;
    private Windows.Graphics.SizeInt32 size;
    internal long LastFrameTicks { get; private set; }
    internal double LastFrameAgeMs { get; private set; }
    internal static bool IsFresh(long sourceTicks, long nowTicks) => sourceTicks > 0 && nowTicks >= sourceTicks && nowTicks - sourceTicks <= TimeSpan.TicksPerMillisecond * 250;
    private static long MonotonicTicks() => (long)(Stopwatch.GetTimestamp() * (decimal)TimeSpan.TicksPerSecond / Stopwatch.Frequency);
    private static readonly Guid TextureId = new("6f15aaf2-d208-4e89-9ab4-489535d34f9c");
    private static readonly Guid AccessId = new("a9b3d012-3df2-4ee3-b8d1-8695f457d3c1");
    internal byte[] Capture(nint target, int x, int y, int width, int height)
    {
        if (target != window) Initialize(target);
        var timer = Stopwatch.StartNew();
        Direct3D11CaptureFrame? frame;
        while (true)
        {
            frame = pool!.TryGetNextFrame();
            if (frame is not null)
            {
                if (IsFresh(frame.SystemRelativeTime.Ticks, MonotonicTicks())) break;
                frame.Dispose();
            }
            if (timer.ElapsedMilliseconds >= 1000) throw NoFrame("No fresh target-window capture frame within 250 ms source age; buffered frames discarded.");
            Thread.Sleep(20);
        }
        for (int pending = 0; pending < 8; pending++)
        {
            var newer = pool!.TryGetNextFrame();
            if (newer is null) break;
            frame.Dispose(); frame = newer;
        }
        using (frame)
        {
            LastFrameTicks = frame.SystemRelativeTime.Ticks;
            long nowTicks = MonotonicTicks();
            if (!IsFresh(LastFrameTicks, nowTicks)) throw NoFrame("Newest target-window frame is stale; no current-paint credit.");
            LastFrameAgeMs = (nowTicks - LastFrameTicks) / (double)TimeSpan.TicksPerMillisecond;
            if (frame.ContentSize.Width != size.Width || frame.ContentSize.Height != size.Height)
            {
                size = frame.ContentSize;
                if (size.Width > 0 && size.Height > 0) pool!.Recreate(projectedDevice!, DirectXPixelFormat.B8G8R8A8UIntNormalized, 2, size);
                throw NoFrame("Target window resized; capture pool recreated without reusing an old frame.");
            }
            if (DwmGetWindowAttribute(target, 9, out Rect bounds, Marshal.SizeOf<Rect>()) < 0)
                throw new InvalidOperationException("Target window frame bounds unavailable.");
            int left = x - bounds.Left, top = y - bounds.Top;
            if (left < 0 || top < 0 || left + width > frame.ContentSize.Width || top + height > frame.ContentSize.Height)
                throw NoFrame($"Window/client geometry changed during capture: crop={left},{top},{width},{height}; frame={frame.ContentSize.Width},{frame.ContentSize.Height}.");
            nint surface = 0, access = 0, texture = 0, staging = 0; bool mapped = false;
            try
            {
                surface = MarshalInterface<IDirect3DSurface>.FromManaged(frame.Surface);
                Check(Marshal.QueryInterface(surface, in AccessId, out access));
                Check(Call<GetInterface>(access, 3)(access, in TextureId, out texture));
                var td = new TextureDescription(); Call<GetTextureDesc>(texture, 10)(texture, ref td);
                if (td.Format != 87) throw new InvalidOperationException("Unexpected window-capture surface format.");
                td.Usage = 3; td.Bind = 0; td.CpuAccess = 0x20000; td.Misc = 0;
                Check(Call<CreateTexture>(device, 5)(device, ref td, 0, out staging));
                Call<CopyResource>(context, 47)(context, staging, texture);
                Check(Call<Map>(context, 14)(context, staging, 0, 1, 0, out var map)); mapped = true;
                var pixels = new byte[checked(width * height * 4)];
                for (int row = 0; row < height; row++)
                    Marshal.Copy(map.Data + (top + row) * (int)map.RowPitch + left * 4, pixels, row * width * 4, width * 4);
                return pixels;
            }
            finally
            {
                if (mapped) Call<Unmap>(context, 15)(context, staging, 0);
                foreach (var value in new[] { staging, texture, access, surface }) if (value != 0) Marshal.Release(value);
            }
        }
    }
    private void Initialize(nint target)
    {
        Dispose();
        if (!GraphicsCaptureSession.IsSupported()) throw new NotSupportedException("Windows Graphics Capture is unsupported.");
        nint dxgi = 0, inspectable = 0, name = 0, factory = 0, captureItem = 0;
        try
        {
            Check(D3D11CreateDevice(0, 1, 0, 0x20, 0, 0, 7, out device, out _, out context));
            var dxgiId = new Guid("54ec77fa-1377-44e6-8c32-88fd5f44c84c");
            Check(Marshal.QueryInterface(device, in dxgiId, out dxgi));
            Check(CreateDirect3D11DeviceFromDXGIDevice(dxgi, out inspectable));
            projectedDevice = MarshalInterface<IDirect3DDevice>.FromAbi(inspectable);
            const string className = "Windows.Graphics.Capture.GraphicsCaptureItem";
            Check(WindowsCreateString(className, className.Length, out name));
            var interopId = new Guid("3628e81b-3cac-4c60-b7f4-23ce0e0c3356");
            Check(RoGetActivationFactory(name, in interopId, out factory));
            var itemId = new Guid("79c3f95b-31f7-4ec2-a464-632ef5d30760");
            Check(Call<CreateForWindow>(factory, 3)(factory, target, in itemId, out captureItem));
            item = MarshalInterface<GraphicsCaptureItem>.FromAbi(captureItem); size = item.Size;
            pool = Direct3D11CaptureFramePool.CreateFreeThreaded(projectedDevice, DirectXPixelFormat.B8G8R8A8UIntNormalized, 2, size);
            session = pool.CreateCaptureSession(item); session.IsCursorCaptureEnabled = false;
            session.StartCapture(); window = target;
        }
        catch { Dispose(); throw; }
        finally
        {
            foreach (var value in new[] { captureItem, factory, inspectable, dxgi }) if (value != 0) Marshal.Release(value);
            if (name != 0) Check(WindowsDeleteString(name));
        }
    }
    public void Dispose()
    {
        session?.Dispose(); pool?.Dispose(); session = null; pool = null; item = null; projectedDevice = null;
        foreach (var value in new[] { context, device }) if (value != 0) Marshal.Release(value);
        context = device = window = 0;
    }
    private static NoNewCaptureFrameException NoFrame(string message) => new(message);
    private static void Check(int result) => Marshal.ThrowExceptionForHR(result);
    private static T Call<T>(nint value, int index) where T : Delegate => Marshal.GetDelegateForFunctionPointer<T>(Marshal.ReadIntPtr(Marshal.ReadIntPtr(value), index * IntPtr.Size));
    [StructLayout(LayoutKind.Sequential)] private struct Rect { public int Left, Top, Right, Bottom; }
    [StructLayout(LayoutKind.Sequential)] private struct TextureDescription { public uint Width, Height, Mips, Array, Format, Samples, Quality, Usage, Bind, CpuAccess, Misc; }
    [StructLayout(LayoutKind.Sequential)] private struct Mapped { public nint Data; public uint RowPitch, DepthPitch; }
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int CreateForWindow(nint self, nint target, in Guid iid, out nint result);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int GetInterface(nint self, in Guid iid, out nint result);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate void GetTextureDesc(nint self, ref TextureDescription description);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int CreateTexture(nint self, ref TextureDescription description, nint data, out nint texture);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate void CopyResource(nint self, nint destination, nint source);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int Map(nint self, nint resource, uint subresource, uint type, uint flags, out Mapped map);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate void Unmap(nint self, nint resource, uint subresource);
    [DllImport("d3d11.dll")] private static extern int D3D11CreateDevice(nint adapter, uint driver, nint software, uint flags, nint levels, uint count, uint sdk, out nint device, out uint level, out nint context);
    [DllImport("d3d11.dll")] private static extern int CreateDirect3D11DeviceFromDXGIDevice(nint dxgiDevice, out nint graphicsDevice);
    [DllImport("combase.dll", CharSet = CharSet.Unicode)] private static extern int WindowsCreateString(string value, int length, out nint result);
    [DllImport("combase.dll")] private static extern int WindowsDeleteString(nint value);
    [DllImport("combase.dll")] private static extern int RoGetActivationFactory(nint name, in Guid iid, out nint factory);
    [DllImport("dwmapi.dll")] private static extern int DwmGetWindowAttribute(nint window, uint attribute, out Rect bounds, int size);
}
