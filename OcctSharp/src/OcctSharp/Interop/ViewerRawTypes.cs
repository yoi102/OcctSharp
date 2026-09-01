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

[StructLayout(LayoutKind.Sequential)]
internal readonly struct ViewerManipulatorStateRaw
{
    internal readonly int Attached;
    internal readonly int ActiveMode;
    internal readonly int ActiveAxis;
    internal readonly int HasActiveTransformation;
    internal readonly int ActivationOnDetection;
    internal readonly int ZoomPersistence;
    internal readonly int Skin;
    internal readonly int Reserved;
    internal readonly double Size;
    internal readonly double Gap;
    internal readonly Ax2Raw Position;
}
