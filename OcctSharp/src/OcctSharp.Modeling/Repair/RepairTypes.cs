namespace OcctSharp;

#pragma warning disable CS1591

public readonly record struct RepairIdentity(Guid SnapshotId, long Revision);
public readonly record struct RepairSelection(RepairIdentity Source, int Index);
public enum RepairControl { Auto = -1, Off = 0, On = 1 }
public enum RepairFindingKind
{
    Validation, ToleranceOutlier, DisconnectedWire, WireOrdering, EndpointGap, DegenerateEdge,
    WireIntersection, ShellOrientation, DisconnectedShell, SmallAreaFace, StripFace, SingularFace,
    WireGap3d, WireGap2d, SewingFreeEdge, SewingMultipleEdge, SewingContiguousEdge, NotApplicable, RemovedHole
}
public enum RepairRelationKind { Unchanged, Modified, Generated, Deleted, Unknown }
public enum RepairStageState { Completed, Failed, Skipped }
public enum RepairCheckState { Passed, Failed, Unavailable, NotRequired }
public sealed record RepairTopologyItem(RepairSelection Selection, ShapeKind Kind, int Orientation,
    int? ParentIndex, double? Tolerance);
/// <summary>
/// Status is the BRepCheck value for Validation; otherwise 0=clean, 1=finding, 2=reversal, 3=unavailable.
/// WireGap3d values use the declared length unit. WireGap2d values use UV units; Limit=0 means
/// no scalar limit is exposed (OCCT derives its check from surface resolution and linear precision).
/// Status=3 never supplies a measured residual, regardless of the numeric Value placeholder.
/// </summary>
public sealed record RepairFinding(RepairFindingKind Kind, RepairSelection? Source, RepairSelection? Related,
    int Status, double Value, double Limit);
public sealed record RepairMetrics(bool IsValid, int TopologyCount, double MaximumTolerance,
    double? Area, double? Volume, double MaximumEndpointGap);
public sealed record RepairToleranceDistribution(ShapeKind Kind, int Count, double Minimum, double Maximum,
    double Mean, IReadOnlyList<RepairSelection> Outliers);
public sealed record RepairInspectionOptions(double Tolerance = 1e-7, double SmallLength = 1e-4,
    double SmallArea = 1e-8, double ToleranceOutlier = 1e-3);
public sealed record RepairTolerancePolicy(double Minimum = 1e-7, double Maximum = 1e-3);
public sealed record RepairBudget(double? MaximumTolerance = 1e-3, double? MaximumToleranceGrowth = null,
    double? MaximumRelativeAreaChange = null, double? MaximumRelativeVolumeChange = null, bool RequireValid = true);
public sealed record RepairBudgetCheck(string Name, RepairCheckState State, double? Measured, double? Limit);
public sealed record RepairHistoryRelation(RepairSelection Source, RepairSelection? Result, RepairRelationKind Kind);
public sealed record RepairStageOutcome(int Index, string Name, RepairStageState State, string Message,
    RepairMetrics? Metrics, IReadOnlyList<RepairFinding> Findings);

/// <summary>Owns an extracted wire independently of its source; provenance is copied.</summary>
public sealed class RepairFreeBoundary : IDisposable
{
    internal RepairFreeBoundary(Shape wire, bool closed, double length, double? area, double? gap, IReadOnlyList<RepairSelection> edges) =>
        (Wire, IsClosed, Length, PlanarArea, EndpointGap, SourceEdges) = (wire, closed, length, area, gap, edges);
    public Shape Wire { get; }
    public bool IsClosed { get; }
    public double Length { get; }
    public double? PlanarArea { get; }
    public double? EndpointGap { get; }
    public IReadOnlyList<RepairSelection> SourceEdges { get; }
    public void Dispose() => Wire.Dispose();
}
