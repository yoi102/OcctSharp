namespace OcctSharp;

#pragma warning disable CS1591
public enum ViewerLightKind { Ambient, Directional, Positional, Spot }
public enum ViewerRenderMode { Raster, PathTracing }
public enum ViewerTransparencyMethod { Blend, WeightedOit }
public enum ViewerToneMapping { Disabled, Filmic }
public enum ViewerShading { Unlit = 0, Phong = 3, Pbr = 4 }
public enum ViewerFaceCulling { Automatic, DoubleSided, Back, Front }
public enum ViewerAlphaMode { Opaque, Mask, Blend, MaskBlend }
public enum ViewerTextureFilter { Nearest, Bilinear, Trilinear }
public enum ViewerTextureAnisotropy { Off, Fast, Middle, Quality }
public enum ViewerPixelFormat { Rgba8, Bgra8 }

/// <summary>Copied driver limits; support does not guarantee every scene can fit in GPU memory.</summary>
public sealed record ViewerRenderCapabilities(int MaximumLights, int MaximumTextureSize, int MaximumDumpWidth,
    int MaximumDumpHeight, int MaximumTextureUnits, int MaximumMsaaSamples, bool SupportsPbr, bool SupportsPathTracing,
    bool SupportsSrgb, bool SupportsWeightedOit, bool SupportsWeightedOitMsaa, double MaximumAnisotropy);

/// <summary>One bounded render request. Unsupported combinations reject before changing the current profile.</summary>
public sealed record ViewerRenderProfile
{
    /// <summary>View lighting pipeline. PBR appearances and environments require Pbr here, not only on the presentation.</summary>
    public ViewerShading Shading { get; init; } = ViewerShading.Phong;
    public ViewerRenderMode Mode { get; init; }
    public int MsaaSamples { get; init; }
    public double ResolutionScale { get; init; } = 1;
    public ViewerTransparencyMethod Transparency { get; init; }
    public double OitDepthFactor { get; init; }
    public ViewerToneMapping ToneMapping { get; init; }
    public double Exposure { get; init; }
    public double WhitePoint { get; init; } = 1;
    public int EnvironmentPower { get; init; } = 5;
    public int EnvironmentLevels { get; init; } = 4;
    public int DiffuseSamples { get; init; } = 64;
    public int SpecularSamples { get; init; } = 32;
    public double BakeProbability { get; init; } = .99;
}

/// <summary>Portable copied light definition. Color is linear, position/range use model world units; spot angle is radians.</summary>
public sealed record ViewerLightDefinition(ViewerLightKind Kind, ViewerColor Color)
{
    public double Intensity { get; init; } = 1;
    public bool Active { get; init; } = true;
    public bool Headlight { get; init; }
    public GpPoint Position { get; init; }
    public GpXyz Direction { get; init; } = new(0, 0, -1);
    public double ConstantAttenuation { get; init; } = 1;
    public double LinearAttenuation { get; init; }
    public double Range { get; init; }
    public double SpotAngle { get; init; } = Math.PI / 6;
    public double Concentration { get; init; } = .5;
}

public sealed record ViewerReviewMaterial(ViewerColor Color)
{
    public double Alpha { get; init; } = 1;
    public double Metallic { get; init; }
    public double Roughness { get; init; } = .5;
    public double IndexOfRefraction { get; init; } = 1.5;
    public double Emission { get; init; }
}

/// <summary>Review-only sampling/coordinate program; does not modify BRep UVs or authored mesh arrays.</summary>
public sealed record ViewerTextureMapping
{
    public bool Planar { get; init; }
    public ViewerPlaneEquation PlaneS { get; init; } = new(1, 0, 0, 0);
    public ViewerPlaneEquation PlaneT { get; init; } = new(0, 1, 0, 0);
    public double ScaleS { get; init; } = 1;
    public double ScaleT { get; init; } = 1;
    public double TranslationS { get; init; }
    public double TranslationT { get; init; }
    public double RotationDegrees { get; init; }
    public bool Repeat { get; init; }
    public ViewerTextureFilter Filter { get; init; } = ViewerTextureFilter.Bilinear;
    /// <summary>OCCT's driver-relative quality level, not an exact numeric anisotropy factor.</summary>
    public ViewerTextureAnisotropy Anisotropy { get; init; }
}

/// <summary>Atomic presentation shading replacement; resetting restores the effective styles captured at first override.</summary>
public sealed record ViewerAppearanceProfile
{
    public ViewerReviewMaterial Front { get; init; } = new(new(1, 1, 1));
    public ViewerReviewMaterial Back { get; init; } = new(new(1, 1, 1));
    public bool DistinguishSides { get; init; }
    public ViewerShading Shading { get; init; } = ViewerShading.Phong;
    public ViewerFaceCulling Culling { get; init; } = ViewerFaceCulling.DoubleSided;
    public ViewerAlphaMode AlphaMode { get; init; } = ViewerAlphaMode.Opaque;
    public double AlphaCutoff { get; init; } = .5;
    public ViewerTextureMapping Mapping { get; init; } = new();
}

/// <summary>Bounded immutable sRGB RGBA/BGRA source image with explicit row origin and stride.</summary>
public sealed class ViewerPixelImage
{
    private readonly byte[] pixels;
    public ViewerPixelImage(int width, int height, ReadOnlySpan<byte> pixels, int stride = 0,
        ViewerPixelFormat format = ViewerPixelFormat.Rgba8, bool bottomUp = false)
    {
        if (width <= 0 || height <= 0 || width > 16384 || height > 16384 || (long)width * height > 16777216)
            throw new ArgumentOutOfRangeException(nameof(width));
        if (stride == 0) stride = checked(width * 4);
        if (stride < width * 4 || (long)stride * height != pixels.Length || pixels.Length > 67108864 || !Enum.IsDefined(format))
            throw new ArgumentException("Invalid pixel buffer/stride/format.");
        Width = width; Height = height; Stride = stride; Format = format; BottomUp = bottomUp; this.pixels = pixels.ToArray();
    }
    public int Width { get; }
    public int Height { get; }
    public int Stride { get; }
    public ViewerPixelFormat Format { get; }
    public bool BottomUp { get; }
    public byte[] CopyPixels() => (byte[])pixels.Clone();
    internal ReadOnlySpan<byte> Pixels => pixels;
}

public sealed record ViewerLayerProfile(bool DepthTest = true, bool DepthWrite = true, bool ClearDepth = false, bool Immediate = false);
public sealed record ViewerCaptureOptions(int Width = 640, int Height = 480, int TileSize = 0,
    bool AdjustAspect = true, ViewerReviewLayer? Layer = null, bool SingleLayer = false, bool DefaultLayer = false);
