using OcctSharp.Interop;
using System.Runtime.InteropServices;

namespace OcctSharp;

public sealed partial class ViewerRendering
{
    /// <summary>Captures independent top-down RGBA bytes. Requires this viewer's live HWND/context.</summary>
    public ViewerColorFrame CaptureColor(ViewerCaptureOptions? options = null)
    {
        options ??= new(); var (bytes, _) = Capture(options, false); return new(options.Width, options.Height, bytes, CaptureScope(options,false));
    }
    /// <summary>Captures normalized float depth and copied camera matrices; reconstruction survives camera edits and viewer disposal.
    /// With no explicit layer, captures the default model layer only, excluding depth-clearing OSD/overlay layers.</summary>
    public unsafe ViewerDepthFrame CaptureDepth(ViewerCaptureOptions? options = null)
    {
        options ??= new(); var (bytes, info) = Capture(options, true);
        float[] values = MemoryMarshal.Cast<byte, float>(bytes).ToArray(); var matrix = new double[16];
        for (int i = 0; i < 16; ++i) matrix[i] = info.InverseViewProjection[i];
        return new(options.Width, options.Height, values, matrix, info.ZeroToOneDepth != 0, info.Near, info.Far, CaptureScope(options,true));
    }
    private ViewerFrameScope CaptureScope(ViewerCaptureOptions options, bool depth)
    {
        if (options.Layer is { } layer) {
            long[] ids = options.SingleLayer ? [layer.Id] : new long[] { 0 }.Concat(layers.TakeWhile(x => !ReferenceEquals(x,layer)).Select(x => x.Id)).Append(layer.Id).ToArray();
            return new(ids,false);
        }
        if (depth || options.DefaultLayer) return new([0],false);
        return new(new long[] { 0 }.Concat(layers.Select(x => x.Id)).ToArray(),true);
    }
    private unsafe (byte[], FrameInfoRaw) Capture(ViewerCaptureOptions options, bool depth)
    {
        EnsureThread(); options.Layer?.Ensure(this);
        if (options.Width <= 0 || options.Height <= 0 || options.Width > 16384 || options.Height > 16384 || (long)options.Width * options.Height > 16777216)
            throw new ArgumentOutOfRangeException(nameof(options));
        if (options.DefaultLayer && options.Layer is not null) throw new ArgumentException("Choose either default or a managed review layer.");
        if (options.SingleLayer && options.Layer is null && !options.DefaultLayer) throw new ArgumentException("Single-layer capture requires an explicit layer.");
        var request = new FrameRequestRaw { Width = options.Width, Height = options.Height, Depth = depth ? 1 : 0,
            TileSize = options.TileSize, AdjustAspect = options.AdjustAspect ? 1 : 0, SingleLayer = options.SingleLayer ? 1 : 0,
            Layer = options.Layer?.Id ?? (options.DefaultLayer ? 0 : -1) };
        var bytes = new byte[checked(options.Width * options.Height * 4)];
        fixed (byte* p = bytes) {
            NativeError.ThrowIfFailed(NativeMethods.FrameCapture(viewer.Handle, in request, p, bytes.Length, out var info), "viewer_frame_capture"); return (bytes, info);
        }
    }
}
