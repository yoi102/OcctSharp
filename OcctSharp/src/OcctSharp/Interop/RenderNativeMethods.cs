using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct RenderCapsRaw { internal int MaxLights, MaxTexture, MaxDumpX, MaxDumpY, MaxTextureUnits, MaxMsaa, Pbr, Raytracing, Srgb, Oit, OitMsaa, Reserved; internal double MaxAnisotropy; }
[StructLayout(LayoutKind.Sequential)]
internal struct RenderProfileRaw { internal int Mode, Msaa, Transparency, ToneMapping; internal double ResolutionScale, OitDepthFactor, Exposure, WhitePoint; internal int EnvironmentPower, EnvironmentLevels, DiffuseSamples, SpecularSamples; internal double BakeProbability; internal int Shading, Reserved; }
[StructLayout(LayoutKind.Sequential)]
internal struct LightRaw { internal long Id; internal int Kind, Active, Headlight, Reserved; internal double Red, Green, Blue, Intensity, X, Y, Z, Dx, Dy, Dz, ConstantAttenuation, LinearAttenuation, Range, Angle, Concentration; }
[StructLayout(LayoutKind.Sequential)]
internal struct PixelInputRaw { internal int Width, Height, Stride, Format, BottomUp, Reserved; }
[StructLayout(LayoutKind.Sequential)]
internal struct ReviewMaterialRaw { internal double Red, Green, Blue, Alpha, Metallic, Roughness, Ior, Emission; }
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct AppearanceRaw
{
    internal ReviewMaterialRaw Front, Back;
    internal int Shading, Distinguish, Culling, AlphaMode;
    internal double AlphaCutoff;
    internal long Texture;
    internal int Planar, Repeat, Filter, Anisotropy;
    internal double ScaleS, ScaleT, TranslateS, TranslateT, Rotation;
    internal fixed double PlaneS[4]; internal fixed double PlaneT[4];
}
[StructLayout(LayoutKind.Sequential)]
internal struct ReviewLayerRaw { internal int DepthTest, DepthWrite, ClearDepth, Immediate; }
[StructLayout(LayoutKind.Sequential)]
internal struct FrameRequestRaw { internal int Width, Height, Depth, TileSize, AdjustAspect, SingleLayer; internal long Layer; }
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct FrameInfoRaw { internal int Width, Height, Stride, ZeroToOneDepth; internal double Near, Far; internal fixed double InverseViewProjection[16]; }
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct ReviewCameraRaw { internal fixed double Eye[3]; internal fixed double Target[3]; internal fixed double Up[3]; internal double Aspect, Scale, FovY, Near, Far; internal int Perspective, AutoDepth; }

internal static partial class NativeMethods
{
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_viewer_review_camera")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus ReviewCamera(ViewerHandle viewer, ReviewCameraRaw* requested, out ReviewCameraRaw effective);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_viewer_render_caps")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus RenderCaps(ViewerHandle viewer, out RenderCapsRaw value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_viewer_render_profile")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus RenderProfile(ViewerHandle viewer, RenderProfileRaw* requested, out RenderProfileRaw effective);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_viewer_lights_replace")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus LightsReplace(ViewerHandle viewer, LightRaw* input, int count, long* ids, int capacity);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_viewer_lights_snapshot")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus LightsSnapshot(ViewerHandle viewer, LightRaw* output, int capacity, out int count);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_viewer_texture_pixels")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus TexturePixels(ViewerHandle viewer, long id, in PixelInputRaw input, byte* bytes, int length, out long result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_viewer_texture_file", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus TextureFile(ViewerHandle viewer, string path, out long id, out PixelInputRaw description);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_viewer_texture_remove")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus TextureRemove(ViewerHandle viewer, long id);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_viewer_appearance")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus ReviewAppearance(ViewerHandle viewer, long presentation, AppearanceRaw* profile);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_viewer_environment_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus EnvironmentCreate(ViewerHandle viewer, long* images, int count, int* order, out long id);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_viewer_environment_set")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus EnvironmentSet(ViewerHandle viewer, long id, int background, int lighting);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_viewer_environment_remove")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus EnvironmentRemove(ViewerHandle viewer, long id);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_viewer_layer_set")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ReviewLayerSet(ViewerHandle viewer, long id, in ReviewLayerRaw input, out long result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_viewer_layer_assign")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ReviewLayerAssign(ViewerHandle viewer, long presentation, long layer);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_viewer_layer_remove")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ReviewLayerRemove(ViewerHandle viewer, long id);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_viewer_frame_capture")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus FrameCapture(ViewerHandle viewer, in FrameRequestRaw request, byte* output, int capacity, out FrameInfoRaw info);
}
