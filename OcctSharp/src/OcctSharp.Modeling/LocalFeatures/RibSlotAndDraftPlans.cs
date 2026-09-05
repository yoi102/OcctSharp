using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591
public readonly record struct RibAngularLimit(double First, double Last);
public sealed record RibSlotOptions
{
    public bool Revolution { get; init; }
    public bool AddMaterial { get; init; } = true;
    public bool Sliding { get; init; } = true;
    public GpXyz PlaneOrigin { get; init; } = new(0, 0, 0);
    public GpXyz PlaneNormal { get; init; } = new(0, 1, 0);
    public GpXyz ThicknessDirection1 { get; init; } = new(0, .2, 0);
    public GpXyz ThicknessDirection2 { get; init; } = new(0, -.2, 0);
    public GpXyz AxisOrigin { get; init; } = new(0, 0, 0);
    public GpXyz AxisDirection { get; init; } = new(0, 0, 1);
    /// <summary>Linear thickness on the first side for MakeRevolutionForm, not an angle.</summary>
    public double Thickness1 { get; init; } = .2;
    public double Thickness2 { get; init; } = .2;
    /// <summary>Optional separate clipping of the native result's difference material. Radians from plane-normal cross axis.</summary>
    public RibAngularLimit? AngularLimit { get; init; }
}
public sealed class RibSlotPlan : IDisposable
{
    private readonly Shape[] inputs;
    private readonly SlidingPairRaw[] sliding;
    private bool disposed;
    public Guid Id { get; } = Guid.NewGuid();
    public RibSlotOptions Options { get; }
    private RibSlotPlan(Shape[] inputs, SlidingPairRaw[] sliding, RibSlotOptions options)
    { this.inputs = inputs; this.sliding = sliding; Options = options; }
    public static RibSlotPlan Create(Shape basis, Shape wire, RibSlotOptions options, IEnumerable<LocalSlidingConstraint>? sliding = null)
    {
        ArgumentNullException.ThrowIfNull(options); List<Shape> source = [basis, wire];
        var slides = ScalarLawDefinition.Copy(sliding ?? [], 256);
        var pairs = slides.Select(s => { ArgumentNullException.ThrowIfNull(s); int edge = source.Count; source.Add(s.ProfileEdge); source.Add(s.BaseFace);
            return new SlidingPairRaw { EdgeInput = edge, FaceInput = edge + 1 }; }).ToArray();
        return new(AuthoringBridge.CopyInputs(source), pairs, options);
    }
    public unsafe LocalFeatureResult Build()
    {
        ObjectDisposedException.ThrowIf(disposed, this); var o = Options;
        RibSlotOptionsRaw raw = new() { Revolution = o.Revolution ? 1 : 0, Fuse = o.AddMaterial ? 1 : 0, Sliding = o.Sliding ? 1 : 0,
            AngularLimit = o.AngularLimit.HasValue ? 1 : 0, PlaneOrigin = LocalFeatureBridge.Raw(o.PlaneOrigin), PlaneNormal = LocalFeatureBridge.Raw(o.PlaneNormal),
            Direction1 = LocalFeatureBridge.Raw(o.ThicknessDirection1), Direction2 = LocalFeatureBridge.Raw(o.ThicknessDirection2),
            AxisOrigin = LocalFeatureBridge.Raw(o.AxisOrigin), AxisDirection = LocalFeatureBridge.Raw(o.AxisDirection),
            Thickness1 = o.Thickness1, Thickness2 = o.Thickness2, AngleFirst = o.AngularLimit?.First ?? 0, AngleLast = o.AngularLimit?.Last ?? 0 };
        return AuthoringBridge.WithInputs(inputs, (p, count) =>
        {
            fixed (SlidingPairRaw* s = sliding)
            {
                NativeError.ThrowIfFailed(NativeMethods.RibSlot(p, count, s, sliding.Length, in raw, out nint result), "rib_slot");
                return LocalFeatureBridge.Read(Id, result);
            }
        });
    }
    public void Dispose() { if (disposed) return; disposed = true; foreach (var shape in inputs) shape.Dispose(); }
}

