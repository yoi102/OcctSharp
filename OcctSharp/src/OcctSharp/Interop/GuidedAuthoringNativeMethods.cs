using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct AuthoringInfoRaw
{
    internal int Ready, Done, Valid, Solid, AlgorithmStatus, HistoryCount, SectionCount, ContinuityLimit;
    internal double ApproximationError;
    internal int ErrorAvailable, Reserved;
}
[StructLayout(LayoutKind.Sequential)]
internal struct AuthoringHistoryRaw { internal int SourceIndex, SubshapeIndex, SourceKind, Kind; }
[StructLayout(LayoutKind.Sequential)]
internal struct SweepSectionRaw { internal int ShapeIndex, LocationIndex, Contact, Correction; }
[StructLayout(LayoutKind.Sequential)]
internal struct SweepOptionsRaw
{
    internal int Frame, SecondaryIndex, Curvilinear, Contact, Transition, MaximumDegree, MaximumSegments, ForceC1;
    internal int SolidPolicy, SimulationCount, Operation, Reserved;
    internal double Tolerance3d, ToleranceBoundary, ToleranceAngular;
    internal XyzRaw Origin, Direction, XDirection;
}
[StructLayout(LayoutKind.Sequential)]
internal struct LoftOptionsRaw
{
    internal int Solid, Ruled, Compatibility, Smoothing, MaximumDegree, Continuity, Parameterization, Reserved;
    internal double Tolerance, Weight1, Weight2, Weight3;
}
internal static partial class NativeMethods
{
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_authoring_copy_inputs")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus AuthoringCopyInputs(nint* inputs, int count, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_authoring_history")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus AuthoringHistory(FeatureResultHandle result, int index, out AuthoringHistoryRaw info, out nint shape);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_guided_sweep")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus GuidedSweep(nint* inputs, int count, SweepSectionRaw* sections, int sectionCount,
        in SweepOptionsRaw options, LawInputRaw* law, out AuthoringInfoRaw info, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_guided_loft")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus GuidedLoft(nint* inputs, int count, in LoftOptionsRaw options,
        out AuthoringInfoRaw info, out nint result);
}
