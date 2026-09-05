using System.Text;
using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591
public enum LocalFeatureHistoryKind
{
    Modified, Generated, FirstCap, LastCap, Lateral, Contact, TangentContact, SurfacePatch,
    ContourEdge, AffectedFace, ProblemShape, Partial, Unchanged, Deleted, Unmapped, Limit, PreLimitShape
}
[Flags]
public enum LocalFeatureGroupSupport { None = 0, Caps = 1, Laterals = 2, Contacts = 4, Patches = 8, Evolution = 16 }
public enum LocalFeatureOperation { Fillet, Chamfer, FaceDraft, ShellDraft, Prism, DraftedPrism, Revolved, Pipe, LinearRibSlot, RevolutionRibSlot, Hole }
public enum LocalFeatureFaultKind { Contour, Vertex, DraftFace }
public readonly record struct LocalFeatureReference(Guid PlanId, int ArgumentIndex, int TopologyIndex, ShapeKind Kind);
public sealed record LocalFeatureHistoryItem(LocalFeatureReference? Source, LocalFeatureHistoryKind Kind, int? Group, Shape? Shape, int? ResultTopologyIndex = null) : IDisposable
{ public void Dispose() => Shape?.Dispose(); }
public sealed record LocalFeatureContour(int Index, int ProgramIndex, RepairSelection Seed, RepairSelection? FirstVertex,
    RepairSelection? LastVertex, bool IsClosed, bool IsClosedAndTangent, double Length,
    double? LawProbeError, int LawSampleCount);
public sealed record LocalFeatureContourEdge(int ContourIndex, int Ordinal, RepairSelection Edge,
    RepairSelection? FirstVertex, RepairSelection? LastVertex, double? FirstParameter, double? LastParameter);
/// <summary>Independent simulated circle and its trim angles; these are not spine station parameters.</summary>
public sealed record FilletSection(int ContourIndex, int PatchIndex, int Ordinal, GpXyz Center, GpXyz Normal,
    GpXyz XDirection, double Radius, double FirstParameter, double LastParameter);
public sealed record LocalFeatureFault(LocalFeatureFaultKind Kind, int? ContourIndex, RepairSelection? Source, int Status);
public sealed record LocalFeatureDiagnostics(LocalFeatureOperation Operation, bool Ready, bool AlgorithmDone, bool ShapeIsValid,
    bool HasPartialResult, int AlgorithmStatus, bool HasComposedHistory, LocalFeatureGroupSupport GroupSupport, string Message);

/// <summary>Owns copied result/history topology. Partial output never satisfies RequireShape.</summary>
public sealed class LocalFeatureResult : IDisposable
{
    private bool disposed;
    internal LocalFeatureResult(Guid id, RepairIdentity? source, string? fingerprint, Shape? shape, LocalFeatureDiagnostics diagnostics,
        LocalFeatureHistoryItem[] history, LocalFeatureContour[] contours, LocalFeatureContourEdge[] edges, FilletSection[] sections, LocalFeatureFault[] faults)
    {
        PlanId = id; Source = source; SourceFingerprint = fingerprint; Shape = shape; Diagnostics = diagnostics;
        History = Array.AsReadOnly(history); Contours = Array.AsReadOnly(contours); ContourEdges = Array.AsReadOnly(edges);
        SimulatedSections = Array.AsReadOnly(sections); Faults = Array.AsReadOnly(faults);
        ResultFingerprint = shape is null ? null : RepairSnapshot.ComputeFingerprint(shape);
    }
    public Guid PlanId { get; }
    public RepairIdentity? Source { get; }
    public string? SourceFingerprint { get; }
    internal string? ResultFingerprint { get; }
    public Shape? Shape { get; }
    public LocalFeatureDiagnostics Diagnostics { get; }
    public IReadOnlyList<LocalFeatureHistoryItem> History { get; }
    public IReadOnlyList<LocalFeatureContour> Contours { get; }
    public IReadOnlyList<LocalFeatureContourEdge> ContourEdges { get; }
    public IReadOnlyList<FilletSection> SimulatedSections { get; }
    public IReadOnlyList<LocalFeatureFault> Faults { get; }
    public IReadOnlyList<Shape> GetGroup(LocalFeatureHistoryKind kind)
    { ThrowIfDisposed(); return Array.AsReadOnly(History.Where(h => h.Kind == kind && h.Shape is not null).Select(h => h.Shape!).ToArray()); }
    public Shape RequireShape()
    {
        ThrowIfDisposed();
        if (!Diagnostics.AlgorithmDone || !Diagnostics.ShapeIsValid || Shape is null)
            throw new InvalidOperationException($"Local feature is not an accepted valid result: {Diagnostics.Message}");
        Shape.ThrowIfDisposed(); return Shape;
    }
    internal void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(disposed, this);
    public void Dispose() { if (disposed) return; disposed = true; Shape?.Dispose(); foreach (var h in History) h.Dispose(); }
}

