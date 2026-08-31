using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct OrientedBoundingBoxRaw
{
    internal readonly XyzRaw Center;
    internal readonly XyzRaw XDirection;
    internal readonly XyzRaw YDirection;
    internal readonly XyzRaw ZDirection;
    internal readonly double HalfSizeX;
    internal readonly double HalfSizeY;
    internal readonly double HalfSizeZ;
}
