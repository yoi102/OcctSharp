using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591
public sealed record ParametricRecomputeReport(bool Succeeded, bool Cancelled, IReadOnlyList<Guid> Executed,
    IReadOnlyList<ParametricPlanIssue> Issues, IReadOnlyList<Guid> Pending);
public sealed record ParametricLogbook(bool Touched, bool Impacted, bool Valid, bool Done);

/// <summary>An independent result copy with explicit generation and last-good staleness.</summary>
public sealed class ParametricResult : IDisposable
{
    internal ParametricResult(Guid document, Guid feature, Guid revision, bool stale, ParametricOutputKind kind,
        Shape? shape, ParametricQuantity? scalar)
    { DocumentId = document; FeatureId = feature; Revision = revision; IsStale = stale; Kind = kind; Shape = shape; Scalar = scalar; }
    public Guid DocumentId { get; }
    public Guid FeatureId { get; }
    public Guid Revision { get; }
    public bool IsStale { get; }
    public ParametricOutputKind Kind { get; }
    public Shape? Shape { get; }
    public ParametricQuantity? Scalar { get; }
    public void Dispose() => Shape?.Dispose();
}

public sealed partial class ParametricDocument
{
    public ParametricResult GetResult(Guid feature, bool allowStale = false)
    {
        var values = ReadFeatures(); var value = Get(values, feature);
        bool stale = IsStale(value, values);
        if (value.ResultRevision is not { } revision || (stale && !allowStale))
            throw new InvalidOperationException("The result is absent or stale; explicitly request last-good access if needed.");
        Shape? shape = null;
        if (value.Definition.OutputKind != ParametricOutputKind.Scalar)
        {
            using var stored = RequiredShape(value.ResultEntry);
            shape = MeshTopology.CopyWithTriangulation(stored);
        }
        return new(Id, feature, revision, stale, value.Definition.OutputKind, shape,
            value.Definition.OutputKind == ParametricOutputKind.Scalar ? Read<ParametricQuantity>(value.ResultEntry, "scalar") : null);
    }

    public ParametricLogbook GetLogbook(Guid feature)
    {
        var values = ReadFeatures(); var value = Get(values, feature); int flags = storage.Logbook(value.Entry, 5);
        // Native valid/done markers describe its previous solve scope. An edited or
        // invalidated result must not be presented as currently valid just because
        // that logbook has not yet been cleared by the next recompute.
        bool current = !IsStale(value, values);
        return new((flags & 1) != 0, (flags & 2) != 0, current && (flags & 4) != 0, current && (flags & 8) != 0);
    }

    public ParametricRecomputeReport Recompute(ParametricRecomputeMode mode = ParametricRecomputeMode.Incremental,
        IReadOnlyList<Guid>? targets = null, CancellationToken cancellationToken = default) =>
        RecomputeCore(null, mode, targets, cancellationToken);

    /// <summary>Edits and publishes an entire successful recompute as one undoable command; failure rolls back the edit too.</summary>
    public ParametricRecomputeReport EditAndRecompute(ParametricFeatureDefinition definition,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return RecomputeCore(definition, ParametricRecomputeMode.Incremental, null, cancellationToken);
    }

