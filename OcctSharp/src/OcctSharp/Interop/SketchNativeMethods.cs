using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct SketchPoint2dRaw(double X, double Y);

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct SketchPlaneRaw(XyzRaw Origin, XyzRaw XDirection, XyzRaw YDirection);

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct SketchCurveRaw
{
    internal int Kind;
    internal int Degree;
    internal int Periodic;
    internal int Rational;
    internal int Reversed;
    internal int PoleCount;
    internal int KnotCount;
    internal double FirstParameter;
    internal double LastParameter;
    internal double MajorRadius;
    internal double MinorRadius;
    internal double AxisAngle;
    internal SketchPoint2dRaw* Poles;
    internal double* Weights;
    internal double* Knots;
    internal int* Multiplicities;
}

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct SketchEvaluationRaw(
    SketchPoint2dRaw Point, SketchPoint2dRaw Derivative, double Parameter);

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct SketchProjectionRaw(
    SketchPoint2dRaw Point, double Parameter, double Distance);

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct SketchIntersectionRaw(
    SketchPoint2dRaw Point, double FirstParameter, double SecondParameter);

internal static partial class NativeMethods
{
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_sketch_curve_evaluate")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SketchCurveEvaluate(
        SketchCurveRaw* curve, double parameter, out SketchEvaluationRaw evaluation);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_sketch_curve_project")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SketchCurveProject(
        SketchCurveRaw* curve, SketchPoint2dRaw point, SketchProjectionRaw* results,
        int capacity, out int count);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_sketch_curve_intersect")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SketchCurveIntersect(
        SketchCurveRaw* first, SketchCurveRaw* second, double tolerance,
        SketchIntersectionRaw* results, int capacity, out int count);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_sketch_curve_make_edge")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SketchCurveMakeEdge(
        SketchCurveRaw* curve, SketchPlaneRaw* plane, out nint shape);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_sketch_profile_make_face")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SketchProfileMakeFace(
        ShapeHandle outerWire, nint* innerWires, int innerWireCount, out nint shape);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_sketch_wire_contains")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SketchWireContains(
        ShapeHandle wire, SketchPoint2dRaw point, double tolerance, out int inside);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_sketch_make_wire")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SketchMakeWire(
        nint* edges, int edgeCount, double tolerance, out nint shape);
}
