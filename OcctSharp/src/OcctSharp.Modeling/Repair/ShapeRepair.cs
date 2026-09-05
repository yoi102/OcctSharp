using OcctSharp.Interop;
using System.Collections.ObjectModel;

namespace OcctSharp;

#pragma warning disable CS1591

public static class ShapeRepair
{
    public static RepairPreview Preview(RepairSnapshot source, RepairPlan plan)
    {
        ArgumentNullException.ThrowIfNull(source); ArgumentNullException.ThrowIfNull(plan); source.ThrowIfDisposed();
        if (plan.Source != source.Identity || plan.Unit != source.Unit) throw new ArgumentException("The recipe belongs to a foreign or stale snapshot.");
        List<RepairStageOutcome> outcomes = [];
        RepairSnapshot current = source;
        IReadOnlyList<RepairHistoryRelation> history = Array.AsReadOnly(source.Topology.Select(value =>
            new RepairHistoryRelation(value.Selection, value.Selection, RepairRelationKind.Unchanged)).ToArray());
        bool failed = false;
        try
        {
            for (int index = 0; index < plan.Steps.Count; ++index)
            {
                RepairStep step = plan.Steps[index];
                if (failed || step.Control == RepairControl.Off)
                {
                    outcomes.Add(new(index, step.Name, RepairStageState.Skipped, failed ? "Previous stage failed; atomic recipe stopped." : "Disabled explicitly.", null, []));
                    continue;
                }
                RepairSnapshot? next = null;
                try
                {
                    int[] selected = (step.Stage is TopologyEditRepair edit ? edit.Edits.Select(value => value.Target) : step.Selection)
                        .Select(Resolve).ToArray();
                    int[] protectedIndices = plan.Protected.Select(Resolve).ToArray();
                    RepairStageRaw raw = step.Stage.ToRaw(plan.Tolerance, plan.MaximumTopology);
                    using RepairResultHandle result = Execute(current, raw, selected, protectedIndices, step.Stage as TopologyEditRepair, Resolve);
                    NativeError.ThrowIfFailed(NativeMethods.RepairResultShape(result, out nint shape), "repair_result_shape");
                    RepairIdentity nextIdentity = new(Guid.NewGuid(), checked(current.Identity.Revision + 1));
                    next = RepairSnapshot.Own(ShapeFactory.FromNativeHandle(shape, "repair_result_shape"), source.Unit, nextIdentity, source.Options);
                    IReadOnlyList<RepairHistoryRelation> stageHistory = ReadHistory(result, current.Identity, nextIdentity);
                    IReadOnlyList<RepairFinding> findings = ReadFindings(result, current.Identity, nextIdentity);
                    history = Compose(history, stageHistory);
                    bool applicable = !findings.Any(value => value.Kind == RepairFindingKind.NotApplicable);
                    outcomes.Add(new(index, step.Name, applicable ? RepairStageState.Completed : RepairStageState.Skipped,
                        applicable ? "Completed on a private copy." : "No topology of the requested kind.", next.Metrics, findings));
                    if (!ReferenceEquals(current, source)) current.Dispose();
                    current = next; next = null;
                }
                catch (Exception error) when (error is ArgumentException or InvalidOperationException or OcctException or InvalidCastException)
                {
                    failed = true;
                    outcomes.Add(new(index, step.Name, RepairStageState.Failed, error.Message, null, []));
                }
                finally { next?.Dispose(); }
            }
            if (failed)
            {
                // No intermediate shape leaves this method after any stage failure.
                return new(plan, source.Metrics, null, outcomes.AsReadOnly(), [], [], false);
            }
            if (ReferenceEquals(current, source))
            {
                RepairIdentity identity = new(Guid.NewGuid(), checked(source.Identity.Revision + 1));
                current = RepairSnapshot.Own(source.CopyShape(), source.Unit, identity, source.Options);
                history = Array.AsReadOnly(history.Select(value => value with { Result = new RepairSelection(identity, value.Source.Index) }).ToArray());
            }
            IReadOnlyList<RepairBudgetCheck> budget = CheckBudget(source.Metrics, current.Metrics, plan.Budget);
            RepairPreview preview = new(plan, source.Metrics, current, outcomes.AsReadOnly(), history, budget, true);
            current = source; // ownership moves to the preview, not to the source
            return preview;
        }
        finally { if (!ReferenceEquals(current, source)) current.Dispose(); }

        int Resolve(RepairSelection selection)
        {
            source.Validate(selection);
            RepairHistoryRelation[] candidates = history.Where(value => value.Source == selection).ToArray();
            if (candidates.Length != 1 || candidates[0].Result is null || candidates[0].Kind is RepairRelationKind.Deleted or RepairRelationKind.Unknown)
                throw new InvalidOperationException("A selected/protected topology has no unambiguous current-stage correspondence.");
            return candidates[0].Result!.Value.Index;
        }
    }

