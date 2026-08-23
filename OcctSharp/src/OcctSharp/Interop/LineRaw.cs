using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct LineRaw
{
    internal LineRaw(XyzRaw origin, XyzRaw direction) { Origin = origin; Direction = direction; }
    internal readonly XyzRaw Origin;
    internal readonly XyzRaw Direction;
}
