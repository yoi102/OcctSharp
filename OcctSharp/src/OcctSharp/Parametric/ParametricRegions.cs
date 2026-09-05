namespace OcctSharp;

#pragma warning disable CS1591
public sealed record ParametricRegionRule(IReadOnlyList<int> Expression, int Material = 0,
    RegionRuleAction Action = RegionRuleAction.Add, int? Dimension = null, double? MaximumMeasure = null);
public sealed record ParametricRegionProgram(string Key, IReadOnlyList<ParametricRegionRule> Rules,
    bool RemoveInternalBoundaries = false, bool MakeContainers = false);
public sealed record ParametricPartitionRecipe(IReadOnlyList<ParametricRegionProgram> Programs,
    PartitionOptions? Options = null, RegionPrecisionPolicy? Acceptance = null) : ParametricRecipe;
public sealed record ParametricVolumeRecipe(VolumeConstructionOptions? Options = null, bool RequireAtLeastOneSolid = true) : ParametricRecipe;

public sealed partial class ParametricDocument
{
    private static Candidate EvaluateRegions(ParametricFeatureDefinition definition, Func<string, Shape> input)
    {
        Shape[] shapes = definition.Inputs.Select(i => input(i.Name)).ToArray();
        Dictionary<string, Shape> outputs = new(StringComparer.Ordinal); Candidate? candidate = null;
        try
        {
            if (definition.Kind == ParametricFeatureKind.Partition)
            {
                var recipe = Recipe<ParametricPartitionRecipe>(definition);
                ArgumentNullException.ThrowIfNull(recipe.Programs);
                if (recipe.Programs.Count is < 1 or > 128) throw new ArgumentException("Partition feature requires one to 128 named outputs.");
                var programs = recipe.Programs.Select(p => new RegionProgram(p.Key, p.Rules.Select(r => new RegionRule(
                    RegionExpression.FromTokens(r.Expression), r.Material, r.Action, r.Dimension, r.MaximumMeasure)).ToArray(),
                    p.RemoveInternalBoundaries, p.MakeContainers)).ToArray();
                using var plan = PartitionPlan.Create(shapes, recipe.Options); using var result = plan.Build(programs);
                var verdict = result.EvaluatePrecision(recipe.Acceptance);
                if (!verdict.Accepted) throw new InvalidOperationException(string.Join("; ", verdict.Reasons));
                foreach (var key in result.OutputKeys) outputs.Add(key, result.CopyOutput(key));
                using var combined = ShapeFactory.CreateCompound(outputs.Values.ToArray());
                candidate = new(Share(combined));
                candidate.Diagnostics.Add(System.Text.Json.JsonSerializer.Serialize(result.Conservation));
                candidate.Diagnostics.Add($"Partition revision {result.Revision}; cell IDs are scoped to this evaluation, not persistent selectors.");
            }
            else
            {
                var recipe = Recipe<ParametricVolumeRecipe>(definition);
                using var plan = VolumeConstructionPlan.Create(shapes, recipe.Options); using var result = plan.Build();
                if (recipe.RequireAtLeastOneSolid && result.Volumes.Count == 0) throw new InvalidOperationException("Required bounded volumes were not constructed.");
                candidate = new(result.CopyResult());
                foreach (var volume in result.Volumes) outputs.Add($"volume-{volume.Id.Index}", result.CopyVolume(volume.Id));
                candidate.Diagnostics.Add(result.Diagnostics.Message);
                candidate.Diagnostics.Add($"Helper box excluded: {result.HelperBoxExcluded}; solid count: {result.Volumes.Count}.");
            }
            foreach (var output in outputs) candidate.RegionOutputs.Add(output.Key, output.Value);
            outputs.Clear(); return candidate;
        }
        catch { candidate?.Dispose(); foreach (var shape in outputs.Values) shape.Dispose(); throw; }
    }

    /// <summary>Lists the accepted generation's named region outputs; stale results require explicit opt-in.</summary>
    public IReadOnlyList<string> GetRegionOutputKeys(Guid feature, bool allowStale = false)
    {
        var values = ReadFeatures(); var value = Get(values, feature);
        if (value.ResultRevision is null || (!allowStale && IsStale(value, values))) throw new InvalidOperationException("Region results are absent or stale.");
        if (value.Definition.Kind is not (ParametricFeatureKind.Partition or ParametricFeatureKind.VolumeConstruction))
            throw new ArgumentException("Feature does not have named region outputs.");
        return Array.AsReadOnly(ReadRegionOutputs(value).Keys.Order(StringComparer.Ordinal).ToArray());
    }

    /// <summary>Copies one named output from the same atomic generation as the complete feature result.</summary>
    public ParametricResult GetRegionOutput(Guid feature, string key, bool allowStale = false)
    {
        ArgumentNullException.ThrowIfNull(key); _ = GetRegionOutputKeys(feature, allowStale);
        var values = ReadFeatures(); var value = Get(values, feature);
        var entries = ReadRegionOutputs(value);
        if (!entries.TryGetValue(key, out string? entry)) throw new ArgumentException("Unknown named region output.");
        using var stored = RequiredShape(entry);
        return new(Id, feature, value.ResultRevision!.Value, IsStale(value, values), ParametricOutputKind.ExactShape,
            MeshTopology.CopyWithTriangulation(stored), null);
    }

    private Dictionary<string, string> ReadRegionOutputs(StoredFeature value)
    {
        var entries = Read<Dictionary<string, string>>(value.ResultEntry, "regionOutputs");
        if (entries.Count > 100000 || entries.Any(x => string.IsNullOrWhiteSpace(x.Key) ||
            !IsDirectChild(x.Value, value.ResultEntry)) || entries.Values.Distinct(StringComparer.Ordinal).Count() != entries.Count)
            throw new InvalidDataException("Invalid region output manifest.");
        return entries;
    }
}
