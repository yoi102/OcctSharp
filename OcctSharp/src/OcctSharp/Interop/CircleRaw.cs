using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct CircleRaw
{
    internal CircleRaw(XyzRaw center, XyzRaw normal, double radius) { Center = center; Normal = normal; Radius = radius; }
    internal readonly XyzRaw Center;
    internal readonly XyzRaw Normal;
    internal readonly double Radius;
}
