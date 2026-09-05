using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591
public enum GuidedSweepFrame { CorrectedFrenet, Frenet, FixedFrame, FixedBinormal, Discrete, SupportSurface, AuxiliarySpine }
public enum GuidedSweepContact { None, KeepContact, ContactOnBorder }
public enum GuidedSweepTransition { Transformed, RightCorner, RoundCorner }
public enum SweepSolidPolicy { Shell, RequireSolid, AllowValidShellIfSolidificationFails }
public sealed record GuidedSweepSection(Shape Profile, Shape? SpineVertex = null, bool WithContact = false, bool WithCorrection = false);
public sealed record GuidedSectionDefinition(int ProfileInputIndex, int? SpineVertexInputIndex, bool WithContact, bool WithCorrection);
public sealed record GuidedSweepOptions
{
    public GuidedSweepFrame Frame { get; init; }
    public GpXyz FrameOrigin { get; init; } = new(0, 0, 0);
    public GpXyz FrameDirection { get; init; } = new(0, 0, 1);
    public GpXyz FrameXDirection { get; init; } = new(1, 0, 0);
    public GuidedSweepContact Contact { get; init; }
    public bool CurvilinearEquivalence { get; init; } = true;
    public GuidedSweepTransition Transition { get; init; }
    public SweepSolidPolicy SolidPolicy { get; init; }
    public bool ForceApproximationC1 { get; init; }
    public int MaximumDegree { get; init; } = 11;
    public int MaximumSegments { get; init; } = 64;
    public double Tolerance3d { get; init; } = 1e-4;
    public double ToleranceBoundary { get; init; } = 1e-4;
    public double ToleranceAngular { get; init; } = 1e-2;
    internal void Validate()
    {
        if (!Enum.IsDefined(Frame) || !Enum.IsDefined(Contact) || !Enum.IsDefined(Transition) || !Enum.IsDefined(SolidPolicy))
            throw new ArgumentOutOfRangeException(nameof(Frame));
        if (MaximumDegree is < 2 or > 25 || MaximumSegments is < 1 or > 512) throw new ArgumentOutOfRangeException(nameof(MaximumDegree));
        AuthoringBridge.Positive(Tolerance3d, nameof(Tolerance3d)); AuthoringBridge.Positive(ToleranceBoundary, nameof(ToleranceBoundary));
        AuthoringBridge.Positive(ToleranceAngular, nameof(ToleranceAngular));
        foreach (double value in new[] { FrameOrigin.X, FrameOrigin.Y, FrameOrigin.Z, FrameDirection.X, FrameDirection.Y, FrameDirection.Z,
            FrameXDirection.X, FrameXDirection.Y, FrameXDirection.Z })
            if (!double.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(FrameOrigin));
    }
}

