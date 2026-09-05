namespace OcctSharp;

#pragma warning disable CS1591
/// <summary>An actual algorithm result associated with a source feature, not a claimed persistent source-subshape correspondence.</summary>
public sealed record ParametricAlgorithmHistory(Guid SourceFeatureId, string Kind, Shape? Shape) : IDisposable
{
    public bool HasExactSourceSubshape { get; }
    public void Dispose() => Shape?.Dispose();
}
public sealed record ParametricSelection(Guid DocumentId, Guid FeatureId, Guid Id, string Entry, ShapeKind Kind, Guid CreatedRevision);
public sealed class ParametricSelectionResult : IDisposable
{
    internal ParametricSelectionResult(ParametricSelectionStatus status, Shape? shape) { Status = status; Shape = shape; }
    public ParametricSelectionStatus Status { get; }
    public Shape? Shape { get; }
    public void Dispose() => Shape?.Dispose();
}
public sealed class ParametricHistorySnapshot : IDisposable
{
    internal ParametricHistorySnapshot(Guid feature, Guid revision, IReadOnlyList<ParametricEvolution> evolutions)
    { FeatureId = feature; ResultRevision = revision; Evolutions = evolutions; }
    public Guid FeatureId { get; }
    public Guid ResultRevision { get; }
    public IReadOnlyList<ParametricEvolution> Evolutions { get; }
    public void Dispose() { foreach (var item in Evolutions) item.Dispose(); }
}

public sealed partial class ParametricDocument
{
    public IReadOnlyList<string> GetDiagnostics(Guid feature)
    {
        var value = Get(ReadFeatures(), feature);
        return storage.GetText(value.HistoryEntry, "diagnostics") is null ? [] : Array.AsReadOnly(Read<string[]>(value.HistoryEntry, "diagnostics"));
    }

    public IReadOnlyList<ParametricAlgorithmHistory> GetAlgorithmHistory(Guid feature)
    {
        var value = Get(ReadFeatures(), feature); List<ParametricAlgorithmHistory> result = [];
        if (storage.GetText(value.HistoryEntry, "algorithmHistory") is null) return result.AsReadOnly();
        var entries = Read<StoredAlgorithmHistory[]>(value.HistoryEntry, "algorithmHistory");
        try
        {
            foreach (var item in entries)
            {
                if (!IsDirectChild(item.Entry, value.HistoryEntry)) throw new InvalidDataException("Invalid history label.");
                result.Add(new(item.SourceFeatureId, item.Kind, item.HasShape ? RequiredShape(item.Entry) : null));
            }
            return result.AsReadOnly();
        }
        catch { foreach (var item in result) item.Dispose(); throw; }
    }
    /// <summary>Creates a persistent selection by index in GetSubShapes(kind), guarded by the exact result generation.</summary>
    public ParametricSelection Select(ParametricResult result, ShapeKind kind, int index)
    {
        ArgumentNullException.ThrowIfNull(result); Check();
        if (result.DocumentId != Id || result.IsStale || result.Kind != ParametricOutputKind.ExactShape)
            throw new ArgumentException("Selections require a current exact result from this document.");
        result.Shape?.ThrowIfDisposed();
        if (kind is not (ShapeKind.Edge or ShapeKind.Face or ShapeKind.Vertex)) throw new ArgumentOutOfRangeException(nameof(kind));
        var features = ReadFeatures(); var feature = Get(features, result.FeatureId);
        if (feature.ResultRevision != result.Revision || IsStale(feature, features)) throw new InvalidOperationException("Selection context generation is stale.");
        using var context = RequiredShape(feature.ResultEntry);
        Shape[] shapes = context.GetSubShapes(kind);
        try
        {
            if ((uint)index >= shapes.Length) throw new ArgumentOutOfRangeException(nameof(index));
            using var command = BeginCommand("Create persistent parametric selection");
            string entry = AddChild(feature.SelectorsEntry);
            if (!storage.Select(entry, feature.ResultEntry, shapes[index], kind)) throw new NotSupportedException("OCCT cannot name this selection.");
            var selection = new ParametricSelection(Id, feature.Definition.Id, Guid.NewGuid(), entry, kind, result.Revision);
            var selections = ReadSelections(feature); selections.Add(selection);
            Write(feature.SelectorsEntry, "selections", selections);
            Write(entry, "selection", selection);
            command.Commit(); return selection;
        }
        finally { foreach (var shape in shapes) shape.Dispose(); }
    }

