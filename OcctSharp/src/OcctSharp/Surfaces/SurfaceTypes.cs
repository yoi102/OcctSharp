namespace OcctSharp;

#pragma warning disable CS1591

public enum SurfaceDomainState { Inside = 0, Outside = 1, Boundary = 2, Unknown = 3 }
public enum SurfaceIsoDirection { ConstantU = 0, ConstantV = 1 }
public enum AnalyticSurfaceKind { Plane = 0, Cylinder = 1, Sphere = 2, Cone = 3, Torus = 4 }
public enum SurfaceIntersectionKind { Point = 0, CoincidentInterval = 1 }

public sealed record SurfaceDescriptor(
    SurfaceGeometryType Kind, SurfaceParameterBounds Bounds, bool IsReversed,
    bool IsUClosed, bool IsVClosed, bool IsUPeriodic, bool IsVPeriodic,
    double UPeriod, double VPeriod);

/// <summary>Copied world-space surface derivatives; singular UV charts are explicitly marked.</summary>
public readonly record struct SurfaceEvaluationPoint(
    SketchPoint2d Uv, GpPoint Point, GpXyz UDerivative, GpXyz VDerivative,
    GpXyz? Normal, bool IsParameterSingular, SurfaceDomainState DomainState,
    double? MinimumCurvature, double? MaximumCurvature, double? MeanCurvature, double? GaussianCurvature);

public readonly record struct SurfacePointSolution(
    int SourceIndex, SketchPoint2d Uv, GpPoint Point, double Distance, SurfaceDomainState DomainState);

/// <summary>
/// Copied B-spline geometry; its parameterization may differ from the original pcurve.
/// Residual is the maximum input-point UV distance for fitting, the converter's UV error
/// for offset/conversion, or the world-space distance at 65 samples for derivation.
/// A sampled derivation residual is not a certified global error bound.
/// IsExactGeometry describes conversion of the resulting UV curve to B-spline; fitting
/// and derivation accuracy are reported separately by Residual.
/// </summary>
public sealed record SurfaceCurveDefinition(
    SketchCurve2d Curve, ParameterRange SourceParameters, bool IsExactGeometry,
    bool PreservesSourceParameterization, double Residual, int SeamBranch = 0);

public readonly record struct SurfaceCurveSample(double Parameter, SketchPoint2d Uv, GpPoint Point, GpXyz Tangent);
public readonly record struct SurfaceUvShift(SketchPoint2d Uv, double UPeriodShift, double VPeriodShift);
public readonly record struct SurfaceTracePoint(int SourceIndex, SketchPoint2d Uv, GpPoint Point, double Distance);

public sealed class SurfaceGrid
{
    internal SurfaceGrid(int uCount, int vCount, IEnumerable<SurfaceEvaluationPoint> samples)
        => (UCount, VCount, Samples) = (uCount, vCount, Array.AsReadOnly(samples.ToArray()));
    public int UCount { get; }
    public int VCount { get; }
    /// <summary>Row-major: index = vIndex * UCount + uIndex. Outside/hole samples are retained and marked.</summary>
    public IReadOnlyList<SurfaceEvaluationPoint> Samples { get; }
}

public sealed record SurfaceProjectionOptions
{
    public double Tolerance3d { get; init; } = 1e-6;
    public double Tolerance2d { get; init; } = 1e-8;
    /// <summary>A negative value disables the maximum-distance filter.</summary>
    public double MaximumDistance { get; init; } = -1;
    public bool LimitToFace { get; init; } = true;
    public int MaximumDegree { get; init; } = 14;
    public int MaximumSegments { get; init; } = 64;
    public FreeformContinuity Continuity { get; init; } = FreeformContinuity.C2;
}

public readonly record struct SurfaceRepairDiagnostics(
    bool WasValid, bool IsValid, int EdgeCountBefore, int EdgeCountAfter,
    int MissingCurveCountBefore, int MissingCurveCountAfter,
    int InconsistentEdgeCountBefore, int InconsistentEdgeCountAfter,
    double MaximumToleranceBefore, double MaximumToleranceAfter);

public sealed class SurfaceRepairResult : IDisposable
{
    internal SurfaceRepairResult(Shape shape, SurfaceRepairDiagnostics diagnostics)
        => (Shape, Diagnostics) = (shape, diagnostics);
    public Shape Shape { get; }
    public SurfaceRepairDiagnostics Diagnostics { get; }
    public void Dispose() => Shape.Dispose();
}

public readonly record struct SurfaceSplitDiagnostics(int ToolCount, int FaceCount, bool IsValid,
    double SourceArea, double ResultArea);

public sealed class SurfaceSplitResult : IDisposable
{
    internal SurfaceSplitResult(Shape shape, SurfaceSplitDiagnostics diagnostics)
        => (Shape, Diagnostics) = (shape, diagnostics);
    public Shape Shape { get; }
    public SurfaceSplitDiagnostics Diagnostics { get; }
    public void Dispose() => Shape.Dispose();
}

public readonly record struct SurfaceBoundarySegment(SurfaceCurveDefinition Definition, double Length, bool IsSeam, bool IsDegenerate = false);

/// <summary>A fully copied oriented loop. A null UV area indicates seam/nesting ambiguity.</summary>
public sealed class SurfaceBoundaryLoop
{
    internal SurfaceBoundaryLoop(int index, bool isOuter, IEnumerable<SurfaceBoundarySegment> segments, double? area)
    {
        Index = index; IsOuter = isOuter; Segments = Array.AsReadOnly(segments.ToArray()); SignedUvArea = area;
    }
    public int Index { get; }
    public bool IsOuter { get; }
    public IReadOnlyList<SurfaceBoundarySegment> Segments { get; }
    public double Length => Segments.Sum(segment => segment.Length);
    public int SeamOccurrenceCount => Segments.Count(segment => segment.IsSeam);
    public double? SignedUvArea { get; }
}

public readonly record struct SurfaceCurveIntersection(
    SurfaceIntersectionKind Kind, ParameterRange CurveParameters, GpPoint FirstPoint, GpPoint LastPoint,
    SketchPoint2d FirstUv, SketchPoint2d LastUv, SurfaceDomainState DomainState);
