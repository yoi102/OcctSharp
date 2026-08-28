using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct EdgeCurveSnapshotRaw
{
    internal readonly int CurveType;
    internal readonly double FirstParameter;
    internal readonly double LastParameter;
    internal readonly XyzRaw StartPoint;
    internal readonly XyzRaw EndPoint;
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct FaceSurfaceSnapshotRaw
{
    internal readonly int SurfaceType;
    internal readonly double FirstUParameter;
    internal readonly double LastUParameter;
    internal readonly double FirstVParameter;
    internal readonly double LastVParameter;
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct CurveEvaluationRaw
{
    internal readonly double Parameter;
    internal readonly XyzRaw Point;
    internal readonly XyzRaw Tangent;
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct CurveDerivativeEvaluationRaw
{
    internal readonly double Parameter;
    internal readonly XyzRaw Point;
    internal readonly XyzRaw FirstDerivative;
    internal readonly XyzRaw SecondDerivative;
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct XyRaw
{
    internal readonly double X;
    internal readonly double Y;
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct PcurveSnapshotRaw
{
    internal readonly double FirstParameter;
    internal readonly double LastParameter;
    internal readonly XyRaw StartPoint;
    internal readonly XyRaw EndPoint;
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct PcurveEvaluationRaw
{
    internal readonly double Parameter;
    internal readonly XyRaw Point;
    internal readonly XyRaw Tangent;
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct CurveProjectionRaw
{
    internal readonly double Parameter;
    internal readonly XyzRaw Point;
    internal readonly double Distance;
    internal readonly int SolutionCount;
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct SurfaceEvaluationRaw
{
    internal readonly double UParameter;
    internal readonly double VParameter;
    internal readonly XyzRaw Point;
    internal readonly XyzRaw Normal;
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct SurfaceDerivativeEvaluationRaw
{
    internal readonly double UParameter;
    internal readonly double VParameter;
    internal readonly XyzRaw Point;
    internal readonly XyzRaw UDerivative;
    internal readonly XyzRaw VDerivative;
    internal readonly XyzRaw Normal;
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct SurfaceProjectionRaw
{
    internal readonly double UParameter;
    internal readonly double VParameter;
    internal readonly XyzRaw Point;
    internal readonly double Distance;
    internal readonly int SolutionCount;
}
