using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct ExtremaSolutionRaw
{
    internal readonly double Distance;
    internal readonly XyzRaw PointOnFirst;
    internal readonly XyzRaw PointOnSecond;
    internal readonly int FirstSupportKind;
    internal readonly int SecondSupportKind;
    internal readonly int HasFirstEdgeParameter;
    internal readonly double FirstEdgeParameter;
    internal readonly int HasSecondEdgeParameter;
    internal readonly double SecondEdgeParameter;
    internal readonly int HasFirstFaceParameters;
    internal readonly double FirstFaceU;
    internal readonly double FirstFaceV;
    internal readonly int HasSecondFaceParameters;
    internal readonly double SecondFaceU;
    internal readonly double SecondFaceV;
    internal readonly int IsInnerSolution;
    internal readonly nint FirstSupport;
    internal readonly nint SecondSupport;
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct InspectionPropertiesRaw
{
    internal readonly double Mass;
    internal readonly XyzRaw Center;
    internal readonly double I11;
    internal readonly double I12;
    internal readonly double I13;
    internal readonly double I21;
    internal readonly double I22;
    internal readonly double I23;
    internal readonly double I31;
    internal readonly double I32;
    internal readonly double I33;
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct RadialMeasurementRaw
{
    internal readonly int GeometryKind;
    internal readonly double Radius;
    internal readonly double Diameter;
    internal readonly double SemiAngle;
}
