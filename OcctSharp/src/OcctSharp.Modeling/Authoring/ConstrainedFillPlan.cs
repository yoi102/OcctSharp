using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591
public enum SurfaceConstraintContinuity { G0, G1, G2 }
public enum SurfaceConstraintKind { Edge, SurfaceUvPoint, Point }
public sealed record SurfaceConstraintDefinition(string Id, SurfaceConstraintKind Kind, int? ShapeInputIndex,
    int? SupportInputIndex, SurfaceConstraintContinuity Continuity, bool Boundary, bool Required, double U, double V, GpPoint Point);
public abstract record SurfaceConstraint(string Id, SurfaceConstraintContinuity Continuity, bool Required = true);
public sealed record SurfaceEdgeConstraint(string Id, Shape Edge, SurfaceConstraintContinuity Continuity = SurfaceConstraintContinuity.G0,
    bool Boundary = true, Shape? SupportFace = null, bool Required = true) : SurfaceConstraint(Id, Continuity, Required);
public sealed record SurfaceUvConstraint(string Id, Shape SupportFace, double U, double V,
    SurfaceConstraintContinuity Continuity = SurfaceConstraintContinuity.G0, bool Required = true) : SurfaceConstraint(Id, Continuity, Required);
public sealed record SurfacePointConstraint(string Id, GpPoint Point, bool Required = true) : SurfaceConstraint(Id, SurfaceConstraintContinuity.G0, Required);
public sealed record ConstrainedFillOptions
{
    public int EnergyDegree { get; init; } = 3;
    public int PointsPerCurve { get; init; } = 15;
    public int Iterations { get; init; } = 2;
    public bool Anisotropic { get; init; }
    public int MaximumDegree { get; init; } = 8;
    public int MaximumSegments { get; init; } = 16;
    public int VerificationSamples { get; init; } = 33;
    public double Tolerance2d { get; init; } = 1e-5;
    public double Tolerance3d { get; init; } = 1e-4;
    public double ToleranceAngular { get; init; } = 1e-2;
    public double ToleranceCurvature { get; init; } = 0.1;
    internal void Validate(int count)
    {
        if (EnergyDegree is < 2 or > 8 || PointsPerCurve is < 5 or > 200 || Iterations is < 1 or > 8
            || MaximumDegree is < 2 or > 25 || MaximumSegments is < 1 or > 128 || VerificationSamples is < 3 or > 257
            || (long)count * PointsPerCurve * (1 << Iterations) > 1000000)
            throw new ArgumentException("The solver controls exceed the bounded resource policy.");
        AuthoringBridge.Positive(Tolerance2d, nameof(Tolerance2d)); AuthoringBridge.Positive(Tolerance3d, nameof(Tolerance3d));
        AuthoringBridge.Positive(ToleranceAngular, nameof(ToleranceAngular)); AuthoringBridge.Positive(ToleranceCurvature, nameof(ToleranceCurvature));
    }
}
public sealed record ConstraintFulfilment(string Id, int KernelIndex, bool Required, bool Accepted,
    double? PositionResidual, double? AngularResidual, double? CurvatureResidual, int SampleCount);
