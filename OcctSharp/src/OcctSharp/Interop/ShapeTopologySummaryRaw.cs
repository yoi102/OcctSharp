using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct TopologyCountsRaw
{
    internal readonly int VertexCount;
    internal readonly int EdgeCount;
    internal readonly int WireCount;
    internal readonly int FaceCount;
    internal readonly int ShellCount;
    internal readonly int SolidCount;
    internal readonly int CompSolidCount;
    internal readonly int CompoundCount;
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct ShapeTopologySummaryRaw
{
    internal readonly TopologyCountsRaw UniqueCounts;
    internal readonly TopologyCountsRaw OccurrenceCounts;
    internal readonly int IsClosed;
    internal readonly int IsValid;
    internal readonly double MinVertexTolerance;
    internal readonly double MaxVertexTolerance;
    internal readonly double MinEdgeTolerance;
    internal readonly double MaxEdgeTolerance;
    internal readonly double MinFaceTolerance;
    internal readonly double MaxFaceTolerance;
}
