using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct DrawingProjectionRaw(
    XyzRaw Origin,
    XyzRaw ViewDirection,
    XyzRaw UpDirection,
    int Perspective,
    double Focus);

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct DrawingPolylineRaw(int PointOffset, int PointCount, int Closed);

internal static partial class NativeMethods
{
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_drawing_compute")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus DrawingCompute(
        nint* shapes,
        int shapeCount,
        DrawingProjectionRaw projection,
        int exact,
        int isoCount,
        double deflection,
        nint* layers,
        int layerCapacity);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_drawing_section")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus DrawingSection(
        ShapeHandle shape,
        XyzRaw planeOrigin,
        XyzRaw planeNormal,
        int approximate,
        out nint section);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_drawing_polyline_count")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus DrawingPolylineCount(
        ShapeHandle shape,
        int samplesPerCurve,
        out int polylineCount,
        out int pointCount);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_drawing_polyline_copy")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus DrawingPolylineCopy(
        ShapeHandle shape,
        int samplesPerCurve,
        DrawingPolylineRaw* polylines,
        int polylineCapacity,
        XyzRaw* points,
        int pointCapacity,
        out int polylinesWritten,
        out int pointsWritten);
}