    // The internal observer allows deterministic cancellation/fault validation at an actual
    // call boundary. Public execution never installs a callback into OCCT or the document.
    internal ParametricRecomputeReport RecomputeCore(ParametricFeatureDefinition? edit, ParametricRecomputeMode mode,
        IReadOnlyList<Guid>? targets, CancellationToken cancellationToken, Action<Guid>? afterEvaluation = null)
    {
        Check();
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        if ((mode == ParametricRecomputeMode.Targeted) != (targets is { Count: > 0 }))
            throw new ArgumentException("Only targeted mode accepts a nonempty result target set.");
        Dictionary<Guid, Candidate> candidates = []; List<Guid> executed = [];
        List<ParametricPlanIssue> issues = []; HashSet<Guid> selected = [];
        Guid? running = null; bool cancelled = false;
        var features = ReadFeatures();
        try
        {
            using var command = BeginCommand(edit is null ? "Recompute parametric graph" : "Edit and recompute parametric graph");
            if (edit is not null) UpdateCore(edit, features);
            var plan = ParametricPlanning.Build(features.Values.Select(x => x.Definition).ToArray());
            selected = mode == ParametricRecomputeMode.Targeted ? Ancestors(targets!, features, plan) : features.Keys.ToHashSet();
            issues.AddRange(plan.Issues.Where(x => selected.Contains(x.FeatureId)));
            if (issues.Count != 0) throw new RecomputeFailure();
            cancellationToken.ThrowIfCancellationRequested();
            storage.Logbook(RootEntry, 0);
            foreach (Guid id in plan.Order.Where(selected.Contains))
            {
                var value = features[id];
                if (mode != ParametricRecomputeMode.Full && !IsStale(value, features)
                    && !plan.Dependencies[id].Any(candidates.ContainsKey)) continue;
                running = id;
                cancellationToken.ThrowIfCancellationRequested();
                storage.Logbook(value.Entry, value.Touched ? 1 : 2);
                WriteFeature(value with { State = ParametricExecutionState.Executing });
                candidates.Add(id, Evaluate(value, features, candidates, plan));
                executed.Add(id);
                afterEvaluation?.Invoke(id);
                cancellationToken.ThrowIfCancellationRequested();
            }
            // All native algorithms have succeeded before any accepted result is replaced.
            foreach (Guid id in executed)
            {
                var value = features[id]; var candidate = candidates[id];
                Guid resultRevision = Guid.NewGuid();
                if (candidate.Shape is not null)
                {
                    using var previous = value.ResultRevision.HasValue ? DocumentStateApi.GetNamedShape(handle, value.ResultEntry) : null;
                    string evolution = AddChild(value.HistoryEntry);
                    if (previous is null) storage.Record(evolution, ParametricEvolutionKind.Primitive, [], [candidate.Shape]);
                    else storage.Record(evolution, ParametricEvolutionKind.Modified, [previous], [candidate.Shape]);
                    storage.Record(value.ResultEntry, ParametricEvolutionKind.Primitive, [], [candidate.Shape]);
                }
                if (candidate.Scalar is { } scalar) Write(value.ResultEntry, "scalar", scalar);
                List<StoredAlgorithmHistory> associations = [];
                foreach (var association in candidate.AlgorithmHistory)
                {
                    string entry = AddChild(value.HistoryEntry);
                    if (association.Shape is { } shape) storage.Record(entry, ParametricEvolutionKind.Primitive, [], [shape]);
                    associations.Add(new(association.SourceFeatureId, association.Kind, entry, association.Shape is not null));
                }
                Write(value.HistoryEntry, "algorithmHistory", associations);
                Write(value.HistoryEntry, "diagnostics", candidate.Diagnostics);
                var revisions = ReadHistoryRevisions(value);
                using (var history = DocumentStateApi.SnapshotLabel(handle, value.HistoryEntry))
                    revisions.Add(new(resultRevision, history.ChildEntries.ToArray()));
                Write(value.HistoryEntry, "revisions", revisions);
                value = value with { ResultRevision = resultRevision, ResultDefinitionRevision = value.DefinitionRevision,
                    InputRevisions = plan.Dependencies[id].ToDictionary(x => x, x => features[x].ResultRevision!.Value),
                    Dirty = false, Touched = false, State = ParametricExecutionState.Succeeded, Error = null };
                features[id] = value; WriteFeature(value); storage.Logbook(value.Entry, 3);
            }
            storage.Logbook(RootEntry, 4);
            command.Commit();
            return new(true, false, executed.AsReadOnly(), [], features.Values.Where(x => IsStale(x, features)).Select(x => x.Definition.Id).ToArray());
        }
        catch (OperationCanceledException) { cancelled = true; issues.Add(new(running ?? Guid.Empty, "Cancelled", "Cancelled between synchronous feature calls.")); }
        catch (RecomputeFailure) { }
        catch (Exception error) when (error is ArgumentException or InvalidOperationException or OcctException or NotSupportedException
            or ArithmeticException or InvalidCastException or System.Text.Json.JsonException)
        { issues.Add(new(running ?? edit?.Id ?? Guid.Empty, "ExecutionFailed", error.Message)); }
        finally { foreach (var candidate in candidates.Values) candidate.Dispose(); }

        // Failed runs publish diagnostics only, in a separate transaction after candidate/edit rollback.
        features = ReadFeatures();
        using (var failure = BeginCommand("Record failed parametric recompute"))
        {
            var plan = ParametricPlanning.Build(features.Values.Select(x => x.Definition).ToArray());
            var failed = issues.Select(x => x.FeatureId).Where(features.ContainsKey).ToHashSet();
            var blocked = Dependants(failed, plan);
            storage.Logbook(RootEntry, 0);
            foreach (Guid id in selected.Concat(blocked).Distinct().Where(features.ContainsKey))
            {
                if (!failed.Contains(id) && !blocked.Contains(id)) continue;
                var value = features[id] with { Dirty = true,
                    State = cancelled ? ParametricExecutionState.NotExecuted : failed.Contains(id) ? ParametricExecutionState.Failed : ParametricExecutionState.Blocked,
                    Error = failed.Contains(id) ? string.Join("; ", issues.Where(x => x.FeatureId == id).Select(x => x.Message)) : "A prerequisite failed." };
                features[id] = value; WriteFeature(value); storage.Logbook(value.Entry, failed.Contains(id) ? 1 : 2);
            }
            failure.Commit();
        }
        return new(false, cancelled, executed.AsReadOnly(), issues.AsReadOnly(), features.Values.Where(x => IsStale(x, features)).Select(x => x.Definition.Id).ToArray());
    }

    private static bool IsStale(StoredFeature value, Dictionary<Guid, StoredFeature> features) =>
        value.Dirty || value.State != ParametricExecutionState.Succeeded || value.ResultRevision is null
        || value.ResultDefinitionRevision != value.DefinitionRevision
        || value.InputRevisions.Any(x => !features.TryGetValue(x.Key, out var input) || input.Dirty || input.ResultRevision != x.Value);

    private static HashSet<Guid> Ancestors(IEnumerable<Guid> roots, Dictionary<Guid, StoredFeature> features, ParametricExecutionPlan plan)
    {
        HashSet<Guid> result = []; Stack<Guid> pending = new(roots);
        while (pending.TryPop(out Guid id))
        {
            _ = Get(features, id);
            if (result.Add(id)) foreach (Guid dependency in plan.Dependencies[id]) pending.Push(dependency);
        }
        return result;
    }

    private sealed class RecomputeFailure : Exception;
    private sealed class Candidate(Shape? shape, ParametricQuantity? scalar = null) : IDisposable
    {
        internal Shape? Shape { get; } = shape;
        internal ParametricQuantity? Scalar { get; } = scalar;
        internal List<ParametricAlgorithmHistory> AlgorithmHistory { get; } = [];
        internal List<string> Diagnostics { get; } = [];
        public void Dispose() { Shape?.Dispose(); foreach (var item in AlgorithmHistory) item.Dispose(); }
    }
    private sealed record StoredAlgorithmHistory(Guid SourceFeatureId, string Kind, string Entry, bool HasShape);
    private sealed record StoredHistoryRevision(Guid ResultRevision, string[] Entries);
}
#pragma warning restore CS1591
