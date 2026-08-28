namespace OcctSharp;

/// <summary>OCCT BRepCheck diagnostic status copied without retaining analyzer state.</summary>
public enum ShapeValidationStatus
{
    /// <summary>No validation error was reported.</summary>
    NoError = 0,
    /// <summary>A point is inconsistent with its supporting curve.</summary>
    InvalidPointOnCurve,
    /// <summary>A point is inconsistent with a curve represented on a surface.</summary>
    InvalidPointOnCurveOnSurface,
    /// <summary>A point is inconsistent with its supporting surface.</summary>
    InvalidPointOnSurface,
    /// <summary>The edge has no required three-dimensional curve.</summary>
    No3dCurve,
    /// <summary>The edge has multiple incompatible three-dimensional curves.</summary>
    Multiple3dCurve,
    /// <summary>The edge's three-dimensional curve is invalid.</summary>
    Invalid3dCurve,
    /// <summary>The edge has no required curve on a surface.</summary>
    NoCurveOnSurface,
    /// <summary>A curve on a surface is invalid.</summary>
    InvalidCurveOnSurface,
    /// <summary>A curve on a closed surface is invalid.</summary>
    InvalidCurveOnClosedSurface,
    /// <summary>The same-range flag is inconsistent with the edge geometry.</summary>
    InvalidSameRangeFlag,
    /// <summary>The same-parameter flag is inconsistent with the edge geometry.</summary>
    InvalidSameParameterFlag,
    /// <summary>The degenerated flag is inconsistent with the edge geometry.</summary>
    InvalidDegeneratedFlag,
    /// <summary>An edge is not connected to a face where one is required.</summary>
    FreeEdge,
    /// <summary>An edge has invalid multiple connectivity.</summary>
    InvalidMultiConnexity,
    /// <summary>A curve or parameter range is invalid.</summary>
    InvalidRange,
    /// <summary>A wire contains no edges.</summary>
    EmptyWire,
    /// <summary>A wire contains a redundant edge.</summary>
    RedundantEdge,
    /// <summary>A wire intersects itself.</summary>
    SelfIntersectingWire,
    /// <summary>A face has no supporting surface.</summary>
    NoSurface,
    /// <summary>A face contains an invalid wire.</summary>
    InvalidWire,
    /// <summary>A face contains a redundant wire.</summary>
    RedundantWire,
    /// <summary>Wires within a face intersect.</summary>
    IntersectingWires,
    /// <summary>Wire nesting within a face is invalid.</summary>
    InvalidImbricationOfWires,
    /// <summary>A shell contains no faces.</summary>
    EmptyShell,
    /// <summary>A shell contains a redundant face.</summary>
    RedundantFace,
    /// <summary>Shell nesting within a solid is invalid.</summary>
    InvalidImbricationOfShells,
    /// <summary>The shape cannot be oriented consistently.</summary>
    UnorientableShape,
    /// <summary>The shape is not topologically closed.</summary>
    NotClosed,
    /// <summary>The shape is not topologically connected.</summary>
    NotConnected,
    /// <summary>A referenced subshape is not contained in its parent shape.</summary>
    SubshapeNotInShape,
    /// <summary>The shape orientation is invalid.</summary>
    BadOrientation,
    /// <summary>A subshape orientation is invalid relative to its parent.</summary>
    BadOrientationOfSubshape,
    /// <summary>A polygonal representation is inconsistent with its triangulation.</summary>
    InvalidPolygonOnTriangulation,
    /// <summary>A topology tolerance value is invalid.</summary>
    InvalidToleranceValue,
    /// <summary>The shape contains an invalid enclosed region.</summary>
    EnclosedRegion,
    /// <summary>OCCT could not complete the requested validation check.</summary>
    CheckFail
}

/// <summary>One copied diagnostic associated with a unique subshape kind.</summary>
public readonly record struct ShapeValidationIssue(ShapeKind ShapeKind, ShapeValidationStatus Status);

/// <summary>Caller-owned detailed BRepCheck snapshot.</summary>
public sealed record ShapeValidationReport(bool IsValid, IReadOnlyList<ShapeValidationIssue> Issues)
{
    /// <summary>Gets the number of copied validation issues.</summary>
    public int IssueCount => Issues.Count;
}

/// <summary>Owns a repaired shape and copied validation snapshots from before and after repair.</summary>
public sealed class ShapeRepairResult : IDisposable
{
    internal ShapeRepairResult(Shape shape, ShapeValidationReport before, ShapeValidationReport after) =>
        (Shape, Before, After) = (shape, before, after);

    /// <summary>Gets the independently owned repaired shape.</summary>
    public Shape Shape { get; }
    /// <summary>Gets the validation snapshot captured before repair.</summary>
    public ShapeValidationReport Before { get; }
    /// <summary>Gets the validation snapshot captured after repair.</summary>
    public ShapeValidationReport After { get; }
    /// <summary>Releases the repaired shape.</summary>
    public void Dispose() => Shape.Dispose();
}