public enum ShellDraftLimit { Length, UnderlyingSurface, Shape }
public enum ShellDraftTransition { RightCorner = 1, RoundCorner = 2 }
/// <summary>Native shell-draft policy. In OCCT 8.0.1, limit-driven modes accept
/// only a single analytic line/circle boundary edge; cornered and multi-edge
/// limit profiles are rejected before the SDK's unsafe internal history path.
/// Length-only mode also supports eligible cornered profiles.</summary>
public sealed record ShellDraftOptions
{
    public ShellDraftLimit Limit { get; init; }
    public ShellDraftTransition Transition { get; init; } = ShellDraftTransition.RightCorner;
    /// <summary>Passed to the native limit operation. For UnderlyingSurface this
    /// means KeepInsideSurface; for Shape the OCCT flag means KeepOutSide.</summary>
    public bool KeepInside { get; init; } = true;
    public bool InternalDraft { get; init; }
    public double Angle { get; init; } = .1;
    /// <summary>Extent along the drafted generatrix, not its projection on Direction.
    /// For a perpendicular planar profile the projected height is Length * cos(Angle).</summary>
    public double Length { get; init; } = 1;
    public double MinimumTransitionAngle { get; init; } = .01;
    public double MaximumTransitionAngle { get; init; } = 3;
    public GpXyz Direction { get; init; } = new(0, 0, 1);
}
public sealed class ShellDraftPlan : IDisposable
{
    private readonly Shape[] inputs;
    private bool disposed;
    public Guid Id { get; } = Guid.NewGuid();
    public ShellDraftOptions Options { get; }
    private ShellDraftPlan(Shape[] inputs, ShellDraftOptions options) { this.inputs = inputs; Options = options; }
    public static ShellDraftPlan Create(Shape profile, ShellDraftOptions options, Shape? stop = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (!Enum.IsDefined(options.Limit) || !Enum.IsDefined(options.Transition)) throw new ArgumentOutOfRangeException(nameof(options));
        if ((options.Limit != ShellDraftLimit.Length) != (stop is not null)) throw new ArgumentException("The shell draft requires exactly its selected stop input.");
        return new(AuthoringBridge.CopyInputs(stop is null ? [profile] : [profile, stop]), options);
    }
    public unsafe LocalFeatureResult Build()
    {
        ObjectDisposedException.ThrowIf(disposed, this); var o = Options;
        ShellDraftOptionsRaw raw = new() { LimitKind = (int)o.Limit, Transition = (int)o.Transition, Keep = o.KeepInside ? 1 : 0,
            InternalDraft = o.InternalDraft ? 1 : 0, Angle = o.Angle, Length = o.Length,
            AngleMinimum = o.MinimumTransitionAngle, AngleMaximum = o.MaximumTransitionAngle, Direction = LocalFeatureBridge.Raw(o.Direction) };
        return AuthoringBridge.WithInputs(inputs, (p, count) =>
        {
            NativeError.ThrowIfFailed(NativeMethods.ShellDraft(p, count, in raw, out nint result), "shell_draft");
            return LocalFeatureBridge.Read(Id, result);
        });
    }
    public void Dispose() { if (disposed) return; disposed = true; foreach (var input in inputs) input.Dispose(); }
}
public enum LocalHoleMode { ThroughAll, BetweenBounds, ThroughNext, UntilEnd, Blind }
public sealed record LocalHoleOptions(LocalHoleMode Mode, GpXyz Origin, GpXyz Direction, double Radius, double First = 0, double Last = 0);
public static class LocalFeatures
{
    public static LocalFeatureResult Hole(RepairSnapshot source, LocalHoleOptions options)
    {
        ArgumentNullException.ThrowIfNull(source); source.ThrowIfDisposed(); ArgumentNullException.ThrowIfNull(options);
        if (!Enum.IsDefined(options.Mode)) throw new ArgumentOutOfRangeException(nameof(options));
        LocalHoleOptionsRaw raw = new() { Mode = (int)options.Mode, Origin = LocalFeatureBridge.Raw(options.Origin),
            Direction = LocalFeatureBridge.Raw(options.Direction), Radius = options.Radius, First = options.First, Last = options.Last };
        NativeError.ThrowIfFailed(NativeMethods.LocalHole(source.Shape.Handle, in raw, out nint result), "local_hole");
        return LocalFeatureBridge.Read(Guid.NewGuid(), result, source.Identity, source.Fingerprint);
    }
}
#pragma warning restore CS1591
