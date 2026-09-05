namespace OcctSharp;

#pragma warning disable CS1591

/// <summary>Owns a private preview. Acceptance returns a new owner, never an intermediate stage shape.</summary>
public sealed class RepairPreview : IDisposable
{
    private bool disposed, accepted;
    internal RepairPreview(RepairPlan plan, RepairMetrics before, RepairSnapshot? result,
        IReadOnlyList<RepairStageOutcome> stages, IReadOnlyList<RepairHistoryRelation> history,
        IReadOnlyList<RepairBudgetCheck> budget, bool completed) =>
        (Plan, Before, Result, Stages, History, BudgetChecks, Completed) = (plan, before, result, stages, history, budget, completed);
    public RepairPlan Plan { get; }
    public RepairMetrics Before { get; }
    public RepairSnapshot? Result { get; }
    public IReadOnlyList<RepairStageOutcome> Stages { get; }
    public IReadOnlyList<RepairHistoryRelation> History { get; }
    public IReadOnlyList<RepairBudgetCheck> BudgetChecks { get; }
    public bool Completed { get; }
    public bool CanAccept => !disposed && !accepted && Completed && Result is not null
        && BudgetChecks.All(value => value.State is RepairCheckState.Passed or RepairCheckState.NotRequired);
    public bool IsAccepted => accepted;
    public Shape Accept()
    {
        EnsureAcceptable(); Shape shape = Result!.CopyShape(); accepted = true; return shape;
    }
    internal void EnsureAcceptable()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (!CanAccept) throw new InvalidOperationException("Repair is failed, over budget, unverified, or already accepted.");
    }
    internal void MarkAccepted() { EnsureAcceptable(); accepted = true; }
    public void Dispose() { if (disposed) return; disposed = true; Result?.Dispose(); }
}
