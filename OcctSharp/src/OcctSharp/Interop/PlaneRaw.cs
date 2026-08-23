using System.Runtime.InteropServices;
namespace OcctSharp.Interop;
[StructLayout(LayoutKind.Sequential)]
internal readonly struct PlaneRaw
{
    internal PlaneRaw(XyzRaw origin, XyzRaw normal) { Origin = origin; Normal = normal; }
    internal readonly XyzRaw Origin, Normal;
}
