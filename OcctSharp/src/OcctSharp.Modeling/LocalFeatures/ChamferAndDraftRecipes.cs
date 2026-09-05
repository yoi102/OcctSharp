using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591
public enum ContourChamferMode { Classic, ConstantThroat, ConstantThroatPenetration }
public enum ChamferDimensions { Symmetric, TwoDistances, DistanceAngle }
/// <summary>Second is radians for DistanceAngle, a distance for TwoDistances, and unused for Symmetric.</summary>
public sealed record ChamferContourProgram(RepairSelection Seed, RepairSelection SupportFace, ChamferDimensions Dimensions,
    double First, double Second = 0);
public enum DraftPropagation { SelectedFaceOnly, NativeTangentChain }
public sealed record FaceDraftProgram(RepairSelection Face, double Angle, GpXyz PullDirection,
    GpXyz NeutralPlaneOrigin, GpXyz NeutralPlaneNormal, DraftPropagation Propagation = DraftPropagation.NativeTangentChain);

public sealed class ContourChamferRecipe
{
    public Guid Id { get; } = Guid.NewGuid();
    public RepairIdentity Source { get; }
    public string SourceFingerprint { get; }
    public ContourChamferMode Mode { get; }
    public IReadOnlyList<ChamferContourProgram> Programs { get; }
    private ContourChamferRecipe(RepairSnapshot source, ChamferContourProgram[] programs, ContourChamferMode mode)
    { Source = source.Identity; SourceFingerprint = source.Fingerprint; Programs = Array.AsReadOnly(programs); Mode = mode; }
    public static ContourChamferRecipe Create(RepairSnapshot source, IEnumerable<ChamferContourProgram> programs, ContourChamferMode mode = ContourChamferMode.Classic)
    {
        ArgumentNullException.ThrowIfNull(source); source.ThrowIfDisposed(); if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        var copy = ScalarLawDefinition.Copy(programs, 256);
        foreach (var p in copy)
        {
            ArgumentNullException.ThrowIfNull(p); LocalFeatureBridge.Select(source, p.Seed, ShapeKind.Edge);
            LocalFeatureBridge.Select(source, p.SupportFace, ShapeKind.Face); AuthoringBridge.Positive(p.First, nameof(programs));
            if (!Enum.IsDefined(p.Dimensions)) throw new ArgumentOutOfRangeException(nameof(programs));
            if (mode == ContourChamferMode.ConstantThroat && p.Dimensions != ChamferDimensions.Symmetric
                || mode == ContourChamferMode.ConstantThroatPenetration && p.Dimensions != ChamferDimensions.TwoDistances)
                throw new ArgumentException("Throat and penetration use their distinct dimension programs.");
            if (p.Dimensions != ChamferDimensions.Symmetric) AuthoringBridge.Positive(p.Second, nameof(programs));
            if (p.Dimensions == ChamferDimensions.DistanceAngle && p.Second >= Math.PI / 2) throw new ArgumentOutOfRangeException(nameof(programs));
        }
        return new(source, copy, mode);
    }
    public ContourChamferRecipe Replace(RepairSnapshot source, int index, ChamferContourProgram program)
    { Validate(source); if ((uint)index >= Programs.Count) throw new ArgumentOutOfRangeException(nameof(index)); var copy = Programs.ToArray(); copy[index] = program; return Create(source, copy, Mode); }
    public ContourChamferRecipe Remove(RepairSnapshot source, int index)
    { Validate(source); if ((uint)index >= Programs.Count) throw new ArgumentOutOfRangeException(nameof(index)); return Create(source, Programs.Where((_, i) => i != index), Mode); }
    public LocalFeatureResult Discover(RepairSnapshot source) => Execute(source, false);
    public LocalFeatureResult Build(RepairSnapshot source) => Execute(source, true);
    private void Validate(RepairSnapshot source)
    {
        ArgumentNullException.ThrowIfNull(source); source.ThrowIfDisposed();
        if (source.Identity != Source || RepairSnapshot.ComputeFingerprint(source.Shape) != SourceFingerprint) throw new ArgumentException("Foreign, changed or stale chamfer source.");
    }
    private unsafe LocalFeatureResult Execute(RepairSnapshot source, bool build)
    {
        Validate(source); var programs = Programs.Select(p => new ChamferProgramRaw { Seed = p.Seed.Index, Support = p.SupportFace.Index,
            Method = (int)p.Dimensions, First = p.First, Second = p.Second }).ToArray();
        fixed (ChamferProgramRaw* p = programs)
        {
            NativeError.ThrowIfFailed(NativeMethods.ContourChamfer(source.Shape.Handle, p, programs.Length, (int)Mode, build ? 1 : 0, out nint result), "contour_chamfer");
            return LocalFeatureBridge.Read(Id, result, Source, SourceFingerprint);
        }
    }
}

public sealed class FaceDraftRecipe
{
    public Guid Id { get; } = Guid.NewGuid();
    public RepairIdentity Source { get; }
    public string SourceFingerprint { get; }
    public IReadOnlyList<FaceDraftProgram> Programs { get; }
    private FaceDraftRecipe(RepairSnapshot source, FaceDraftProgram[] programs)
    { Source = source.Identity; SourceFingerprint = source.Fingerprint; Programs = Array.AsReadOnly(programs); }
    public static FaceDraftRecipe Create(RepairSnapshot source, IEnumerable<FaceDraftProgram> programs)
    {
        ArgumentNullException.ThrowIfNull(source); source.ThrowIfDisposed(); var copy = ScalarLawDefinition.Copy(programs, 256);
        if (copy.Length == 0) throw new ArgumentException("At least one draft face is required.");
        foreach (var p in copy)
        {
            ArgumentNullException.ThrowIfNull(p); LocalFeatureBridge.Select(source, p.Face, ShapeKind.Face);
            if (!Enum.IsDefined(p.Propagation) || !double.IsFinite(p.Angle) || Math.Abs(p.Angle) <= 1e-4 || Math.Abs(p.Angle) >= Math.PI / 2)
                throw new ArgumentOutOfRangeException(nameof(programs), "Draft angles must exceed the kernel no-op threshold and remain below pi/2.");
        }
        return new(source, copy);
    }
    public LocalFeatureResult Preflight(RepairSnapshot source) => Execute(source, false);
    public LocalFeatureResult Build(RepairSnapshot source) => Execute(source, true);
    private unsafe LocalFeatureResult Execute(RepairSnapshot source, bool build)
    {
        ArgumentNullException.ThrowIfNull(source); source.ThrowIfDisposed();
        if (source.Identity != Source || RepairSnapshot.ComputeFingerprint(source.Shape) != SourceFingerprint) throw new ArgumentException("Foreign, changed or stale draft source.");
        var programs = Programs.Select(p => new FaceDraftProgramRaw { Face = p.Face.Index, Propagation = (int)p.Propagation, Angle = p.Angle,
            Direction = LocalFeatureBridge.Raw(p.PullDirection), PlaneOrigin = LocalFeatureBridge.Raw(p.NeutralPlaneOrigin), PlaneNormal = LocalFeatureBridge.Raw(p.NeutralPlaneNormal) }).ToArray();
        fixed (FaceDraftProgramRaw* p = programs)
        {
            NativeError.ThrowIfFailed(NativeMethods.FaceDraft(source.Shape.Handle, p, programs.Length, build ? 1 : 0, out nint result), "face_draft");
            return LocalFeatureBridge.Read(Id, result, Source, SourceFingerprint);
        }
    }
}
#pragma warning restore CS1591
