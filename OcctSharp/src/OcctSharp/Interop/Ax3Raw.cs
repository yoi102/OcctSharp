using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct Ax3Raw
{
    internal Ax3Raw(XyzRaw origin, XyzRaw xDirection, XyzRaw yDirection, XyzRaw direction)
    {
        Origin = origin;
        XDirection = xDirection;
        YDirection = yDirection;
        Direction = direction;
    }

    internal readonly XyzRaw Origin;
    internal readonly XyzRaw XDirection;
    internal readonly XyzRaw YDirection;
    internal readonly XyzRaw Direction;
}
