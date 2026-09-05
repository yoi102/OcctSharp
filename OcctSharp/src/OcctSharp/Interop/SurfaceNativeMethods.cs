using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct SurfaceInfoRaw(int Kind, int Orientation, int ClosedU, int ClosedV, int PeriodicU, int PeriodicV, double FirstU, double LastU, double FirstV, double LastV, double PeriodU, double PeriodV);

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct SurfaceSampleRaw(int State, int NormalDefined, int CurvatureDefined, int Singular, SketchPoint2dRaw Uv, XyzRaw Point, XyzRaw Du, XyzRaw Dv, XyzRaw Normal, double MinimumCurvature, double MaximumCurvature, double MeanCurvature, double GaussianCurvature);

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct SurfacePointSolutionRaw(int SourceIndex, int State, SketchPoint2dRaw Uv, XyzRaw Point, double Distance);

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct SurfaceCurveInfoRaw(int Degree, int Periodic, int PoleCount, int KnotCount, int Reversed, int Exact, int ParameterPreserved, int Reserved, double First, double Last, double SourceFirst, double SourceLast, double Residual);

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct SurfaceProjectionOptionsRaw(double Tolerance3d, double Tolerance2d, double MaximumDistance, int LimitToFace, int MaximumDegree, int MaximumSegments, int Continuity);

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct SurfaceRepairInfoRaw(int ValidBefore, int ValidAfter, int EdgesBefore, int EdgesAfter, int MissingBefore, int MissingAfter, int InconsistentBefore, int InconsistentAfter, double ToleranceBefore, double ToleranceAfter);

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct SurfaceBoundaryInfoRaw(int LoopIndex, int Outer, int Orientation, int Seam, int Degenerate, int Reserved, double Length);

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct SurfaceIntersectionRaw(int Kind, int State, double FirstParameter, double LastParameter, XyzRaw FirstPoint, XyzRaw LastPoint, SketchPoint2dRaw FirstUv, SketchPoint2dRaw LastUv);

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct SurfaceCurveSampleRaw(double Parameter, SketchPoint2dRaw Uv, XyzRaw Point, XyzRaw Tangent);

internal static partial class NativeMethods
{
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_surface_describe")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SurfaceDescribe(ShapeHandle face, out SurfaceInfoRaw result);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_surface_evaluate")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SurfaceEvaluate(ShapeHandle face, SketchPoint2dRaw* points, int count, double tolerance, SurfaceSampleRaw* results);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_surface_classify")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SurfaceClassify(ShapeHandle face, SketchPoint2dRaw* points, int count, double tolerance, int* results);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_surface_project_points")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SurfaceProjectPoints(ShapeHandle face, XyzRaw* points, int pointCount, int limitToFace, double tolerance, SurfacePointSolutionRaw* results, int capacity, out int count);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_surface_iso")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SurfaceIso(ShapeHandle face, int direction, double parameter, double first, double last, out nint shape);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_surface_sample_curve")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SurfaceSampleCurve(ShapeHandle face, ShapeHandle edge, int count, double tolerance, SurfaceCurveSampleRaw* results);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_surface_curve_definition")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SurfaceCurveDefinition(ShapeHandle face, ShapeHandle edge, int branch, int derive, double tolerance, out SurfaceCurveInfoRaw info, SketchPoint2dRaw* poles, double* weights, int poleCapacity, double* knots, int* multiplicities, int knotCapacity);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_surface_fit_uv")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SurfaceFitUv(SketchPoint2dRaw* points, int count, int interpolate, int periodic, int minimumDegree, int maximumDegree, int continuity, double tolerance, out SurfaceCurveInfoRaw info, SketchPoint2dRaw* poles, double* weights, int poleCapacity, double* knots, int* multiplicities, int knotCapacity);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_surface_offset_uv")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SurfaceOffsetUv(SketchCurveRaw* curve, double distance, double tolerance, out SurfaceCurveInfoRaw info, SketchPoint2dRaw* poles, double* weights, int poleCapacity, double* knots, int* multiplicities, int knotCapacity);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_surface_lift_curve")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SurfaceLiftCurve(ShapeHandle face, SketchCurveRaw* curve, int build3d, double tolerance, out nint shape);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_surface_project_shape")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SurfaceProjectShape(ShapeHandle face, ShapeHandle source, in SurfaceProjectionOptionsRaw options, out nint shape);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_surface_make_wire")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SurfaceMakeWire(ShapeHandle face, nint* edges, int count, double tolerance, out nint shape);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_surface_make_face")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SurfaceMakeFace(ShapeHandle face, nint* wires, int count, double tolerance, out nint shape);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_surface_repair")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SurfaceRepair(ShapeHandle shape, int perform, double tolerance, double maximumTolerance, out SurfaceRepairInfoRaw info, out nint result);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_surface_boundary")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SurfaceBoundary(ShapeHandle face, SurfaceBoundaryInfoRaw* info, nint* edges, int capacity, out int count);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_surface_split")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SurfaceSplit(ShapeHandle face, nint* tools, int count, out nint shape);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_surface_create_analytic")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SurfaceCreateAnalytic(int kind, in SketchPlaneRaw plane, double radius, double secondary, double* bounds, double tolerance, out nint shape);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_surface_section")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SurfaceSection(ShapeHandle first, ShapeHandle second, double tolerance, out nint shape);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_surface_intersect_curve")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SurfaceIntersectCurve(ShapeHandle face, ShapeHandle edge, double tolerance, SurfaceIntersectionRaw* results, int capacity, out int count);

}