    public IReadOnlyList<ParametricSelection> GetSelections(Guid feature) => ReadSelections(Get(ReadFeatures(), feature)).AsReadOnly();

    public ParametricSelectionResult Resolve(ParametricSelection selection)
    {
        ArgumentNullException.ThrowIfNull(selection); Check();
        if (selection.DocumentId != Id) throw new ArgumentException("The selector belongs to another document.");
        var features = ReadFeatures();
        if (!features.TryGetValue(selection.FeatureId, out var feature)) return new(ParametricSelectionStatus.Deleted, null);
        if (!ReadSelections(feature).Contains(selection)) throw new ArgumentException("The selector identity is absent or was altered.");
        if (IsStale(feature, features)) return new(ParametricSelectionStatus.Unsupported, null);
        using var command = BeginCommand("Resolve persistent parametric selection");
        var result = storage.Resolve(selection.Entry, selection.Kind);
        try { command.Commit(); return new(result.Status, result.Shape); }
        catch { result.Shape?.Dispose(); throw; }
    }

    /// <summary>Copies cumulative evolution recorded through a persisted result generation, independent of the OCAF transaction nesting level.</summary>
    public ParametricHistorySnapshot GetHistory(Guid feature, Guid? resultRevision = null)
    {
        var value = Get(ReadFeatures(), feature);
        Guid revision = resultRevision ?? value.ResultRevision ?? throw new InvalidOperationException("The feature has no published history.");
        var snapshot = ReadHistoryRevisions(value).SingleOrDefault(x => x.ResultRevision == revision)
            ?? throw new ArgumentException("The requested result generation does not belong to this feature.", nameof(resultRevision));
        List<ParametricEvolution> items = [];
        try
        {
            foreach (string child in snapshot.Entries) items.AddRange(storage.History(child));
            return new(feature, revision, items.AsReadOnly());
        }
        catch { foreach (var item in items) item.Dispose(); throw; }
    }

    private List<StoredHistoryRevision> ReadHistoryRevisions(StoredFeature feature)
    {
        var revisions = storage.GetText(feature.HistoryEntry, "revisions") is null ? []
            : Read<List<StoredHistoryRevision>>(feature.HistoryEntry, "revisions");
        if (revisions.Count > 10000 || revisions.Select(x => x.ResultRevision).Distinct().Count() != revisions.Count
            || revisions.Any(x => x.ResultRevision == Guid.Empty || x.Entries is null
                || x.Entries.Any(entry => !IsDirectChild(entry, feature.HistoryEntry))))
            throw new InvalidDataException("Malformed result history revision table.");
        return revisions;
    }

    private List<ParametricSelection> ReadSelections(StoredFeature feature)
    {
        var json = storage.GetText(feature.SelectorsEntry, "selections");
        var values = json is null ? [] : System.Text.Json.JsonSerializer.Deserialize<List<ParametricSelection>>(json, JsonOptions)
            ?? throw new InvalidDataException("Null selection table.");
        if (values.Count > 4096 || values.Select(x => x.Id).Distinct().Count() != values.Count
            || values.Any(x => x.DocumentId != Id || x.FeatureId != feature.Definition.Id || x.Id == Guid.Empty
                || !IsDirectChild(x.Entry, feature.SelectorsEntry) || x.Kind is not (ShapeKind.Edge or ShapeKind.Face or ShapeKind.Vertex)))
            throw new InvalidDataException("Malformed selection table.");
        return values;
    }
}
#pragma warning restore CS1591
