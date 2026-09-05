using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctSharp.Interop;
[StructLayout(LayoutKind.Sequential)]
internal struct FillConstraintRaw
{
    internal int Kind, ShapeIndex, SupportIndex, Order, Boundary, Required, Id, Reserved;
    internal double U, V;
    internal XyzRaw Point;
}
[StructLayout(LayoutKind.Sequential)]
internal struct FillOptionsRaw
{
    internal int Degree, PointsPerCurve, Iterations, Anisotropic, MaximumDegree, MaximumSegments, SeedIndex, VerificationSamples;
    internal double Tolerance2d, Tolerance3d, ToleranceAngular, ToleranceCurvature;
}
[StructLayout(LayoutKind.Sequential)]
internal struct ConstraintResidualRaw
{
    internal int Id, KernelIndex, Defined, Accepted;
    internal double Position, Angle, Curvature;
    internal int SampleCount, Required;
}
internal static partial class NativeMethods
{
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_constrained_fill")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus ConstrainedFill(nint* inputs, int inputCount, FillConstraintRaw* constraints, int count,
        in FillOptionsRaw options, ConstraintResidualRaw* residuals, int capacity, out AuthoringInfoRaw info, out nint result);
}
