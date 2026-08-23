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
