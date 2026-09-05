using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591
public sealed record VolumeConstructionOptions(double FuzzyTolerance = 0, bool IntersectInputs = true,
    bool AvoidInternalShapes = false, bool RunParallel = false, int MaximumSolids = 100000);
public sealed record ConstructedVolume(RegionCellId Id, double Volume, bool IsValid);
public sealed record VolumeSourceFace(RegionCellId Volume, int InputIndex, int FaceIndex, int ItemIndex);
public enum VolumePointState { Inside, Outside, OnBoundary, Unknown }
public enum VolumeShellRole { Unknown = -1, Exterior, Cavity }
public sealed record VolumeShellInfo(RegionCellId Volume, int ShellIndex, VolumeShellRole Role, bool IsClosed, int Orientation, int ItemIndex);
public sealed record VolumeShellCandidate(int Index, bool IsClosed, bool IsValid, int ItemIndex);
public sealed record VolumePointHit(RegionCellId Volume, VolumePointState State);
public enum VolumeBoundaryPolicy { Exclude, Include, Reject }

/// <summary>Independent selected topology and copied classifications, including excluded hits.</summary>
public sealed class VolumePointSelection : IDisposable
{
    internal VolumePointSelection(Shape shape, IReadOnlyList<VolumePointHit> hits, IReadOnlyList<RegionCellId> selected)
    { Shape = shape; Hits = hits; Selected = selected; }
    public Shape Shape { get; }
    public IReadOnlyList<VolumePointHit> Hits { get; }
    public IReadOnlyList<RegionCellId> Selected { get; }
    public void Dispose() => Shape.Dispose();
}

/// <summary>Copied face/shape inputs; a false IntersectInputs still performs native interference verification.</summary>
public sealed class VolumeConstructionPlan : IDisposable
{
    private readonly Shape[] inputs;
    private bool disposed;
    private VolumeConstructionPlan(Shape[] inputs, VolumeConstructionOptions options) { this.inputs = inputs; Options = options; }
    public VolumeConstructionOptions Options { get; }
    public static VolumeConstructionPlan Create(IReadOnlyList<Shape> inputs, VolumeConstructionOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(inputs); options ??= new();
        if (inputs.Count is < 1 or > 512 || !double.IsFinite(options.FuzzyTolerance) || options.FuzzyTolerance < 0 || options.MaximumSolids is < 1 or > 100000)
            throw new ArgumentException("Invalid volume construction input count, precision or capacity.");
        return new(AuthoringBridge.CopyInputs(inputs), options);
    }
    public unsafe VolumeConstructionResult Build()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        VolumeOptionsRaw options = new() { Fuzzy = Options.FuzzyTolerance, Intersect = Options.IntersectInputs ? 1 : 0,
            AvoidInternal = Options.AvoidInternalShapes ? 1 : 0, Parallel = Options.RunParallel ? 1 : 0, MaxSolids = Options.MaximumSolids };
        var storage = AuthoringBridge.WithInputs(inputs, (p, count) =>
        {
            NativeError.ThrowIfFailed(NativeMethods.VolumeBuild(p, count, in options, out nint result), "volume_build");
            return RegionStorage.Read(result);
        });
        try { return new(storage); } catch { storage.Dispose(); throw; }
    }
    public void Dispose() { if (disposed) return; disposed = true; foreach (var shape in inputs) shape.Dispose(); }
}

