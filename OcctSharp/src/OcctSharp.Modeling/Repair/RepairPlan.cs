namespace OcctSharp;

#pragma warning disable CS1591

/// <summary>A frozen, source-bound recipe. Failure policy is always all-or-nothing.</summary>
public sealed class RepairPlan
{
    public RepairIdentity Source { get; }
    public string SourceFingerprint { get; }
    public string Unit { get; }
    public IReadOnlyList<RepairStep> Steps { get; }
    public IReadOnlyList<RepairSelection> Protected { get; }
    public RepairTolerancePolicy Tolerance { get; }
    public RepairBudget Budget { get; }
    public int MaximumTopology { get; }

    public RepairPlan(RepairSnapshot source, IReadOnlyList<RepairStep> steps,
        IReadOnlyList<RepairSelection>? protectedShapes = null, RepairTolerancePolicy? tolerance = null,
        RepairBudget? budget = null, int maximumTopology = 100000)
    {
        ArgumentNullException.ThrowIfNull(source); source.ThrowIfDisposed(); ArgumentNullException.ThrowIfNull(steps);
        if (steps.Count is < 1 or > 256) throw new ArgumentOutOfRangeException(nameof(steps));
        if (maximumTopology is < 1 or > 1000000) throw new ArgumentOutOfRangeException(nameof(maximumTopology));
        Source = source.Identity; Unit = source.Unit; SourceFingerprint = source.Fingerprint;
        Steps = Array.AsReadOnly(steps.ToArray()); Protected = Array.AsReadOnly(protectedShapes?.ToArray() ?? []);
        Tolerance = tolerance ?? new(); Budget = budget ?? new(); MaximumTopology = maximumTopology;
        RepairStage.Positive(Tolerance.Minimum); RepairStage.Positive(Tolerance.Maximum);
        if (Tolerance.Maximum < Tolerance.Minimum) throw new ArgumentException("Maximum tolerance is below minimum tolerance.");
        foreach (double? limit in new[] { Budget.MaximumTolerance, Budget.MaximumToleranceGrowth,
            Budget.MaximumRelativeAreaChange, Budget.MaximumRelativeVolumeChange })
            if (limit.HasValue && (!double.IsFinite(limit.Value) || limit < 0)) throw new ArgumentOutOfRangeException(nameof(budget));
        ValidateSelection(Protected);
        HashSet<string> names = new(StringComparer.Ordinal);
        foreach (RepairStep step in Steps)
        {
            ArgumentNullException.ThrowIfNull(step);
            if (!names.Add(step.Name)) throw new ArgumentException("Repair stage names must be unique.", nameof(steps));
            _ = step.Stage.ToRaw(Tolerance, MaximumTopology); ValidateSelection(step.Selection);
            if (step.Stage is not TopologyEditRepair edit) continue;
            if (step.Selection.Count != 0) throw new ArgumentException("An edit stage selects its own targets.");
            ValidateSelection(edit.Edits.Select(value => value.Target).ToArray());
            HashSet<int> targets = edit.Edits.Select(value => value.Target.Index).ToHashSet();
            foreach (RepairTopologyEdit change in edit.Edits)
            {
                if (change.Replacement is not { } replacement) continue;
                source.Validate(replacement);
                if (source.Topology[change.Target.Index].Kind != source.Topology[replacement.Index].Kind)
                    throw new ArgumentException("Topology edit replacement kind differs from its target.");
                if (targets.Contains(replacement.Index)) throw new ArgumentException("Conflicting or cyclic edit dependencies.");
            }
        }
        void ValidateSelection(IReadOnlyList<RepairSelection> selection)
        {
            HashSet<int> indices = [];
            foreach (RepairSelection value in selection)
            {
                source.Validate(value);
                if (!indices.Add(value.Index)) throw new ArgumentException("Repair selections must be unique.");
            }
        }
    }
}
