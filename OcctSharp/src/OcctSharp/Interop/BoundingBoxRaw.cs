using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct BoundingBoxRaw
{
    internal readonly double MinX;
    internal readonly double MinY;
    internal readonly double MinZ;
    internal readonly double MaxX;
    internal readonly double MaxY;
    internal readonly double MaxZ;
}
