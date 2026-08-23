using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct MeshVertexRaw
{
    internal readonly double X;
    internal readonly double Y;
    internal readonly double Z;
    internal readonly double NormalX;
    internal readonly double NormalY;
    internal readonly double NormalZ;
}
