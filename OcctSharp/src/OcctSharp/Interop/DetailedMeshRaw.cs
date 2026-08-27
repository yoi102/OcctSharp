using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct DetailedMeshVertexRaw
{
    internal readonly double X;
    internal readonly double Y;
    internal readonly double Z;
    internal readonly double NormalX;
    internal readonly double NormalY;
    internal readonly double NormalZ;
    internal readonly double U;
    internal readonly double V;
    internal readonly int HasUv;
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct DetailedMeshTriangleRaw
{
    internal readonly int VertexA;
    internal readonly int VertexB;
    internal readonly int VertexC;
    internal readonly int FaceIndex;
    internal readonly int IsReversed;
}
