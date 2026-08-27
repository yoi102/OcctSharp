using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct ShapeDistanceResultRaw
{
    internal readonly double Distance;
    internal readonly XyzRaw PointOnFirst;
    internal readonly XyzRaw PointOnSecond;
    internal readonly int SolutionCount;
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct BooleanHistorySummaryRaw
{
    internal readonly int LeftSourceCount;
    internal readonly int LeftModifiedSourceCount;
    internal readonly int LeftGeneratedSourceCount;
    internal readonly int LeftDeletedSourceCount;
    internal readonly int LeftModifiedResultCount;
    internal readonly int LeftGeneratedResultCount;
    internal readonly int RightSourceCount;
    internal readonly int RightModifiedSourceCount;
    internal readonly int RightGeneratedSourceCount;
    internal readonly int RightDeletedSourceCount;
    internal readonly int RightModifiedResultCount;
    internal readonly int RightGeneratedResultCount;
}
