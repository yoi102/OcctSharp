using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591
public enum LimitedFeatureKind { Prism, DraftedPrism, Revolved, Pipe }
public enum LocalFeatureLimit { Extent, Until, FromUntil, UntilEnd, FromEnd, ThroughAll, UntilAndExtent }
public sealed record LocalSlidingConstraint(Shape ProfileEdge, Shape BaseFace);
public sealed record LimitedFeatureOptions
{
    public LimitedFeatureKind Kind { get; init; }
    public LocalFeatureLimit Limit { get; init; }
    public bool AddMaterial { get; init; } = true;
    public bool Modify { get; init; } = true;
    /// <summary>Length for prisms, radians for revolution, unused for a complete pipe spine.</summary>
    public double Extent { get; init; } = 1;
    /// <summary>Drafted-prism angle in radians. UntilEnd uses the SDK's box-derived slanted length,
    /// which can be too short axially on a cube; such kernel failures remain diagnostics.</summary>
    public double DraftAngle { get; init; }
    public GpXyz AxisOrigin { get; init; } = new(0, 0, 0);
    /// <summary>Ordinary prism direction or revolution axis. Drafted prisms derive their direction
    /// from the profile geometry; pipe direction follows the spine.</summary>
    public GpXyz Direction { get; init; } = new(0, 0, 1);
}

/// <summary>Owns one copied base/profile/support/limit/sliding graph; each execution uses another private copy.
/// Planar limits may use unbounded support surfaces. Revolved limits follow native base-side selection,
/// not necessarily the first positive angle. Limited pipes require eligible bounded spine curves;
/// OCCT 8.0.1 can fail converting untrimmed line curves. Failed builders are never replaced by Booleans.</summary>
public sealed class LimitedFeaturePlan : IDisposable
{
    private readonly Shape[] inputs;
    private readonly SlidingPairRaw[] sliding;
    private readonly LimitedFeatureOptionsRaw raw;
    private bool disposed;
    public Guid Id { get; } = Guid.NewGuid();
    public LimitedFeatureOptions Options { get; }
    public int InputCount => inputs.Length;
    private LimitedFeaturePlan(Shape[] inputs, SlidingPairRaw[] sliding, LimitedFeatureOptionsRaw raw, LimitedFeatureOptions options)
    { this.inputs = inputs; this.sliding = sliding; this.raw = raw; Options = options; }
    public static LimitedFeaturePlan Create(Shape basis, Shape profile, Shape supportFace, LimitedFeatureOptions options,
        Shape? from = null, Shape? until = null, Shape? spine = null, IEnumerable<LocalSlidingConstraint>? sliding = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (!Enum.IsDefined(options.Kind) || !Enum.IsDefined(options.Limit)) throw new ArgumentOutOfRangeException(nameof(options));
        bool needsUntil = options.Limit is LocalFeatureLimit.Until or LocalFeatureLimit.FromUntil or LocalFeatureLimit.FromEnd or LocalFeatureLimit.UntilAndExtent;
        if (needsUntil != (until is not null) || (options.Limit == LocalFeatureLimit.FromUntil) != (from is not null))
            throw new ArgumentException("The selected mode requires exactly its From/Until inputs.");
        if ((options.Kind == LimitedFeatureKind.Pipe) != (spine is not null)) throw new ArgumentException("Only a pipe feature requires a spine.");
        if (options.Kind == LimitedFeatureKind.Pipe && options.Limit is not (LocalFeatureLimit.Extent or LocalFeatureLimit.Until or LocalFeatureLimit.FromUntil))
            throw new ArgumentException("Pipe features support complete-spine, Until and From/Until modes.");
        if (options.Kind == LimitedFeatureKind.Revolved && options.Limit is LocalFeatureLimit.UntilEnd or LocalFeatureLimit.FromEnd)
            throw new ArgumentException("Prism end modes are not revolved feature modes.");
        if (options.Limit is LocalFeatureLimit.Extent or LocalFeatureLimit.UntilAndExtent) AuthoringBridge.Positive(options.Extent, nameof(options.Extent));
        if (options.Kind == LimitedFeatureKind.Revolved && options.Limit is LocalFeatureLimit.Extent or LocalFeatureLimit.UntilAndExtent && options.Extent > 2 * Math.PI)
            throw new ArgumentOutOfRangeException(nameof(options), "Revolution extent cannot exceed one turn.");
        List<Shape> source = [basis, profile, supportFace];
        int Add(Shape? shape) { if (shape is null) return -1; int index = source.Count; source.Add(shape); return index; }
        LimitedFeatureOptionsRaw raw = new() { Operation = (int)options.Kind, LimitMode = (int)options.Limit,
            Fuse = options.AddMaterial ? 1 : 0, Modify = options.Modify ? 1 : 0, SupportInput = 2,
            FromInput = Add(from), UntilInput = Add(until), PathInput = Add(spine), Extent = options.Extent,
            DraftAngle = options.DraftAngle, Origin = LocalFeatureBridge.Raw(options.AxisOrigin), Direction = LocalFeatureBridge.Raw(options.Direction) };
        var slides = ScalarLawDefinition.Copy(sliding ?? [], 256);
        var pairs = slides.Select(s => { ArgumentNullException.ThrowIfNull(s); return new SlidingPairRaw { EdgeInput = Add(s.ProfileEdge), FaceInput = Add(s.BaseFace) }; }).ToArray();
        return new(AuthoringBridge.CopyInputs(source), pairs, raw, options);
    }
    public unsafe LocalFeatureResult Build()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return AuthoringBridge.WithInputs(inputs, (p, count) =>
        {
            fixed (SlidingPairRaw* s = sliding)
            {
                nint result; var status = Options.Kind is LimitedFeatureKind.Prism or LimitedFeatureKind.DraftedPrism
                    ? NativeMethods.LimitedPrism(p, count, s, sliding.Length, in raw, out result)
                    : NativeMethods.LimitedSweep(p, count, s, sliding.Length, in raw, out result);
                NativeError.ThrowIfFailed(status, "limited_feature"); return LocalFeatureBridge.Read(Id, result);
            }
        });
    }
    public Shape CopyInput(int index)
    { ObjectDisposedException.ThrowIf(disposed, this); if ((uint)index >= inputs.Length) throw new ArgumentOutOfRangeException(nameof(index)); return AuthoringBridge.CopyInputs([inputs[index]])[0]; }
    public void Dispose() { if (disposed) return; disposed = true; foreach (var input in inputs) input.Dispose(); }
}
#pragma warning restore CS1591
