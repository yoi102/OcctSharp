using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

internal static partial class NativeMethods
{
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_oriented_bounding_box")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetOrientedBoundingBox(
        ShapeHandle shape,
        out OrientedBoundingBoxRaw bounds);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_digital_mockup_candidate_pairs")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial NativeStatus GetDigitalMockupCandidatePairs(
        nint* shapes,
        int shapeCount,
        double expansion,
        int* pairs,
        int pairCapacity,
        out int pairCount,
        out int axisComparisonCount);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_digital_mockup_pair_analyze")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus AnalyzeDigitalMockupPair(
        ShapeHandle first,
        ShapeHandle second,
        double confusionTolerance,
        double fuzzyTolerance,
        int runParallel,
        int nonDestructive,
        out int classification,
        out double distance,
        out double overlapVolume,
        out nint issueShape);
}