internal static class LocalFeatureBridge
{
    internal static Shape ShareSelected(RepairSnapshot source, RepairSelection selection)
    {
        source.Validate(selection);
        NativeError.ThrowIfFailed(NativeMethods.LocalFeatureSourceSubshape(source.Shape.Handle, selection.Index, out nint result), "local_feature_source_subshape");
        return ShapeFactory.FromNativeHandle(result, "local_feature_source_subshape");
    }
    internal static XyzRaw Raw(GpXyz value) => new(value.X, value.Y, value.Z);
    private static GpXyz Value(XyzRaw value) => new(value.X, value.Y, value.Z);
    internal static void Select(RepairSnapshot source, RepairSelection selection, ShapeKind kind)
    {
        source.Validate(selection);
        if (source.Topology[selection.Index].Kind != kind) throw new ArgumentException($"Expected a {kind} selection.");
    }
    internal static unsafe LocalFeatureResult Read(Guid id, nint raw, RepairIdentity? source = null, string? fingerprint = null)
    {
        using FeatureResultHandle result = new(raw); Shape? shape = null; List<LocalFeatureHistoryItem> history = [];
        try
        {
            NativeError.ThrowIfFailed(NativeMethods.LocalFeatureSnapshot(result, out var info, null, 0, null, 0, null, 0, null, 0), "local_feature_counts");
            foreach (int count in new[] { info.ContourCount, info.EdgeCount, info.SectionCount, info.FaultCount, info.HistoryCount })
                if (count is < 0 or > 1000000) throw new InvalidOperationException("Local feature count exceeds its bounded contract.");
            ContourInfoRaw[] contours = new ContourInfoRaw[info.ContourCount]; ContourEdgeRaw[] edges = new ContourEdgeRaw[info.EdgeCount];
            FilletSectionRaw[] sections = new FilletSectionRaw[info.SectionCount]; LocalFeatureFaultRaw[] faults = new LocalFeatureFaultRaw[info.FaultCount];
            fixed (ContourInfoRaw* c = contours) fixed (ContourEdgeRaw* e = edges)
            fixed (FilletSectionRaw* s = sections) fixed (LocalFeatureFaultRaw* f = faults)
                NativeError.ThrowIfFailed(NativeMethods.LocalFeatureSnapshot(result, out info, c, contours.Length, e, edges.Length,
                    s, sections.Length, f, faults.Length), "local_feature_snapshot");
            NativeError.ThrowIfFailed(NativeMethods.GetFeatureResultShape(result, out nint root), "local_feature_shape");
            if (root != 0) shape = ShapeFactory.FromNativeHandle(root, "local_feature_shape");
            for (int i = 0; i < info.HistoryCount; i++)
            {
                NativeError.ThrowIfFailed(NativeMethods.LocalFeatureHistory(result, i, out var h, out nint item), "local_feature_history");
                Shape? owned = item == 0 ? null : ShapeFactory.FromNativeHandle(item, "local_feature_history");
                try { history.Add(new(h.ArgumentIndex < 0 || h.TopologyIndex < 0 ? null : new(id, h.ArgumentIndex, h.TopologyIndex, (ShapeKind)h.SourceKind),
                    (LocalFeatureHistoryKind)h.Kind, h.Group < 0 ? null : h.Group, owned, h.ResultTopologyIndex < 0 ? null : h.ResultTopologyIndex)); }
                catch { owned?.Dispose(); throw; }
            }
            NativeError.ThrowIfFailed(NativeMethods.GetFeatureResultMessage(result, null, 0, out int required), "local_feature_message_count");
            if (required is < 1 or > 1048576) throw new InvalidOperationException("Local feature message exceeds its bounded contract.");
            byte[] bytes = new byte[required]; fixed (byte* buffer = bytes)
                NativeError.ThrowIfFailed(NativeMethods.GetFeatureResultMessage(result, buffer, bytes.Length, out _), "local_feature_message");
            string message = Encoding.UTF8.GetString(bytes.AsSpan(0, bytes.Length - 1));
            RepairSelection? Selection(int index) => source is { } identity && index >= 0 ? new(identity, index) : null;
            return new(id, source, fingerprint, shape, new((LocalFeatureOperation)info.Operation, info.Ready != 0, info.Done != 0,
                info.Valid != 0, info.Partial != 0, info.AlgorithmStatus, info.Composed != 0, (LocalFeatureGroupSupport)info.GroupSupport, message),
                history.ToArray(), contours.Select(c => new LocalFeatureContour(c.Index, c.Program, Selection(c.Seed)!.Value,
                    Selection(c.FirstVertex), Selection(c.LastVertex), c.Closed != 0, c.Tangent != 0, c.Length,
                    c.LawApproximated != 0 ? c.LawProbeError : null, c.LawSampleCount)).ToArray(),
                edges.Select(e => new LocalFeatureContourEdge(e.Contour, e.Ordinal, Selection(e.SourceIndex)!.Value,
                    Selection(e.FirstVertex), Selection(e.LastVertex), e.FirstParameter >= 0 ? e.FirstParameter : null, e.LastParameter >= 0 ? e.LastParameter : null)).ToArray(),
                sections.Select(s => new FilletSection(s.Contour, s.Patch, s.Ordinal, Value(s.Center), Value(s.Normal), Value(s.XDirection),
                    s.Radius, s.FirstParameter, s.LastParameter)).ToArray(),
                faults.Select(f => new LocalFeatureFault((LocalFeatureFaultKind)f.Kind, f.Contour < 0 ? null : f.Contour, Selection(f.SourceIndex), f.Status)).ToArray());
        }
        catch { shape?.Dispose(); foreach (var h in history) h.Dispose(); throw; }
    }
}
#pragma warning restore CS1591
