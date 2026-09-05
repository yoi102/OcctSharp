namespace OcctSharp;

#pragma warning disable CS1591
/// <summary>Owns a private candidate snapshot after Q budget and exact unchanged-topology checks.</summary>
public sealed class LocalFeatureAcceptance : IDisposable
{
    private readonly RepairSnapshot snapshot;
    private bool disposed, accepted;
    private LocalFeatureAcceptance(RepairSnapshot snapshot, RepairBudgetCheck[] checks)
    { this.snapshot = snapshot; Checks = Array.AsReadOnly(checks); }
    public IReadOnlyList<RepairBudgetCheck> Checks { get; }
    public bool CanAccept => !disposed && !accepted && Checks.All(c => c.State is RepairCheckState.Passed or RepairCheckState.NotRequired);
    /// <summary>Requires a result tied to this exact snapshot; no positional or nearest-neighbour matching is used.</summary>
    public static LocalFeatureAcceptance Inspect(RepairSnapshot source, LocalFeatureResult result,
        RepairBudget? budget = null, IEnumerable<RepairSelection>? protectedTopology = null)
    {
        ArgumentNullException.ThrowIfNull(source); ArgumentNullException.ThrowIfNull(result); source.ThrowIfDisposed(); result.ThrowIfDisposed();
        if (result.Source != source.Identity || result.SourceFingerprint != source.Fingerprint)
            throw new ArgumentException("Acceptance requires the result's original source snapshot.");
        if (RepairSnapshot.ComputeFingerprint(source.Shape) != source.Fingerprint)
            throw new InvalidOperationException("The source geometry was changed after snapshot creation.");
        var root = result.RequireShape();
        if (RepairSnapshot.ComputeFingerprint(root) != result.ResultFingerprint)
            throw new InvalidOperationException("The local-feature result was changed after execution.");
        var selected = ScalarLawDefinition.Copy(protectedTopology ?? [], 100000);
        foreach (var item in selected) source.Validate(item);
        budget ??= new();
        foreach (double? limit in new[] { budget.MaximumTolerance, budget.MaximumToleranceGrowth, budget.MaximumRelativeAreaChange, budget.MaximumRelativeVolumeChange })
            if (limit is { } value && (!double.IsFinite(value) || value < 0)) throw new ArgumentOutOfRangeException(nameof(budget));
        var snapshot = RepairSnapshot.Create(root, source.Unit, checked(source.Identity.Revision + 1), source.Options);
        try
        {
            List<RepairBudgetCheck> checks = [.. ShapeRepair.CheckBudget(source.Metrics, snapshot.Metrics, budget)];
            foreach (var selection in selected.Distinct())
            {
                var matches = result.History.Where(h => h.Kind == LocalFeatureHistoryKind.Unchanged && h.Source is { ArgumentIndex: 0 } s
                    && s.PlanId == result.PlanId && s.TopologyIndex == selection.Index && h.Shape is not null).ToArray();
                bool same = false;
                if (matches.Length == 1)
                {
                    using var before = source.CopySubshape(selection);
                    // Canonicalize both standalone closures: the native history may
                    // still carry pcurves on adjacent faces outside this subshape.
                    using var after = RepairSnapshot.Create(matches[0].Shape!);
                    same = RepairSnapshot.ComputeFingerprint(before) == after.Fingerprint;
                }
                checks.Add(new($"protected-{selection.Index}", same ? RepairCheckState.Passed : RepairCheckState.Failed, same ? 1 : 0, 1));
            }
            return new(snapshot, checks.ToArray());
        }
        catch { snapshot.Dispose(); throw; }
    }
    public Shape Accept()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (!CanAccept) throw new InvalidOperationException("Local-feature acceptance is rejected or already consumed.");
        var copy = snapshot.CopyShape(); accepted = true; return copy;
    }
    public void Dispose() { if (disposed) return; disposed = true; snapshot.Dispose(); }
}
#pragma warning restore CS1591
