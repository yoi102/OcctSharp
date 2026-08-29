using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct PmiDimensionRaw(
    int Type,
    int HasQualifier,
    int Qualifier,
    int HasAngularQualifier,
    int AngularQualifier,
    int HasClassOfTolerance,
    int IsHole,
    int FormVariance,
    int Grade,
    int LeftDecimalPlaces,
    int RightDecimalPlaces,
    int HasDirection,
    XyzRaw Direction,
    int HasPlane,
    Ax2Raw Plane,
    int HasFirstPoint,
    XyzRaw FirstPoint,
    int HasSecondPoint,
    XyzRaw SecondPoint,
    int HasTextPoint,
    XyzRaw TextPoint);

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct PmiToleranceRaw(
    int Type,
    int TypeOfValue,
    double Value,
    int MaterialRequirement,
    int ZoneModifier,
    double ZoneModifierValue,
    double MaximumValueModifier,
    int HasAxis,
    Ax2Raw Axis,
    int HasPlane,
    Ax2Raw Plane,
    int HasPoint,
    XyzRaw Point,
    int HasTextPoint,
    XyzRaw TextPoint,
    int AffectedPlaneType,
    PlaneRaw AffectedPlane);

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct PmiDatumRaw(
    int Position,
    int IsDatumTarget,
    int TargetType,
    double TargetLength,
    double TargetWidth,
    int TargetNumber,
    int HasTargetAxis,
    Ax2Raw TargetAxis,
    int HasPlane,
    Ax2Raw Plane,
    int HasPoint,
    XyzRaw Point,
    int HasTextPoint,
    XyzRaw TextPoint,
    int HasModifierWithValue,
    int ModifierWithValue,
    double ModifierValue);

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct SavedViewRaw(
    int ProjectionType,
    XyzRaw ProjectionPoint,
    XyzRaw ViewDirection,
    XyzRaw UpDirection,
    double ZoomFactor,
    double WindowHorizontalSize,
    double WindowVerticalSize,
    int HasFrontClipping,
    double FrontClippingDistance,
    int HasBackClipping,
    double BackClippingDistance,
    int HasViewVolumeSidesClipping);

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct PlaneEquationRaw(double A, double B, double C, double D, int Capping);
