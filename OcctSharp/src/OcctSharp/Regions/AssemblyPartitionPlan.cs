namespace OcctSharp;

#pragma warning disable CS1591
public enum RegionAssemblyRulePolicy { PerOccurrence, SharedDefinitionRules }
public sealed record RegionAssemblyInput(int Index, string OccurrenceKey, string DefinitionKey, string RuleKey,
    string? Name, XdeColor? Color, string Fingerprint);

/// <summary>Captures each located occurrence independently. Sharing rule keys never collapses geometry.</summary>
public sealed class AssemblyPartitionPlan : IDisposable
{
    private readonly PartitionPlan plan;
    private readonly XdeDocument document;
    private readonly XdeLabel root;
    private readonly string[][] paths;
    private bool disposed;
    private AssemblyPartitionPlan(XdeDocument document, XdeLabel root, string[][] paths, RegionAssemblyRulePolicy policy,
        PartitionPlan plan, RegionAssemblyInput[] inputs)
    {
        this.document = document; this.root = root; this.paths = paths; this.plan = plan; Policy = policy; Inputs = Array.AsReadOnly(inputs);
    }
    public RegionAssemblyRulePolicy Policy { get; }
    public IReadOnlyList<RegionAssemblyInput> Inputs { get; }
    public static AssemblyPartitionPlan Capture(XdeDocument document, XdeLabel root,
        IReadOnlyList<IReadOnlyList<string>> occurrencePaths, RegionAssemblyRulePolicy policy = RegionAssemblyRulePolicy.PerOccurrence,
        PartitionOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(document); ArgumentNullException.ThrowIfNull(root); ArgumentNullException.ThrowIfNull(occurrencePaths);
        if (!Enum.IsDefined(policy) || occurrencePaths.Count is < 1 or > 128) throw new ArgumentException("Invalid assembly partition policy or count.");
        string[][] paths = occurrencePaths.Select(p => ScalarLawDefinition.Copy(p, 256)).ToArray();
        if (paths.Select(p => string.Join('/', p)).Distinct(StringComparer.Ordinal).Count() != paths.Length) throw new ArgumentException("Duplicate occurrence paths.");
        List<AssemblyOccurrenceResolution> resolved = []; List<RegionAssemblyInput> inputs = [];
        try
        {
            for (int i = 0; i < paths.Length; i++)
            {
                var item = document.ResolveOccurrencePath(root, paths[i]); resolved.Add(item);
                if (item.Definition.IsAssembly) throw new ArgumentException("Partition inputs must resolve to leaf definitions.");
                string key = string.Join('/', paths[i]);
                inputs.Add(new(i, key, item.Definition.Entry, policy == RegionAssemblyRulePolicy.PerOccurrence ? key : item.Definition.Entry,
                    item.Occurrence.Name ?? item.Definition.Name, item.Occurrence.Color ?? item.Definition.Color,
                    RepairSnapshot.ComputeFingerprint(item.LocatedShape)));
            }
            return new(document, root, paths, policy, PartitionPlan.Create(resolved.Select(r => r.LocatedShape).ToArray(), options), inputs.ToArray());
        }
        finally { foreach (var item in resolved) item.Dispose(); }
    }
    public RegionExpression ExpressionFor(string ruleKey)
    {
        ObjectDisposedException.ThrowIf(disposed, this); ArgumentNullException.ThrowIfNull(ruleKey);
        var indices = Inputs.Where(i => i.RuleKey == ruleKey).Select(i => i.Index).ToArray();
        if (indices.Length == 0) throw new ArgumentException("Unknown assembly rule key.");
        return indices.Select(RegionExpression.Input).Aggregate((a, b) => a.Union(b));
    }
    public PartitionResult Build(IReadOnlyList<RegionProgram>? programs = null)
    { ObjectDisposedException.ThrowIf(disposed, this); return plan.Build(programs); }
    public RegionProductSet Publish(XdeDocument target, PartitionResult result, IReadOnlyList<RegionProductDefinition> products, string name = "Assembly regions")
    {
        ObjectDisposedException.ThrowIf(disposed, this); ArgumentNullException.ThrowIfNull(result);
        if (result.PlanId != plan.Id || !IsCurrent()) throw new InvalidOperationException("Foreign partition or stale assembly context; recapture before publication.");
        return RegionProducts.Create(target, result, products, name, Inputs);
    }
    /// <summary>Explicitly recaptures edited placements/definitions, producing new immutable inputs and later new cell IDs.</summary>
    public AssemblyPartitionPlan Refresh()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return Capture(document, root, paths, Policy, plan.Options);
    }
    public bool IsCurrent()
    {
        ObjectDisposedException.ThrowIf(disposed, this); document.ThrowIfDisposed();
        for (int i = 0; i < paths.Length; i++)
        {
            using var current = document.ResolveOccurrencePath(root, paths[i]);
            if (current.Definition.Entry != Inputs[i].DefinitionKey || RepairSnapshot.ComputeFingerprint(current.LocatedShape) != Inputs[i].Fingerprint) return false;
        }
        return true;
    }
    public void Dispose() { if (disposed) return; disposed = true; plan.Dispose(); }
}
