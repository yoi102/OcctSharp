using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct EdgeCurveSnapshotRaw
{
    internal readonly int CurveType;
    internal readonly double FirstParameter;
    internal readonly double LastParameter;
    internal readonly XyzRaw StartPoint;
    internal readonly XyzRaw EndPoint;
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct FaceSurfaceSnapshotRaw
{
    internal readonly int SurfaceType;
    internal readonly double FirstUParameter;
    internal readonly double LastUParameter;
    internal readonly double FirstVParameter;
    internal readonly double LastVParameter;
}