public sealed class ConstrainedFillResult : IDisposable
{
    internal ConstrainedFillResult(AuthoringResult result, ConstraintFulfilment[] constraints)
    { Result = result; Constraints = Array.AsReadOnly(constraints); }
    public AuthoringResult Result { get; }
    public IReadOnlyList<ConstraintFulfilment> Constraints { get; }
    public bool Accepted => Result.Diagnostics.AlgorithmDone && Result.Diagnostics.ShapeIsValid && Constraints.Where(c => c.Required).All(c => c.Accepted);
    public Shape RequireFace()
    {
        if (!Accepted) throw new InvalidOperationException("Required surface constraints did not pass final-surface residual acceptance.");
        return Result.RequireShape();
    }
    public void Dispose() => Result.Dispose();
}
/// <summary>Owns a copied edge/support/seed dependency graph and immutable per-constraint controls.</summary>
public sealed class ConstrainedFillPlan : IDisposable
{
    private readonly Shape[] inputs;
    private readonly FillConstraintRaw[] constraints;
    private readonly string[] ids;
    private readonly int seedIndex;
    private bool disposed;
    public Guid Id { get; } = Guid.NewGuid();
    public ConstrainedFillOptions Options { get; }
    public IReadOnlyList<string> ConstraintIds => Array.AsReadOnly(ids);
    public int InputCount => inputs.Length;
    public int? InitialSurfaceInputIndex => seedIndex >= 0 ? seedIndex : null;
    public IReadOnlyList<SurfaceConstraintDefinition> Constraints => Array.AsReadOnly(constraints.Select(c => new SurfaceConstraintDefinition(
        ids[c.Id], (SurfaceConstraintKind)c.Kind, c.ShapeIndex >= 0 ? c.ShapeIndex : null, c.SupportIndex >= 0 ? c.SupportIndex : null,
        (SurfaceConstraintContinuity)c.Order, c.Boundary != 0, c.Required != 0, c.U, c.V, new(c.Point.X, c.Point.Y, c.Point.Z))).ToArray());
    private ConstrainedFillPlan(Shape[] inputs, FillConstraintRaw[] constraints, string[] ids, int seed, ConstrainedFillOptions options)
    { this.inputs = inputs; this.constraints = constraints; this.ids = ids; seedIndex = seed; Options = options; }
    public static ConstrainedFillPlan Create(IEnumerable<SurfaceConstraint> constraints, ConstrainedFillOptions? options = null, Shape? initialSurface = null)
    {
        SurfaceConstraint[] copied = ScalarLawDefinition.Copy(constraints, 256); options ??= new(); options.Validate(copied.Length);
        if (copied.OfType<SurfaceEdgeConstraint>().Count(c => c.Boundary) < 2) throw new ArgumentException("At least two boundary edges are required.");
        List<Shape> sources = []; HashSet<string> ids = new(StringComparer.Ordinal); List<FillConstraintRaw> raw = [];
        int Add(Shape? shape) { if (shape is null) return -1; int index = sources.Count; sources.Add(shape); return index; }
        foreach (var constraint in copied)
        {
            ArgumentNullException.ThrowIfNull(constraint); ArgumentException.ThrowIfNullOrWhiteSpace(constraint.Id);
            if (constraint.Id.Length > 1024 || !ids.Add(constraint.Id) || !Enum.IsDefined(constraint.Continuity)) throw new ArgumentException("Constraint IDs must be unique and continuity valid.");
            FillConstraintRaw c = new() { Id = raw.Count, ShapeIndex = -1, SupportIndex = -1,
                Order = (int)constraint.Continuity, Required = constraint.Required ? 1 : 0 };
            switch (constraint)
            {
                case SurfaceEdgeConstraint edge:
                    ArgumentNullException.ThrowIfNull(edge.Edge); c.Kind = 0; c.ShapeIndex = Add(edge.Edge);
                    c.SupportIndex = Add(edge.SupportFace); c.Boundary = edge.Boundary ? 1 : 0;
                    if (edge.Continuity != SurfaceConstraintContinuity.G0 && edge.SupportFace is null) throw new ArgumentException("G1/G2 edges need an explicit support face.");
                    break;
                case SurfaceUvConstraint uv:
                    ArgumentNullException.ThrowIfNull(uv.SupportFace); c.Kind = 1; c.SupportIndex = Add(uv.SupportFace); c.U = uv.U; c.V = uv.V;
                    if (!double.IsFinite(uv.U) || !double.IsFinite(uv.V)) throw new ArgumentOutOfRangeException(nameof(constraints));
                    break;
                case SurfacePointConstraint point:
                    c.Kind = 2; c.Point = new(point.Point.X, point.Point.Y, point.Point.Z);
                    if (!double.IsFinite(point.Point.X) || !double.IsFinite(point.Point.Y) || !double.IsFinite(point.Point.Z)) throw new ArgumentOutOfRangeException(nameof(constraints));
                    break;
                default: throw new ArgumentException("Unknown surface constraint kind.");
            }
            raw.Add(c);
        }
        int seed = Add(initialSurface);
        return new(AuthoringBridge.CopyInputs(sources), raw.ToArray(), copied.Select(c => c.Id).ToArray(), seed, options);
    }
    public unsafe ConstrainedFillResult Build()
    {
        ObjectDisposedException.ThrowIf(disposed, this); var o = Options;
        FillOptionsRaw options = new() { Degree = o.EnergyDegree, PointsPerCurve = o.PointsPerCurve, Iterations = o.Iterations, Anisotropic = o.Anisotropic ? 1 : 0,
            MaximumDegree = o.MaximumDegree, MaximumSegments = o.MaximumSegments, SeedIndex = seedIndex, VerificationSamples = o.VerificationSamples,
            Tolerance2d = o.Tolerance2d, Tolerance3d = o.Tolerance3d, ToleranceAngular = o.ToleranceAngular, ToleranceCurvature = o.ToleranceCurvature };
        return AuthoringBridge.WithInputs<ConstrainedFillResult>(inputs, (p, count) =>
        {
            ConstraintResidualRaw[] residuals = new ConstraintResidualRaw[constraints.Length];
            fixed (FillConstraintRaw* c = constraints) fixed (ConstraintResidualRaw* r = residuals)
            {
                NativeError.ThrowIfFailed(NativeMethods.ConstrainedFill(p, count, c, constraints.Length, in options, r, residuals.Length,
                    out AuthoringInfoRaw info, out nint raw), "constrained_fill");
                AuthoringResult result = AuthoringBridge.Read(Id, raw, info);
                try { return new(result, residuals.Select(v => new ConstraintFulfilment(ids[v.Id], v.KernelIndex, v.Required != 0, v.Accepted != 0,
                    (v.Defined & 1) != 0 ? v.Position : null, (v.Defined & 2) != 0 ? v.Angle : null, (v.Defined & 4) != 0 ? v.Curvature : null, v.SampleCount)).ToArray()); }
                catch { result.Dispose(); throw; }
            }
        });
    }
    public Shape CopyInput(int index)
    {
        ObjectDisposedException.ThrowIf(disposed, this); if ((uint)index >= inputs.Length) throw new ArgumentOutOfRangeException(nameof(index));
        return AuthoringBridge.CopyInputs([inputs[index]])[0];
    }
    public void Dispose() { if (disposed) return; disposed = true; foreach (Shape input in inputs) input.Dispose(); }
}
#pragma warning restore CS1591
