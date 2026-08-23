namespace OcctSharp;

/// <summary>A caller-owned minimum-distance value copied from OCCT.</summary>
public readonly record struct ShapeDistanceResult(
    double Distance,
    GpPoint PointOnFirst,
    GpPoint PointOnSecond,
    int SolutionCount);
