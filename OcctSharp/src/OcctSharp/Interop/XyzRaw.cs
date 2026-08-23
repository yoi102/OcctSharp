using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct XyzRaw
{
    internal XyzRaw(double x, double y, double z) { X = x; Y = y; Z = z; }
    internal readonly double X;
    internal readonly double Y;
    internal readonly double Z;
}
