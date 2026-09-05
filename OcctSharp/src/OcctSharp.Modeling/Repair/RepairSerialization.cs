using System.Text.Json;
using System.Text.Json.Serialization;

namespace OcctSharp;

#pragma warning disable CS1591

public sealed record RepairRecipeRecord(int Schema, RepairIdentity Source, string SourceFingerprint, string Unit,
    IReadOnlyList<RepairStep> Steps, IReadOnlyList<RepairSelection> Protected, RepairTolerancePolicy Tolerance,
    RepairBudget Budget, int MaximumTopology);
public sealed record RepairAuditRecord(int Schema, RepairRecipeRecord Recipe, RepairMetrics Before, RepairMetrics? After,
    bool Completed, bool Accepted, IReadOnlyList<RepairStageOutcome> Stages,
    IReadOnlyList<RepairHistoryRelation> History, IReadOnlyList<RepairBudgetCheck> BudgetChecks);

public static class RepairSerialization
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter() }
    };
    public static string SerializeRecipe(RepairPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan); return JsonSerializer.Serialize(Record(plan), Options);
    }
    /// <summary>Rebinds copied selectors only when topology fingerprint, unit and source revision match.</summary>
    public static RepairPlan DeserializeRecipe(string json, RepairSnapshot matchingSource)
    {
        ArgumentNullException.ThrowIfNull(matchingSource); matchingSource.ThrowIfDisposed(); ValidateJson(json);
        RepairRecipeRecord record = JsonSerializer.Deserialize<RepairRecipeRecord>(json, Options) ?? throw new ArgumentException("Missing recipe.");
        ValidateRecipeRecord(record);
        if (record.Schema != 1 || record.SourceFingerprint != matchingSource.Fingerprint || record.Unit != matchingSource.Unit
            || record.Source.Revision != matchingSource.Identity.Revision)
            throw new ArgumentException("Recipe schema, topology fingerprint, units or source revision do not match.");
        RepairStep[] steps = record.Steps.Select(step => new RepairStep(step.Name,
            step.Stage is TopologyEditRepair edit ? new TopologyEditRepair(edit.Edits.Select(value =>
                new RepairTopologyEdit(Rebind(value.Target), value.Replacement is { } replacement ? Rebind(replacement) : null)).ToArray()) : step.Stage,
            step.Selection.Select(Rebind).ToArray(), step.Control)).ToArray();
        return new(matchingSource, steps, record.Protected.Select(Rebind).ToArray(), record.Tolerance, record.Budget, record.MaximumTopology);
        RepairSelection Rebind(RepairSelection selection)
        {
            if (selection.Source != record.Source) throw new ArgumentException("Recipe contains a foreign selection.");
            return matchingSource.Select(selection.Index);
        }
    }
    public static string SerializeAudit(RepairPreview preview)
    {
        ArgumentNullException.ThrowIfNull(preview);
        return JsonSerializer.Serialize(new RepairAuditRecord(1, Record(preview.Plan), preview.Before,
            preview.Result?.Metrics, preview.Completed, preview.IsAccepted, preview.Stages, preview.History, preview.BudgetChecks), Options);
    }
    public static RepairAuditRecord DeserializeAudit(string json)
    {
        ValidateJson(json); RepairAuditRecord record = JsonSerializer.Deserialize<RepairAuditRecord>(json, Options) ?? throw new ArgumentException("Missing audit.");
        if (record.Schema != 1) throw new ArgumentException("Unsupported repair audit schema.");
        if (record.Recipe is null || record.Before is null || record.Stages is null || record.History is null || record.BudgetChecks is null)
            throw new ArgumentException("Incomplete repair audit.");
        ValidateRecipeRecord(record.Recipe);
        if (record.Stages.Count != record.Recipe.Steps.Count || record.Before.TopologyCount < 1
            || record.Completed != (record.After is not null)
            || record.Stages.Where((stage, index) => stage is null || stage.Index != index
                || stage.Name != record.Recipe.Steps[index].Name || !Enum.IsDefined(stage.State)).Any()
            || record.Completed && record.Stages.Any(stage => stage.State == RepairStageState.Failed)
            || record.BudgetChecks.Any(check => check is null || !Enum.IsDefined(check.State))
            || record.Accepted && (!record.Completed || record.BudgetChecks.Count == 0
                || record.BudgetChecks.Any(check => check.State is not (RepairCheckState.Passed or RepairCheckState.NotRequired))))
            throw new ArgumentException("Inconsistent repair audit outcomes.");
        foreach (RepairHistoryRelation relation in record.History)
            if (relation is null || !Enum.IsDefined(relation.Kind) || relation.Source.Source != record.Recipe.Source
                || (uint)relation.Source.Index >= record.Before.TopologyCount
                || relation.Result is { } target && (record.After is null || (uint)target.Index >= record.After.TopologyCount)
                || (relation.Kind is RepairRelationKind.Deleted or RepairRelationKind.Unknown) != (relation.Result is null))
                throw new ArgumentException("Invalid repair audit topology correspondence.");
        return record;
    }
    private static void ValidateRecipeRecord(RepairRecipeRecord record)
    {
        if (record.Schema != 1 || record.Source.SnapshotId == Guid.Empty || record.Source.Revision < 0
            || record.SourceFingerprint is null || record.SourceFingerprint.Length != 64
            || !record.SourceFingerprint.All(Uri.IsHexDigit) || string.IsNullOrWhiteSpace(record.Unit)
            || record.Steps is null || record.Steps.Count is < 1 or > 256 || record.Steps.Any(step => step is null)
            || record.Protected is null || record.Tolerance is null || record.Budget is null
            || record.MaximumTopology is < 1 or > 1000000)
            throw new ArgumentException("Incomplete or invalid repair recipe.");
    }
    private static RepairRecipeRecord Record(RepairPlan plan) => new(1, plan.Source, plan.SourceFingerprint, plan.Unit,
        plan.Steps, plan.Protected, plan.Tolerance, plan.Budget, plan.MaximumTopology);
    private static void ValidateJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        if (json.Length > 16777216) throw new ArgumentException("Repair record exceeds the 16 MiB limit.", nameof(json));
    }
}
