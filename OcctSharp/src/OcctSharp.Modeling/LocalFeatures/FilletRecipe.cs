using System.Runtime.InteropServices;
using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591
public enum FilletRepresentation { Rational, QuasiAngular, Polynomial }
public enum FilletContinuity { C0, C1, C2 }
public readonly record struct FilletRadiusSample(double Parameter, double Radius);
public readonly record struct FilletVertexRadius(RepairSelection Vertex, double Radius);
public sealed record ContourFilletOptions
{
    public FilletRepresentation Representation { get; init; }
    public FilletContinuity Continuity { get; init; } = FilletContinuity.C1;
    public double TangentTolerance { get; init; } = 1e-2;
    public double Tolerance3d { get; init; } = 1e-4;
    public double Tolerance2d { get; init; } = 1e-5;
    public double Approximation3d { get; init; } = 1e-4;
    public double Approximation2d { get; init; } = 1e-5;
    public double Deflection { get; init; } = 1e-2;
    public double AngularTolerance { get; init; } = 1e-2;
    internal FilletOptionsRaw Raw(int action)
    {
        if (!Enum.IsDefined(Representation) || !Enum.IsDefined(Continuity)) throw new ArgumentException("Undefined fillet representation or continuity.");
        foreach (double value in new[] { TangentTolerance, Tolerance3d, Tolerance2d, Approximation3d, Approximation2d, Deflection, AngularTolerance })
            AuthoringBridge.Positive(value, nameof(ContourFilletOptions));
        return new() { Action = action, Representation = (int)Representation, Continuity = (int)Continuity,
            TangentTolerance = TangentTolerance, Tolerance3d = Tolerance3d, Tolerance2d = Tolerance2d,
            Approximation3d = Approximation3d, Approximation2d = Approximation2d, Deflection = Deflection, AngularTolerance = AngularTolerance };
    }
}

/// <summary>Immutable copied radius program on one source-bound tangent contour.</summary>
public sealed class FilletContourProgram
{
    public RepairSelection Seed { get; }
    public double? ConstantRadius { get; }
    public ScalarLawDefinition? Law { get; }
    public IReadOnlyList<FilletRadiusSample> Samples { get; }
    public IReadOnlyList<FilletVertexRadius> VertexRadii { get; }
    private FilletContourProgram(RepairSelection seed, double? radius, ScalarLawDefinition? law,
        FilletRadiusSample[] samples, FilletVertexRadius[] vertices)
    { Seed = seed; ConstantRadius = radius; Law = law; Samples = Array.AsReadOnly(samples); VertexRadii = Array.AsReadOnly(vertices); }
    public static FilletContourProgram Constant(RepairSelection seed, double radius)
    { AuthoringBridge.Positive(radius, nameof(radius)); return new(seed, radius, null, [], []); }
    public static FilletContourProgram FromLaw(RepairSelection seed, ScalarLawDefinition law)
    {
        ArgumentNullException.ThrowIfNull(law);
        if (law.Domain != new LawDomain(0, 1)) throw new ArgumentException("Contour laws use normalized [0,1] parameters.");
        if (!law.Sample().HasGlobalPositivityProof) throw new ArgumentException("The radius law needs a positive conservative control bound.");
        return new(seed, null, law, [], []);
    }
    public static FilletContourProgram Sampled(RepairSelection seed, IEnumerable<FilletRadiusSample> samples)
    {
        var copied = ScalarLawDefinition.Copy(samples);
        if (copied.Length < 2 || copied[0].Parameter != 0 || copied[^1].Parameter != 1)
            throw new ArgumentException("Radius samples must cover the complete [0,1] contour.");
        for (int i = 0; i < copied.Length; i++)
        {
            AuthoringBridge.Positive(copied[i].Radius, nameof(samples));
            if (!double.IsFinite(copied[i].Parameter) || (i > 0 && copied[i].Parameter <= copied[i - 1].Parameter))
                throw new ArgumentException("Radius sample parameters must strictly increase.");
        }
        return new(seed, null, null, copied, []);
    }
    /// <summary>Overrides constant-program vertex radii. For law/sample programs, anchors must agree
    /// with the authored law; changing that law requires an explicit replacement program.</summary>
    public FilletContourProgram WithVertexRadii(IEnumerable<FilletVertexRadius> vertices)
    {
        var copied = ScalarLawDefinition.Copy(vertices);
        foreach (var value in copied) AuthoringBridge.Positive(value.Radius, nameof(vertices));
        if (copied.Select(v => v.Vertex).Distinct().Count() != copied.Length) throw new ArgumentException("Duplicate vertex constraints.");
        return new(Seed, ConstantRadius, Law, Samples.ToArray(), copied);
    }
}

