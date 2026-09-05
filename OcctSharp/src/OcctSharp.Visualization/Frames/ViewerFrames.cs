namespace OcctSharp;

#pragma warning disable CS1591
/// <summary>Copied capture scope. Layer IDs are scoped to the original viewer, not portable asset identifiers.</summary>
public sealed class ViewerFrameScope
{
    internal ViewerFrameScope(long[] ids, bool overlays) { LayerIds = Array.AsReadOnly(ids); IncludesStandardOverlays = overlays; }
    public IReadOnlyList<long> LayerIds { get; }
    public bool IncludesStandardOverlays { get; }
}
/// <summary>Independent top-down RGBA8 framebuffer copy. RGB is display encoded; alpha is the renderer's composite coverage, not a straight-alpha asset.</summary>
public sealed class ViewerColorFrame
{
    private readonly byte[] pixels;
    internal ViewerColorFrame(int width, int height, byte[] ownedPixels, ViewerFrameScope scope) { Width = width; Height = height; pixels = ownedPixels; Scope = scope; }
    public ViewerFrameScope Scope { get; }
    /// <summary>Creates a framework-independent frame from copied top-down RGBA bytes.</summary>
    public static ViewerColorFrame FromRgba(int width, int height, ReadOnlySpan<byte> pixels)
    {
        if (width <= 0 || height <= 0 || (long)width * height > 16777216 || (long)width * height * 4 != pixels.Length)
            throw new ArgumentException("Invalid bounded RGBA frame dimensions or buffer length.");
        return new(width,height,pixels.ToArray(),new([],false));
    }
    public int Width { get; }
    public int Height { get; }
    public int Stride => Width * 4;
    public string ChannelOrder { get; } = "RGBA";
    public bool IsTopDown { get; } = true;
    public byte[] CopyPixels() => (byte[])pixels.Clone();
    /// <summary>Copies display RGB into opaque BGRA, suitable for a WPF Bgr32 thumbnail without misinterpreting composite alpha.</summary>
    public byte[] CopyOpaqueBgra()
    {
        var result = new byte[pixels.Length];
        for (int i = 0; i < pixels.Length; i += 4) { result[i] = pixels[i + 2]; result[i + 1] = pixels[i + 1]; result[i + 2] = pixels[i]; result[i + 3] = 255; }
        return result;
    }
}

/// <summary>Independent normalized depth samples and the capture's inverse projection. Does not reference the live viewer.</summary>
public sealed class ViewerDepthFrame
{
    private readonly float[] depths;
    private readonly double[] inverse;
    internal ViewerDepthFrame(int width, int height, float[] ownedDepth, double[] ownedInverse, bool zeroToOne, double near, double far, ViewerFrameScope scope)
    { Width = width; Height = height; depths = ownedDepth; inverse = ownedInverse; ZeroToOneProjection = zeroToOne; NearPlane = near; FarPlane = far; Scope = scope; }
    public ViewerFrameScope Scope { get; }
    public int Width { get; }
    public int Height { get; }
    public bool IsTopDown { get; } = true;
    public bool ZeroToOneProjection { get; }
    public double NearPlane { get; }
    public double FarPlane { get; }
    public float BackgroundDepth { get; } = 1;
    public float[] CopyDepths() => (float[])depths.Clone();
    public double[] CopyInverseViewProjection() => (double[])inverse.Clone();
    public float GetDepth(int x, int y) { ValidatePixel(x, y); return depths[y * Width + x]; }
    /// <summary>Reconstructs the pixel center. Background, clipped, nonfinite and singular points return false.</summary>
    public bool TryReconstruct(int x, int y, out GpPoint point)
    {
        ValidatePixel(x, y); point = default; double d = depths[y * Width + x];
        if (!double.IsFinite(d) || d < 0 || d >= 1) return false;
        double nx = (x + .5) * 2 / Width - 1, ny = 1 - (y + .5) * 2 / Height, nz = ZeroToOneProjection ? d : d * 2 - 1;
        double w = inverse[12] * nx + inverse[13] * ny + inverse[14] * nz + inverse[15];
        if (!double.IsFinite(w) || Math.Abs(w) < 1e-15) return false;
        double px = (inverse[0] * nx + inverse[1] * ny + inverse[2] * nz + inverse[3]) / w;
        double py = (inverse[4] * nx + inverse[5] * ny + inverse[6] * nz + inverse[7]) / w;
        double pz = (inverse[8] * nx + inverse[9] * ny + inverse[10] * nz + inverse[11]) / w;
        if (!double.IsFinite(px) || !double.IsFinite(py) || !double.IsFinite(pz)) return false;
        point = new(px, py, pz); return true;
    }
    private void ValidatePixel(int x, int y) { if ((uint)x >= (uint)Width || (uint)y >= (uint)Height) throw new ArgumentOutOfRangeException(nameof(x)); }
}
