namespace OcctSharp;

/// <summary>Copied counts for the eight OCCT topology kinds used by ordinary CAD models.</summary>
public readonly record struct TopologyCounts(
    int VertexCount,
    int EdgeCount,
    int WireCount,
    int FaceCount,
    int ShellCount,
    int SolidCount,
    int CompSolidCount,
    int CompoundCount);

/// <summary>Copied minimum and maximum OCCT tolerance values for one topology kind.</summary>
public readonly record struct ToleranceRange(double Minimum, double Maximum);

/// <summary>One caller-owned snapshot of whole-shape topology health and tolerances.</summary>
public sealed record ShapeTopologySummary(
    TopologyCounts UniqueCounts,
    TopologyCounts OccurrenceCounts,
    bool IsClosed,
    bool IsValid,
    ToleranceRange VertexTolerance,
    ToleranceRange EdgeTolerance,
    ToleranceRange FaceTolerance);
