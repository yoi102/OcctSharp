using System.Text.Json;
using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591
/// <summary>Typed parametric graph attached to an OCAF/XDE document. Operations are synchronous and not concurrent.</summary>
public sealed partial class ParametricDocument : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new() { MaxDepth = 128 };
    private readonly object parent;
    private readonly IDisposable? ownedParent;
    private readonly OcafDocumentHandle handle;
    private readonly ParametricStorage storage;
    private bool disposed;

    private ParametricDocument(object parent, OcafDocumentHandle handle, bool ownsParent, string? root)
    {
        this.parent = parent;
        this.handle = handle;
        ownedParent = ownsParent ? (IDisposable)parent : null;
        storage = new(handle);
        if (root is null)
        {
            using var command = BeginCommand("Create parametric graph");
            RootEntry = AddChild("0:1");
            WriteManifest(new(1, Guid.NewGuid(), []));
            command.Commit();
        }
        else
        {
            RootEntry = root;
            _ = ReadManifest(); // Reject unknown schemas before any recovery mutation.
            var features = ReadFeatures();
            if (features.Values.Any(x => x.State == ParametricExecutionState.Executing))
            {
                using var command = BeginCommand("Recover interrupted parametric execution");
                var plan = ParametricPlanning.Build(features.Values.Select(x => x.Definition).ToArray());
                var interrupted = Dependants(features.Values.Where(x => x.State == ParametricExecutionState.Executing).Select(x => x.Definition.Id), plan);
                foreach (var feature in features.Values.Where(x => interrupted.Contains(x.Definition.Id)))
                    WriteFeature(feature with { State = ParametricExecutionState.NotExecuted, Dirty = true, Error = "Interrupted execution requires recompute." });
                command.Commit();
            }
        }
    }

    public string RootEntry { get; }
    public Guid Id { get { Check(); return ReadManifest().DocumentId; } }
    public static ParametricDocument Create()
    {
        var doc = OcafDocument.Create();
        try { return new(doc, doc.Handle, true, null); }
        catch { doc.Dispose(); throw; }
    }
    public static ParametricDocument CreateXde()
    {
        var doc = XdeDocument.Create();
        try { return new(doc, doc.Handle, true, null); }
        catch { doc.Dispose(); throw; }
    }
    public static ParametricDocument Attach(OcafDocument document, string? rootEntry = null)
    {
        ArgumentNullException.ThrowIfNull(document); document.ThrowIfDisposed();
        return new(document, document.Handle, false, rootEntry);
    }
    public static ParametricDocument Attach(XdeDocument document, string? rootEntry = null)
    {
        ArgumentNullException.ThrowIfNull(document); document.ThrowIfDisposed();
        return new(document, document.Handle, false, rootEntry);
    }
    public static ParametricDocument Open(string path, string rootEntry, bool xde = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootEntry);
        if (xde)
        {
            var doc = XdeDocument.Open(path);
            try { return new(doc, doc.Handle, true, rootEntry); }
            catch { doc.Dispose(); throw; }
        }
        else
        {
            var doc = OcafDocument.Open(path);
            try { return new(doc, doc.Handle, true, rootEntry); }
            catch { doc.Dispose(); throw; }
        }
    }

    public XdeDocument Xde => parent as XdeDocument ?? throw new InvalidOperationException("This is a generic OCAF graph.");
    public IReadOnlyList<ParametricFeatureSnapshot> Features => ReadFeatures().Values.OrderBy(x => x.Definition.Id).Select(Snapshot).ToArray();
    public ParametricExecutionPlan CreatePlan() => ParametricPlanning.Build(ReadFeatures().Values.Select(x => x.Definition).ToArray());

    public Guid Add(ParametricFeatureDefinition definition, Shape? source = null)
    {
        Check(); ArgumentNullException.ThrowIfNull(definition);
        var features = ReadFeatures();
        if (features.ContainsKey(definition.Id)) throw new ArgumentException("The feature identity already exists.");
        if ((definition.Kind == ParametricFeatureKind.SourceShape) != (source is not null))
            throw new ArgumentException("Only source features require a source shape.");
        source?.ThrowIfDisposed();
        if (source is not null) RequireExactTopology(source);
        var plan = ParametricPlanning.Build(features.Values.Select(x => x.Definition).Append(definition).ToArray());
        RejectInvalidGraph(plan);
        using var command = BeginCommand("Add parametric feature");
        string entry = AddChild(RootEntry);
        string parameters = AddChild(entry), result = AddChild(entry), references = AddChild(entry), history = AddChild(entry), selectors = AddChild(entry), sourceEntry = AddChild(entry);
        int nativeId = storage.Register(entry, DriverId(definition.Kind));
        var record = new StoredFeature(definition, entry, parameters, result, references, history, selectors, sourceEntry, nativeId,
            Guid.NewGuid(), null, null, [], ParametricExecutionState.NotExecuted, true, true, null, new Dictionary<string, string>());
        WriteParameters(record);
        if (source is not null)
        {
            using var copy = MeshTopology.CopyWithTriangulation(source);
            DocumentStateApi.SetNamedShape(handle, sourceEntry, copy);
        }
        features.Add(definition.Id, record);
        WriteFeature(record);
        Wire(record, features, plan);
        WriteManifest(ReadManifest() with { Entries = features.Values.Select(x => x.Entry).Order(StringComparer.Ordinal).ToArray() });
        storage.Logbook(entry, 1);
        command.Commit();
        return definition.Id;
    }

    public void Update(ParametricFeatureDefinition definition)
    {
        Check(); ArgumentNullException.ThrowIfNull(definition);
        var features = ReadFeatures();
        using var command = BeginCommand("Edit parametric feature");
        UpdateCore(definition, features);
        command.Commit();
    }

    private void UpdateCore(ParametricFeatureDefinition definition, Dictionary<Guid, StoredFeature> features)
    {
        var old = Get(features, definition.Id);
        if (old.Definition.Kind != definition.Kind) throw new ArgumentException("Changing feature kind requires a new feature identity.");
        var proposed = old with { Definition = definition, DefinitionRevision = Guid.NewGuid(), Dirty = true, Touched = true, Error = null };
        features[definition.Id] = proposed;
        var plan = ParametricPlanning.Build(features.Values.Select(x => x.Definition).ToArray());
        RejectInvalidGraph(plan);
        WriteParameters(proposed);
        Wire(proposed, features, plan);
        storage.Logbook(proposed.Entry, 1);
        var dirty = Dependants([definition.Id], plan);
        foreach (Guid id in dirty)
        {
            features[id] = features[id] with { Dirty = true, State = ParametricExecutionState.NotExecuted, Error = null };
            WriteFeature(features[id]);
            storage.Logbook(features[id].Entry, features[id].Touched ? 1 : 2);
        }
    }

    public ParametricValue ReadParameter(Guid feature, string name)
    {
        var record = Get(ReadFeatures(), feature);
        return record.ParameterEntries.TryGetValue(name, out string? entry) ? storage.GetParameter(entry) : ParametricValue.Missing();
    }

    public string Save(string path, DocumentStorageFormat format)
    {
        Check(); ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!Path.HasExtension(path)) path += format switch
        {
            DocumentStorageFormat.BinOcaf => ".cbf", DocumentStorageFormat.BinXcaf => ".xbf",
            DocumentStorageFormat.XmlOcaf or DocumentStorageFormat.XmlXcaf => ".xml",
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
        if (parent is XdeDocument xde) return xde.Save(path, format);
        return ((OcafDocument)parent).Save(path, format);
    }
    public bool Undo() { Check(); return DocumentStateApi.Undo(handle); }
    public bool Redo() { Check(); return DocumentStateApi.Redo(handle); }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        ownedParent?.Dispose();
    }

    private void Check()
    {
        ObjectDisposedException.ThrowIf(disposed || handle.IsClosed || handle.IsInvalid, this);
        GC.KeepAlive(parent);
    }
    private string AddChild(string parentEntry, bool reserve = true)
    {
        NativeError.ThrowIfFailed(NativeMethods.AddOcafChild(handle, parentEntry, out int tag), "parametric_add_child");
        string entry = $"{parentEntry}:{tag}";
        // Empty OCAF labels do not survive storage. Every reserved schema node needs an attribute.
        if (reserve) storage.SetText(entry, "reserved", "1");
        return entry;
    }
    private T Read<T>(string entry, string key) => JsonSerializer.Deserialize<T>(storage.GetText(entry, key)
        ?? throw new InvalidDataException($"Missing parametric {key}."), JsonOptions) ?? throw new InvalidDataException($"Null parametric {key}.");
    private void Write<T>(string entry, string key, T value) => storage.SetText(entry, key, JsonSerializer.Serialize(value, JsonOptions));
    private GraphManifest ReadManifest()
    {
        Check(); var value = Read<GraphManifest>(RootEntry, "manifest");
        if (value.SchemaVersion != 1 || value.DocumentId == Guid.Empty || value.Entries is null || value.Entries.Length > 4096 || value.Entries.Distinct(StringComparer.Ordinal).Count() != value.Entries.Length)
            throw new NotSupportedException("Unsupported or malformed parametric document schema.");
        return value;
    }
    private void WriteManifest(GraphManifest manifest) => Write(RootEntry, "manifest", manifest);
    private Dictionary<Guid, StoredFeature> ReadFeatures()
    {
        Check(); Dictionary<Guid, StoredFeature> values = [];
        foreach (string entry in ReadManifest().Entries)
        {
            if (!IsDirectChild(entry, RootEntry)) throw new InvalidDataException("Feature lies outside its graph.");
            var feature = Read<StoredFeature>(entry, "feature");
            ValidateFeature(feature, entry);
            if (feature.Entry != entry || !values.TryAdd(feature.Definition.Id, feature)) throw new InvalidDataException("Invalid feature identity table.");
            var native = storage.Links(entry, false);
            if (native.Id != feature.NativeId || native.State != ParametricStorage.EncodeState(feature.State)) throw new InvalidDataException("Persisted function identity/state differs from its scope.");
        }
        var plan = ParametricPlanning.Build(values.Values.Select(x => x.Definition).ToArray());
        foreach (var feature in values.Values)
        {
            var dependencies = plan.Dependencies[feature.Definition.Id];
            if (dependencies.Any(x => !values.ContainsKey(x))
                || !storage.Links(feature.Entry, false).Links.Order().SequenceEqual(dependencies.Select(x => values[x].NativeId).Order()))
                throw new InvalidDataException("Persisted native dependencies differ from the feature definition.");
            if (!feature.Dirty && feature.State == ParametricExecutionState.Succeeded
                && !feature.InputRevisions.Keys.Order().SequenceEqual(dependencies.Order()))
                throw new InvalidDataException("A current result has an incomplete dependency revision table.");
        }
        return values;
    }
    private void WriteFeature(StoredFeature feature)
    {
        Write(feature.Entry, "feature", feature);
        storage.State(feature.Entry, feature.State);
    }
    private static bool IsDirectChild(string entry, string parentEntry) => entry is not null
        && entry.StartsWith(parentEntry + ":", StringComparison.Ordinal)
        && int.TryParse(entry.AsSpan(parentEntry.Length + 1), System.Globalization.NumberStyles.None,
            System.Globalization.CultureInfo.InvariantCulture, out int tag) && tag > 0;

    private static void ValidateFeature(StoredFeature feature, string entry)
    {
        string[] children = [feature.ParametersEntry, feature.ResultEntry, feature.ReferencesEntry,
            feature.HistoryEntry, feature.SelectorsEntry, feature.SourceEntry];
        if (feature.Definition is null || children.Any(x => !IsDirectChild(x, entry)) || children.Distinct(StringComparer.Ordinal).Count() != children.Length
            || feature.NativeId < 1 || feature.DefinitionRevision == Guid.Empty || !Enum.IsDefined(feature.State)
            || feature.InputRevisions is null || feature.ParameterEntries is null || feature.ParameterEntries.Count > 256
            || feature.ParameterEntries.Any(x => !feature.Definition.Parameters.ContainsKey(x.Key) || !IsDirectChild(x.Value, feature.ParametersEntry))
            || feature.ParameterEntries.Values.Distinct(StringComparer.Ordinal).Count() != feature.ParameterEntries.Count)
            throw new InvalidDataException("Malformed feature storage paths or revisions.");
    }
    private void WriteParameters(StoredFeature feature)
    {
        foreach (var pair in feature.Definition.Parameters)
        {
            if (!feature.ParameterEntries.TryGetValue(pair.Key, out string? entry))
                feature.ParameterEntries.Add(pair.Key, entry = AddChild(feature.ParametersEntry));
            storage.SetParameter(entry, pair.Value.Value ?? ParametricValue.Missing());
            Write(entry, "expression", pair.Value.Expression);
            storage.SetText(entry, "name", pair.Key);
        }
        foreach (string key in feature.ParameterEntries.Keys.Except(feature.Definition.Parameters.Keys).ToArray())
        {
            storage.SetParameter(feature.ParameterEntries[key], ParametricValue.Missing());
            feature.ParameterEntries.Remove(key);
        }
    }
    private void Wire(StoredFeature feature, Dictionary<Guid, StoredFeature> features, ParametricExecutionPlan plan)
    {
        storage.Rewire(feature.Entry, plan.Dependencies[feature.Definition.Id].Select(id => features[id].NativeId));
        DocumentStateApi.SetReferenceArray(handle, feature.ReferencesEntry,
            feature.Definition.Inputs.Select(x => features[x.FeatureId].ResultEntry).ToArray());
    }
    private static void RejectInvalidGraph(ParametricExecutionPlan plan)
    {
        var invalid = plan.Issues.FirstOrDefault(x => x.Code is "BlockedGraph" or "MissingInput" or "InputType");
        if (invalid is not null) throw new ArgumentException(invalid.Message);
    }
    private static HashSet<Guid> Dependants(IEnumerable<Guid> seeds, ParametricExecutionPlan plan)
    {
        HashSet<Guid> result = new(seeds);
        foreach (Guid id in plan.Order) if (plan.Dependencies[id].Any(result.Contains)) result.Add(id);
        return result;
    }
    private static StoredFeature Get(Dictionary<Guid, StoredFeature> features, Guid id) => features.TryGetValue(id, out var value)
        ? value : throw new ArgumentException("The feature is absent from this document.", nameof(id));
    private static Guid DriverId(ParametricFeatureKind kind) => new($"f8f12000-7021-4f71-8341-{(int)kind:000000000000}");
    private static ParametricFeatureSnapshot Snapshot(StoredFeature value) => new(value.Definition, value.Entry, value.NativeId,
        value.DefinitionRevision, value.ResultRevision, value.State, value.Dirty, value.Error);
    private Command BeginCommand(string name) { Check(); return new(handle, name); }

    private sealed class Command : IDisposable
    {
        private readonly OcafDocumentHandle document;
        private readonly string name;
        private bool active = true;
        internal Command(OcafDocumentHandle document, string name)
        {
            this.document = document; this.name = name;
            NativeError.ThrowIfFailed(NativeMethods.BeginOcafCommand(document), "parametric_begin_command");
        }
        internal void Commit() { DocumentStateApi.CommitNamedCommand(document, name); active = false; }
        public void Dispose()
        {
            if (active && !document.IsClosed) NativeError.ThrowIfFailed(NativeMethods.AbortOcafCommand(document), "parametric_abort_command");
            active = false;
        }
    }

    private sealed record GraphManifest(int SchemaVersion, Guid DocumentId, string[] Entries);
    private sealed record StoredFeature(ParametricFeatureDefinition Definition, string Entry, string ParametersEntry,
        string ResultEntry, string ReferencesEntry, string HistoryEntry, string SelectorsEntry, string SourceEntry, int NativeId,
        Guid DefinitionRevision, Guid? ResultRevision, Guid? ResultDefinitionRevision, Dictionary<Guid, Guid> InputRevisions,
        ParametricExecutionState State, bool Dirty, bool Touched, string? Error, Dictionary<string, string> ParameterEntries);
}

public sealed record ParametricFeatureSnapshot(ParametricFeatureDefinition Definition, string Entry, int FunctionId,
    Guid DefinitionRevision, Guid? ResultRevision, ParametricExecutionState State, bool Dirty, string? Error);
#pragma warning restore CS1591
