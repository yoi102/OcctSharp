using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctSharp.Interop;
[StructLayout(LayoutKind.Sequential)]
internal struct PatchOptionsRaw
{
    internal int Operation, Style, WithRatio, MinimumMultiplicity, Bezier, Reserved;
    internal double First, Last, FirstU, LastU, FirstV, LastV, Tolerance;
}
[StructLayout(LayoutKind.Sequential)]
internal struct PatchSpanRaw
{
    internal int SourceIndex, UIndex, VIndex, Orientation;
    internal double First, Last, FirstV, LastV, ResultFirst, ResultLast;
}
internal static partial class NativeMethods
{
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_patch_convert")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus PatchConvert(nint* inputs, int inputCount, in PatchOptionsRaw options,
        PatchSpanRaw* spans, int capacity, out int count, out AuthoringInfoRaw info, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_authoring_surface_join")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus SurfaceJoin(ShapeHandle boundary, ShapeHandle first, ShapeHandle second,
        int count, double tolerance, ConstraintResidualRaw* residuals, int capacity);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_authoring_curve_join")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CurveJoin(ShapeHandle first, ShapeHandle second, double firstParameter,
        double secondParameter, int reverseSecond, out ConstraintResidualRaw residual);
}