    private static unsafe RepairResultHandle Execute(RepairSnapshot source, RepairStageRaw stage, int[] selected,
        int[] protectedIndices, TopologyEditRepair? edit, Func<RepairSelection, int> resolve)
    {
        List<Shape> owned = []; nint[] replacements = edit is null ? [] : new nint[edit.Edits.Count];
        try
        {
            if (edit is not null)
                for (int i = 0; i < edit.Edits.Count; ++i)
                {
                    if (edit.Edits[i].Replacement is not { } replacement) continue;
                    Shape shape = source.CopySubshape(source.Select(resolve(replacement))); owned.Add(shape);
                    replacements[i] = shape.Handle.DangerousGetHandle();
                }
            fixed (int* selectedPointer = selected)
            fixed (int* protectedPointer = protectedIndices)
            fixed (nint* replacementPointer = replacements)
            {
                NativeError.ThrowIfFailed(NativeMethods.RepairExecute(source.Shape.Handle, in stage, selectedPointer, selected.Length,
                    protectedPointer, protectedIndices.Length, replacementPointer, replacements.Length, out nint result), "repair_execute");
                if (result == 0) throw new InvalidOperationException("Native repair returned a null result.");
                return new(result);
            }
        }
        finally { foreach (Shape shape in owned) shape.Dispose(); }
    }
    private static unsafe ReadOnlyCollection<RepairHistoryRelation> ReadHistory(RepairResultHandle result, RepairIdentity source, RepairIdentity target)
    {
        NativeError.ThrowIfFailed(NativeMethods.RepairResultHistory(result, null, 0, out int count), "repair_history_count");
        RepairRelationRaw[] values = new RepairRelationRaw[count];
        fixed (RepairRelationRaw* pointer = values)
            NativeError.ThrowIfFailed(NativeMethods.RepairResultHistory(result, pointer, count, out count), "repair_history");
        return Array.AsReadOnly(values.Select(value => new RepairHistoryRelation(new(source, value.SourceIndex),
            value.ResultIndex >= 0 ? new(target, value.ResultIndex) : null, (RepairRelationKind)value.Kind)).Distinct().ToArray());
    }
    private static unsafe ReadOnlyCollection<RepairFinding> ReadFindings(RepairResultHandle result, RepairIdentity source, RepairIdentity target)
    {
        NativeError.ThrowIfFailed(NativeMethods.RepairResultFindings(result, null, 0, out int count), "repair_findings_count");
        RepairFindingRaw[] values = new RepairFindingRaw[count];
        fixed (RepairFindingRaw* pointer = values)
            NativeError.ThrowIfFailed(NativeMethods.RepairResultFindings(result, pointer, count, out count), "repair_findings");
        return Array.AsReadOnly(values.Select(value => RepairSnapshot.Convert(value, value.Kind is >= 12 and <= 16 ? target : source)).ToArray());
    }
    internal static IReadOnlyList<RepairHistoryRelation> Compose(IReadOnlyList<RepairHistoryRelation> previous,
        IReadOnlyList<RepairHistoryRelation> next)
    {
        List<RepairHistoryRelation> result = [];
        ILookup<RepairSelection, RepairHistoryRelation> lookup = next.ToLookup(value => value.Source);
        foreach (RepairHistoryRelation relation in previous)
        {
            if (relation.Result is null || relation.Kind is RepairRelationKind.Deleted or RepairRelationKind.Unknown)
            { result.Add(relation with { Result = null }); continue; }
            RepairHistoryRelation[] successors = lookup[relation.Result.Value].ToArray();
            if (successors.Length == 0) { result.Add(new(relation.Source, null, RepairRelationKind.Unknown)); continue; }
            foreach (RepairHistoryRelation successor in successors)
            {
                RepairRelationKind kind = successor.Kind is RepairRelationKind.Deleted or RepairRelationKind.Unknown ? successor.Kind
                    : relation.Kind == RepairRelationKind.Generated || successor.Kind == RepairRelationKind.Generated ? RepairRelationKind.Generated
                    : relation.Kind == RepairRelationKind.Modified || successor.Kind == RepairRelationKind.Modified ? RepairRelationKind.Modified : RepairRelationKind.Unchanged;
                result.Add(new(relation.Source, successor.Result, kind));
            }
        }
        return Array.AsReadOnly(result.Distinct().ToArray());
    }
    internal static ReadOnlyCollection<RepairBudgetCheck> CheckBudget(RepairMetrics before, RepairMetrics after, RepairBudget budget)
    {
        List<RepairBudgetCheck> checks = [];
        checks.Add(new("validity", !budget.RequireValid ? RepairCheckState.NotRequired : after.IsValid ? RepairCheckState.Passed : RepairCheckState.Failed,
            after.IsValid ? 1 : 0, budget.RequireValid ? 1 : null));
        Check("maximum-tolerance", after.MaximumTolerance, budget.MaximumTolerance);
        Check("tolerance-growth", before.MaximumTolerance > 0 ? after.MaximumTolerance / before.MaximumTolerance : null, budget.MaximumToleranceGrowth);
        Check("relative-area-change", Drift(before.Area, after.Area), budget.MaximumRelativeAreaChange);
        Check("relative-volume-change", Drift(before.Volume, after.Volume), budget.MaximumRelativeVolumeChange);
        return checks.AsReadOnly();
        void Check(string name, double? value, double? limit) => checks.Add(new(name,
            limit is null ? RepairCheckState.NotRequired : value is null || !double.IsFinite(value.Value) ? RepairCheckState.Unavailable
                : value <= limit ? RepairCheckState.Passed : RepairCheckState.Failed, value, limit));
        static double? Drift(double? initial, double? final) => initial.HasValue && final.HasValue
            ? initial == 0 ? final == 0 ? 0 : null : Math.Abs(final.Value - initial.Value) / Math.Abs(initial.Value) : null;
    }
}
