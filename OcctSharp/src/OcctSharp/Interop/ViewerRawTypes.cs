using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct ViewerCameraRaw
{
    internal ViewerCameraRaw(XyzRaw eye, XyzRaw target, XyzRaw up, XyzRaw projection)
    {
        Eye = eye;
        Target = target;
        Up = up;
        Projection = projection;
    }

    internal readonly XyzRaw Eye;
    internal readonly XyzRaw Target;
    internal readonly XyzRaw Up;
    internal readonly XyzRaw Projection;
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct ViewerPickRayRaw
{
    internal readonly XyzRaw Origin;
    internal readonly XyzRaw Direction;
}
