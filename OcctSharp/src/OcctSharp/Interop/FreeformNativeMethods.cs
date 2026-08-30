using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct FreeformCurveInfoRaw(
    int Kind, int Degree, int Periodic, int Rational, int PoleCount, int KnotCount,
    double FirstParameter, double LastParameter);

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct FreeformSurfaceInfoRaw(
    int Kind, int UDegree, int VDegree, int UPeriodic, int VPeriodic, int Rational,
    int UPoleCount, int VPoleCount, int UKnotCount, int VKnotCount,
    double FirstU, double LastU, double FirstV, double LastV);

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct FreeformSolutionRaw(
    XyzRaw FirstPoint, XyzRaw SecondPoint, double FirstParameter,
    double SecondParameter, double ThirdParameter, double Distance);

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct FreeformDiagnosticsRaw(
    int Status, int InputCount, int ResultCount, int ModifiedCount, int GeneratedCount,
    int DeletedCount, int IsValid, int IsClosed, double G0Error, double G1Error,
    double G2Error, double ApproximationError);

internal static partial class NativeMethods
{
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_freeform_curve_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus FreeformCurveCreate(
        int kind, XyzRaw* poles, double* weights, int poleCount, double* knots,
        int* multiplicities, int knotCount, int degree, int periodic, out nint edge);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_freeform_curve_interpolate")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus FreeformCurveInterpolate(
        XyzRaw* points, int pointCount, XyzRaw* endpointTangents, int tangentCount,
        int periodic, double tolerance, out nint edge);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_freeform_curve_approximate")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus FreeformCurveApproximate(
        XyzRaw* points, int pointCount, int minimumDegree, int maximumDegree,
        int continuity, double tolerance, out nint edge);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_freeform_curve_info")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus FreeformCurveInfo(ShapeHandle edge, out FreeformCurveInfoRaw info);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_freeform_curve_copy_definition")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus FreeformCurveCopyDefinition(
        ShapeHandle edge, XyzRaw* poles, int poleCapacity, double* weights,
        int weightCapacity, double* knots, int knotCapacity, int* multiplicities,
        int multiplicityCapacity);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_freeform_curve_edit")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus FreeformCurveEdit(
        ShapeHandle edge, int operation, int degree, double firstParameter,
        double lastParameter, out nint result);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_freeform_curve_split")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus FreeformCurveSplit(
        ShapeHandle edge, double* parameters, int parameterCount, nint* results,
        int capacity, out int written);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_freeform_curve_project_count")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus FreeformCurveProjectCount(ShapeHandle edge, XyzRaw point, out int count);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_freeform_curve_project_copy")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus FreeformCurveProjectCopy(
        ShapeHandle edge, XyzRaw point, FreeformSolutionRaw* solutions, int capacity, out int written);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_freeform_curve_extrema_count")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus FreeformCurveExtremaCount(ShapeHandle first, ShapeHandle second, out int count);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_freeform_curve_extrema_copy")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus FreeformCurveExtremaCopy(
        ShapeHandle first, ShapeHandle second, FreeformSolutionRaw* solutions, int capacity, out int written);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_freeform_curve_face_intersection_count")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus FreeformCurveFaceIntersectionCount(ShapeHandle edge, ShapeHandle face, out int count);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_freeform_curve_face_intersection_copy")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus FreeformCurveFaceIntersectionCopy(
        ShapeHandle edge, ShapeHandle face, FreeformSolutionRaw* solutions, int capacity, out int written);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_freeform_planar_profile")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus FreeformPlanarProfile(
        XyzRaw* points, int pointCount, XyzRaw origin, XyzRaw normal, XyzRaw xDirection,
        int interpolate, double tolerance, out nint wire);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_freeform_planar_offset")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus FreeformPlanarOffset(
        ShapeHandle wire, double distance, double altitude, int joinType, out nint result);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_freeform_surface_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus FreeformSurfaceCreate(
        int kind, XyzRaw* poles, double* weights, int uPoleCount, int vPoleCount,
        double* uKnots, int* uMultiplicities, int uKnotCount,
        double* vKnots, int* vMultiplicities, int vKnotCount,
        int uDegree, int vDegree, int uPeriodic, int vPeriodic,
        double* bounds, double tolerance, out nint face);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_freeform_surface_approximate")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus FreeformSurfaceApproximate(
        XyzRaw* points, int uCount, int vCount, int minimumDegree, int maximumDegree,
        int continuity, double tolerance, out nint face);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_freeform_surface_info")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus FreeformSurfaceInfo(ShapeHandle face, out FreeformSurfaceInfoRaw info);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_freeform_surface_copy_definition")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus FreeformSurfaceCopyDefinition(
        ShapeHandle face, XyzRaw* poles, int poleCapacity, double* weights, int weightCapacity,
        double* uKnots, int uKnotCapacity, int* uMultiplicities, int uMultiplicityCapacity,
        double* vKnots, int vKnotCapacity, int* vMultiplicities, int vMultiplicityCapacity);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_freeform_surface_edit")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus FreeformSurfaceEdit(
        ShapeHandle face, int operation, int uDegree, int vDegree, double* bounds,
        double tolerance, out nint result);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_freeform_ruled_face")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus FreeformRuledFace(ShapeHandle firstEdge, ShapeHandle secondEdge, out nint face);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_freeform_fill")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus FreeformFill(
        nint* edges, int edgeCount, XyzRaw* points, int pointCount, int continuity,
        double tolerance, out FreeformDiagnosticsRaw diagnostics, out nint face);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_freeform_split")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus FreeformSplit(
        nint* objects, int objectCount, nint* tools, int toolCount,
        out FreeformDiagnosticsRaw diagnostics, out nint result);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_freeform_pipe_shell")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus FreeformPipeShell(
        ShapeHandle spine, nint* profiles, int profileCount, int makeSolid, int frenet,
        int transitionMode, double tolerance, int maximumDegree, int maximumSegments,
        out FreeformDiagnosticsRaw diagnostics, out nint result);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_freeform_loft")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus FreeformLoft(
        nint* sections, int sectionCount, int makeSolid, int ruled, int smoothing,
        int continuity, int maximumDegree, double tolerance,
        out FreeformDiagnosticsRaw diagnostics, out nint result);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_freeform_heal")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus FreeformHeal(
        ShapeHandle shape, double tolerance, out FreeformDiagnosticsRaw diagnostics, out nint result);
}
