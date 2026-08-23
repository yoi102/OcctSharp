using System.Runtime.InteropServices;
namespace OcctSharp.Interop;
[StructLayout(LayoutKind.Sequential)]
internal readonly struct Ax2Raw
{
    internal Ax2Raw(XyzRaw origin, XyzRaw xDirection, XyzRaw yDirection, XyzRaw direction) { Origin = origin; XDirection = xDirection; YDirection = yDirection; Direction = direction; }
    internal readonly XyzRaw Origin, XDirection, YDirection, Direction;
}