/// <summary>Zero/one/many bounded solids, actual source-face images and unresolved input boundaries.</summary>
public sealed class VolumeConstructionResult : IDisposable
{
    private readonly RegionStorage storage;
    private readonly int[] cells;
    internal VolumeConstructionResult(RegionStorage storage)
    {
        this.storage = storage; Revision = Guid.NewGuid();
        var items = storage.Find(RegionItemKind.Cell).OrderBy(x => x.Item.A).ToArray(); cells = items.Select(x => x.Index).ToArray();
        Volumes = Array.AsReadOnly(items.Select(x => new ConstructedVolume(new(Revision, x.Item.A), x.Item.Measure, x.Item.Flags != 0)).ToArray());
        SourceFaces = Array.AsReadOnly(storage.Find(RegionItemKind.SourceFace).Select(x => new VolumeSourceFace(new(Revision, x.Item.A), x.Item.B, x.Item.C, x.Index)).ToArray());
        Shells = Array.AsReadOnly(storage.Find(RegionItemKind.VolumeShell).Select(x => new VolumeShellInfo(new(Revision, x.Item.A), x.Item.B,
            (VolumeShellRole)x.Item.C, x.Item.D != 0, x.Item.Flags, x.Index)).ToArray());
        ShellCandidates = Array.AsReadOnly(storage.Find(RegionItemKind.ShellCandidate).Select(x => new VolumeShellCandidate(x.Item.A, x.Item.B != 0, x.Item.C != 0, x.Index)).ToArray());
        UnusedFaceItems = Array.AsReadOnly(storage.Find(RegionItemKind.UnusedFace).Select(x => x.Index).ToArray());
        FreeBoundaryItems = Array.AsReadOnly(storage.Find(RegionItemKind.FreeBoundary).Select(x => x.Index).ToArray());
        InternalTopologyItems = Array.AsReadOnly(storage.Find(RegionItemKind.InternalTopology).Select(x => x.Index).ToArray());
        HelperBoxExcluded = storage.Find(RegionItemKind.HelperCheck).Any(x => x.Item.Flags == 1 && x.Item.B == 0);
        Diagnostics = new(storage.Info.Done != 0, storage.Info.Valid != 0, storage.Info.Warnings != 0, storage.Message,
            Array.AsReadOnly(storage.Find(RegionItemKind.Fault).Select(x => x.Item.A).Distinct().ToArray()));
    }
    public Guid Revision { get; }
    public IReadOnlyList<ConstructedVolume> Volumes { get; }
    public IReadOnlyList<VolumeSourceFace> SourceFaces { get; }
    public IReadOnlyList<VolumeShellInfo> Shells { get; }
    public IReadOnlyList<VolumeShellCandidate> ShellCandidates { get; }
    public IReadOnlyList<int> UnusedFaceItems { get; }
    public IReadOnlyList<int> FreeBoundaryItems { get; }
    public IReadOnlyList<int> InternalTopologyItems { get; }
    public bool HelperBoxExcluded { get; }
    public RegionDiagnostics Diagnostics { get; }
    public Shape CopyVolume(RegionCellId id)
    {
        storage.ThrowIfDisposed();
        if (id.Revision != Revision || (uint)id.Index >= cells.Length) throw new ArgumentException("Foreign or invalid volume ID.");
        return storage.Copy(cells[id.Index]);
    }
    public Shape CopyDiagnosticShape(int itemIndex)
    {
        storage.ThrowIfDisposed();
        if (!UnusedFaceItems.Contains(itemIndex) && !FreeBoundaryItems.Contains(itemIndex) && !InternalTopologyItems.Contains(itemIndex)
            && !SourceFaces.Any(f => f.ItemIndex == itemIndex) && !Shells.Any(s => s.ItemIndex == itemIndex)
            && !ShellCandidates.Any(s => s.ItemIndex == itemIndex)) throw new ArgumentException("Not a volume diagnostic item.");
        return storage.Copy(itemIndex);
    }
    public Shape CopyResult()
    {
        storage.ThrowIfDisposed();
        if (!Diagnostics.AlgorithmDone || !Diagnostics.IsValid || !HelperBoxExcluded)
            throw new InvalidOperationException($"Volume construction is not accepted: {Diagnostics.Message}");
        return storage.Copy(storage.Find(RegionItemKind.Output).Single().Index);
    }
    public void Dispose() => storage.Dispose();
    public IReadOnlyList<VolumePointHit> ClassifyPoint(GpPoint point, double tolerance = 1e-7)
    {
        storage.ThrowIfDisposed();
        if (!double.IsFinite(tolerance) || tolerance <= 0) throw new ArgumentOutOfRangeException(nameof(tolerance));
        List<VolumePointHit> results = [];
        foreach (var volume in Volumes)
        {
            NativeError.ThrowIfFailed(NativeMethods.RegionClassifySolid(storage.Get(cells[volume.Id.Index]).Handle,
                new(point.X, point.Y, point.Z), tolerance, out int state), "region_classify_solid");
            results.Add(new(volume.Id, (VolumePointState)state));
        }
        return results.AsReadOnly();
    }
    /// <summary>Copies volumes containing the point. Unknown containment rejects; boundary handling is explicit.</summary>
    public VolumePointSelection SelectPoint(GpPoint point, VolumeBoundaryPolicy boundaryPolicy, double tolerance = 1e-7)
    {
        if (!Enum.IsDefined(boundaryPolicy)) throw new ArgumentOutOfRangeException(nameof(boundaryPolicy));
        var hits = ClassifyPoint(point, tolerance);
        if (hits.Any(h => h.State == VolumePointState.Unknown ||
            (h.State == VolumePointState.OnBoundary && boundaryPolicy == VolumeBoundaryPolicy.Reject)))
            throw new InvalidOperationException("Point containment is unknown or rejected by the explicit boundary policy.");
        var selected = hits.Where(h => h.State == VolumePointState.Inside ||
            (h.State == VolumePointState.OnBoundary && boundaryPolicy == VolumeBoundaryPolicy.Include)).Select(h => h.Volume).ToArray();
        List<Shape> copies = [];
        try
        {
            foreach (var id in selected) copies.Add(CopyVolume(id));
            if (copies.Count == 0)
            {
                NativeError.ThrowIfFailed(NativeMethods.CreateCompound(out nint empty), "region_empty_selection");
                return new(ShapeFactory.FromNativeHandle(empty, "region_empty_selection"), hits, Array.AsReadOnly(selected));
            }
            return new(ShapeFactory.CreateCompound(copies), hits, Array.AsReadOnly(selected));
        }
        finally { foreach (var copy in copies) copy.Dispose(); }
    }
    /// <summary>Creates typed containers from exact shared-face solids, preserving disconnected bodies.</summary>
    public Shape CopyAdjacencyContainers()
    {
        storage.ThrowIfDisposed();
        if (cells.Length < 2) return CopyResult();
        using var plan = PartitionPlan.Create(cells.Select(storage.Get).ToArray());
        using var result = plan.Build([new("volumes", [new(RegionExpression.All)], makeContainers: true)]);
        return result.CopyOutput("volumes");
    }
}