/// <summary>Copied recipe; Build requires its original immutable source snapshot. No native builder is retained.</summary>
public sealed class ContourFilletRecipe
{
    public Guid Id { get; } = Guid.NewGuid();
    public RepairIdentity Source { get; }
    public string SourceFingerprint { get; }
    public ContourFilletOptions Options { get; }
    public IReadOnlyList<FilletContourProgram> Programs { get; }
    private ContourFilletRecipe(RepairIdentity source, string fingerprint, FilletContourProgram[] programs, ContourFilletOptions options)
    { Source = source; SourceFingerprint = fingerprint; Programs = Array.AsReadOnly(programs); Options = options; }
    public static ContourFilletRecipe Create(RepairSnapshot source, IEnumerable<FilletContourProgram> programs, ContourFilletOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(source); source.ThrowIfDisposed(); options ??= new(); options.Raw(0);
        var copied = ScalarLawDefinition.Copy(programs, 256);
        foreach (var p in copied)
        {
            ArgumentNullException.ThrowIfNull(p); LocalFeatureBridge.Select(source, p.Seed, ShapeKind.Edge);
            foreach (var v in p.VertexRadii) LocalFeatureBridge.Select(source, v.Vertex, ShapeKind.Vertex);
        }
        return new(source.Identity, source.Fingerprint, copied, options);
    }
    public ContourFilletRecipe Replace(RepairSnapshot source, int index, FilletContourProgram program)
    { Validate(source); if ((uint)index >= Programs.Count) throw new ArgumentOutOfRangeException(nameof(index)); var copy = Programs.ToArray(); copy[index] = program; return Create(source, copy, Options); }
    public ContourFilletRecipe Remove(RepairSnapshot source, int index)
    { Validate(source); if ((uint)index >= Programs.Count) throw new ArgumentOutOfRangeException(nameof(index)); return Create(source, Programs.Where((_, i) => i != index), Options); }
    public LocalFeatureResult Discover(RepairSnapshot source) => Execute(source, 0);
    public LocalFeatureResult Simulate(RepairSnapshot source) => Execute(source, 1);
    public LocalFeatureResult Build(RepairSnapshot source) => Execute(source, 2);
    private void Validate(RepairSnapshot source)
    {
        ArgumentNullException.ThrowIfNull(source); source.ThrowIfDisposed();
        if (source.Identity != Source || source.Fingerprint != SourceFingerprint) throw new ArgumentException("Foreign or stale contour source.");
        if (RepairSnapshot.ComputeFingerprint(source.Shape) != SourceFingerprint) throw new InvalidOperationException("The frozen contour source was changed.");
    }
    private unsafe LocalFeatureResult Execute(RepairSnapshot source, int action)
    {
        Validate(source); List<RadiusSampleRaw> samples = []; List<VertexRadiusRaw> vertices = []; List<ScalarLawDefinition> laws = [];
        FilletProgramRaw[] programs = Programs.Select(p =>
        {
            FilletProgramRaw raw = new() { Seed = p.Seed.Index, Mode = p.ConstantRadius.HasValue ? 0 : p.Law is not null ? 1 : 2,
                Radius = p.ConstantRadius ?? 0, SampleOffset = samples.Count, SampleCount = p.Samples.Count, VertexOffset = vertices.Count,
                VertexCount = p.VertexRadii.Count, LawIndex = p.Law is null ? -1 : laws.Count };
            samples.AddRange(p.Samples.Select(s => new RadiusSampleRaw { Parameter = s.Parameter, Radius = s.Radius }));
            vertices.AddRange(p.VertexRadii.Select(v => new VertexRadiusRaw { Vertex = v.Vertex.Index, Radius = v.Radius }));
            if (p.Law is not null) laws.Add(p.Law); return raw;
        }).ToArray();
        if (samples.Count > 65536 || vertices.Count > 65536) throw new ArgumentException("Recipe exceeds bounded radius buffers.");
        RadiusSampleRaw[] sampleArray = samples.ToArray(); VertexRadiusRaw[] vertexArray = vertices.ToArray();
        FilletOptionsRaw options = Options.Raw(action);
        return PinnedFilletLaws.Call(laws, (lawPointer, lawCount) =>
        {
            fixed (FilletProgramRaw* p = programs) fixed (RadiusSampleRaw* s = sampleArray) fixed (VertexRadiusRaw* v = vertexArray)
            {
                NativeError.ThrowIfFailed(NativeMethods.ContourFillet(source.Shape.Handle, p, programs.Length, s, sampleArray.Length,
                    v, vertexArray.Length, lawPointer, lawCount, in options, out nint result), "contour_fillet");
                return LocalFeatureBridge.Read(Id, result, Source, SourceFingerprint);
            }
        });
    }
}

internal static class PinnedFilletLaws
{
    internal unsafe delegate T Action<T>(LawInputRaw* laws, int count);
    internal static unsafe T Call<T>(IReadOnlyList<ScalarLawDefinition> laws, Action<T> action)
    {
        List<GCHandle> pins = []; LawInputRaw[] values = new LawInputRaw[laws.Count];
        try
        {
            nint Pin(Array array) { if (array.Length == 0) return 0; var pin = GCHandle.Alloc(array, GCHandleType.Pinned); pins.Add(pin); return pin.AddrOfPinnedObject(); }
            for (int i = 0; i < laws.Count; i++)
            {
                var b = laws[i].Buffers(); values[i] = new() { Spans = (LawSpanRaw*)Pin(b.Spans), Values = (double*)Pin(b.Values),
                    Multiplicities = (int*)Pin(b.Multiplicities), SpanCount = b.Spans.Length, ValueCount = b.Values.Length,
                    MultiplicityCount = b.Multiplicities.Length, First = laws[i].Domain.First, Last = laws[i].Domain.Last };
            }
            fixed (LawInputRaw* p = values) return action(p, values.Length);
        }
        finally { foreach (var pin in pins) if (pin.IsAllocated) pin.Free(); }
    }
}
#pragma warning restore CS1591