/// <summary>Owns a private copy of the complete source dependency graph, including attachment/support identities.</summary>
public sealed class GuidedSweepPlan : IDisposable
{
    private readonly Shape[] inputs;
    private readonly SweepSectionRaw[] sections;
    private readonly int secondaryIndex;
    private bool disposed;
    public Guid Id { get; } = Guid.NewGuid();
    public GuidedSweepOptions Options { get; }
    public ScalarLawDefinition? ScaleLaw { get; }
    public int SectionCount => sections.Length;
    public int InputCount => inputs.Length;
    public int? GuideOrSupportInputIndex => secondaryIndex >= 0 ? secondaryIndex : null;
    public IReadOnlyList<GuidedSectionDefinition> Sections => Array.AsReadOnly(sections.Select(s => new GuidedSectionDefinition(
        s.ShapeIndex, s.LocationIndex >= 0 ? s.LocationIndex : null, s.Contact != 0, s.Correction != 0)).ToArray());
    private GuidedSweepPlan(Shape[] inputs, SweepSectionRaw[] sections, int secondary,
        GuidedSweepOptions options, ScalarLawDefinition? law)
    { this.inputs = inputs; this.sections = sections; secondaryIndex = secondary; Options = options; ScaleLaw = law; }
    public static GuidedSweepPlan Create(Shape spine, IEnumerable<GuidedSweepSection> sections,
        GuidedSweepOptions? options = null, Shape? guideOrSupport = null, ScalarLawDefinition? scaleLaw = null)
    {
        ArgumentNullException.ThrowIfNull(spine); options ??= new(); options.Validate();
        GuidedSweepSection[] copied = ScalarLawDefinition.Copy(sections, 128);
        if (copied.Length == 0) throw new ArgumentException("At least one section is required.");
        bool needsSecondary = options.Frame is GuidedSweepFrame.AuxiliarySpine or GuidedSweepFrame.SupportSurface;
        if (needsSecondary != (guideOrSupport is not null)) throw new ArgumentException("Provide a guide/support only for its selected frame mode.");
        if (options.Frame != GuidedSweepFrame.AuxiliarySpine && options.Contact != GuidedSweepContact.None)
            throw new ArgumentException("Auxiliary contact requires the auxiliary frame.");
        if (scaleLaw is not null && (copied.Length != 1 || options.Frame == GuidedSweepFrame.AuxiliarySpine))
            throw new ArgumentException("Scale laws require one section and cannot be combined with auxiliary guides.");
        List<Shape> source = [spine]; int secondary = -1;
        if (guideOrSupport is not null) { secondary = source.Count; source.Add(guideOrSupport); }
        List<SweepSectionRaw> raw = [];
        foreach (var section in copied)
        {
            ArgumentNullException.ThrowIfNull(section); ArgumentNullException.ThrowIfNull(section.Profile);
            int profile = source.Count; source.Add(section.Profile); int location = -1;
            if (section.SpineVertex is not null) { location = source.Count; source.Add(section.SpineVertex); }
            raw.Add(new() { ShapeIndex = profile, LocationIndex = location, Contact = section.WithContact ? 1 : 0, Correction = section.WithCorrection ? 1 : 0 });
        }
        return new(AuthoringBridge.CopyInputs(source), raw.ToArray(), secondary, options, scaleLaw);
    }
    public AuthoringDiagnostics Preflight() { using var result = Execute(0, 2); return result.Diagnostics; }
    /// <summary>OCCT samples an equally spaced count along the spine; no arbitrary station list is implied.</summary>
    public AuthoringResult Simulate(int count = 5) => Execute(1, count);
    public AuthoringResult Build() => Execute(2, 2);
    private unsafe AuthoringResult Execute(int operation, int count)
    {
        ObjectDisposedException.ThrowIf(disposed, this); if (count is < 2 or > 256) throw new ArgumentOutOfRangeException(nameof(count));
        var o = Options;
        SweepOptionsRaw options = new() { Frame = (int)o.Frame, SecondaryIndex = secondaryIndex, Curvilinear = o.CurvilinearEquivalence ? 1 : 0,
            Contact = (int)o.Contact, Transition = (int)o.Transition, MaximumDegree = o.MaximumDegree, MaximumSegments = o.MaximumSegments,
            ForceC1 = o.ForceApproximationC1 ? 1 : 0, SolidPolicy = (int)o.SolidPolicy, SimulationCount = count, Operation = operation,
            Tolerance3d = o.Tolerance3d, ToleranceBoundary = o.ToleranceBoundary, ToleranceAngular = o.ToleranceAngular,
            Origin = new(o.FrameOrigin.X, o.FrameOrigin.Y, o.FrameOrigin.Z),
            Direction = new(o.FrameDirection.X, o.FrameDirection.Y, o.FrameDirection.Z),
            XDirection = new(o.FrameXDirection.X, o.FrameXDirection.Y, o.FrameXDirection.Z) };
        return AuthoringBridge.WithInputs(inputs, (p, length) =>
        {
            var buffers = ScaleLaw?.Buffers() ?? (Array.Empty<LawSpanRaw>(), Array.Empty<double>(), Array.Empty<int>());
            fixed (SweepSectionRaw* s = sections) fixed (LawSpanRaw* spans = buffers.Item1)
            fixed (double* values = buffers.Item2) fixed (int* multiplicities = buffers.Item3)
            {
                LawInputRaw law = new() { Spans = spans, Values = values, Multiplicities = multiplicities,
                    SpanCount = buffers.Item1.Length, ValueCount = buffers.Item2.Length, MultiplicityCount = buffers.Item3.Length,
                    First = ScaleLaw?.Domain.First ?? 0, Last = ScaleLaw?.Domain.Last ?? 0 };
                NativeError.ThrowIfFailed(NativeMethods.GuidedSweep(p, length, s, sections.Length, in options,
                    ScaleLaw is null ? null : &law, out AuthoringInfoRaw info, out nint output), "guided_sweep");
                return AuthoringBridge.Read(Id, output, info);
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
