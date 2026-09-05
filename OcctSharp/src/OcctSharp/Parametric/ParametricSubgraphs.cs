namespace OcctSharp;

#pragma warning disable CS1591
public sealed partial class ParametricDocument
{
    public IReadOnlyList<Guid> Delete(IReadOnlyList<Guid> features, ParametricDeletePolicy policy = ParametricDeletePolicy.RejectDependants)
    {
        ArgumentNullException.ThrowIfNull(features); Check();
        if (!Enum.IsDefined(policy)) throw new ArgumentOutOfRangeException(nameof(policy));
        var values = ReadFeatures(); var plan = ParametricPlanning.Build(values.Values.Select(x => x.Definition).ToArray());
        var selected = features.ToHashSet(); foreach (Guid id in selected) _ = Get(values, id);
        var closure = Dependants(selected, plan);
        if (policy == ParametricDeletePolicy.RejectDependants && !closure.SetEquals(selected))
            throw new InvalidOperationException("The deletion would leave dependant features dangling.");
        var order = plan.Order.Reverse().Where(closure.Contains).ToArray();
        using var command = BeginCommand("Delete parametric subgraph");
        foreach (Guid id in order) { storage.Remove(values[id].Entry); values.Remove(id); }
        WriteManifest(ReadManifest() with { Entries = values.Values.Select(x => x.Entry).Order(StringComparer.Ordinal).ToArray() });
        command.Commit(); return Array.AsReadOnly(order);
    }

    /// <summary>Relocates one closed subgraph inside this document. External dependencies are never copied implicitly.</summary>
    public IReadOnlyDictionary<Guid, Guid> Duplicate(IReadOnlyList<Guid> features,
        ParametricExternalReferencePolicy externalReferences = ParametricExternalReferencePolicy.Reject)
    {
        ArgumentNullException.ThrowIfNull(features); Check();
        if (!Enum.IsDefined(externalReferences)) throw new ArgumentOutOfRangeException(nameof(externalReferences));
        var values = ReadFeatures(); var plan = ParametricPlanning.Build(values.Values.Select(x => x.Definition).ToArray());
        var selected = features.ToHashSet();
        if (selected.Count is < 1 or > 2048 || selected.Count + values.Count > 4096) throw new ArgumentException("Invalid or excessive duplicate graph size.");
        foreach (Guid id in selected) _ = Get(values, id);
        if (externalReferences == ParametricExternalReferencePolicy.Reject && selected.Any(id => plan.Dependencies[id].Any(x => !selected.Contains(x))))
            throw new InvalidOperationException("The subgraph contains external dependencies.");
        Guid[] order = plan.Order.Where(selected.Contains).ToArray();
        var ids = order.ToDictionary(x => x, _ => Guid.NewGuid());
        using var command = BeginCommand("Duplicate parametric subgraph");
        string[] sources = order.Select(x => values[x].Entry).ToArray();
        string[] destinations = order.Select(_ => AddChild(RootEntry, reserve: false)).ToArray();
        storage.Relocate(sources, destinations, externalReferences == ParametricExternalReferencePolicy.Retain);
        for (int i = 0; i < order.Length; i++)
        {
            var old = values[order[i]];
            string Path(string entry) => destinations[i] + entry[old.Entry.Length..];
            Guid Map(Guid id) => ids.GetValueOrDefault(id, id);
            ParametricExpression Expression(ParametricExpression value) => new(value.Kind, value.Literal,
                value.Reference is { } r ? new(Map(r.FeatureId), r.Name) : null, value.Arguments.Select(Expression).ToArray());
            var definition = new ParametricFeatureDefinition(ids[order[i]], old.Definition.Name, old.Definition.Kind,
                old.Definition.Parameters.ToDictionary(x => x.Key, x => x.Value.Expression is { } expression
                    ? ParametricParameter.FromExpression(Expression(expression)) : x.Value, StringComparer.Ordinal),
                old.Definition.Inputs.Select(x => x with { FeatureId = Map(x.FeatureId) }).ToArray(), old.Definition.Recipe);
            var copy = old with { Definition = definition, Entry = destinations[i], ParametersEntry = Path(old.ParametersEntry),
                ResultEntry = Path(old.ResultEntry), ReferencesEntry = Path(old.ReferencesEntry), HistoryEntry = Path(old.HistoryEntry),
                SelectorsEntry = Path(old.SelectorsEntry), SourceEntry = Path(old.SourceEntry), NativeId = storage.Register(destinations[i], DriverId(definition.Kind)),
                DefinitionRevision = Guid.NewGuid(), ResultRevision = null, ResultDefinitionRevision = null,
                InputRevisions = [], State = ParametricExecutionState.NotExecuted, Dirty = true, Touched = true, Error = null,
                ParameterEntries = old.ParameterEntries.ToDictionary(x => x.Key, x => Path(x.Value), StringComparer.Ordinal) };
            var selectors = ReadSelections(old).Select(x => x with { FeatureId = definition.Id, Id = Guid.NewGuid(), Entry = Path(x.Entry) }).ToList();
            Write(copy.SelectorsEntry, "selections", selectors);
            Write(copy.HistoryEntry, "revisions", Array.Empty<StoredHistoryRevision>());
            foreach (var selection in selectors) Write(selection.Entry, "selection", selection);
            if (storage.GetText(old.HistoryEntry, "algorithmHistory") is not null)
            {
                var history = Read<StoredAlgorithmHistory[]>(old.HistoryEntry, "algorithmHistory");
                if (history.Any(x => !IsDirectChild(x.Entry, old.HistoryEntry))) throw new InvalidDataException("Invalid source history label.");
                Write(copy.HistoryEntry, "algorithmHistory", history.Select(x => x with
                { SourceFeatureId = Map(x.SourceFeatureId), Entry = Path(x.Entry) }).ToArray());
            }
            values.Add(definition.Id, copy); WriteParameters(copy); WriteFeature(copy);
        }
        var copiedPlan = ParametricPlanning.Build(values.Values.Select(x => x.Definition).ToArray());
        RejectInvalidGraph(copiedPlan);
        foreach (Guid id in order) Wire(values[ids[id]], values, copiedPlan);
        WriteManifest(ReadManifest() with { Entries = values.Values.Select(x => x.Entry).Order(StringComparer.Ordinal).ToArray() });
        command.Commit();
        return new System.Collections.ObjectModel.ReadOnlyDictionary<Guid, Guid>(ids);
    }
}
#pragma warning restore CS1591
