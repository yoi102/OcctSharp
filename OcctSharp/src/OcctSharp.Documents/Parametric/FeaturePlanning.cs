namespace OcctSharp;

/// <summary>Validates copied feature definitions and resolves their bounded dependency closure.</summary>
public static class ParametricPlanning
{
    /// <summary>Builds a stable DAG and reports missing, mistyped or cyclic inputs without document mutation.</summary>
    public static ParametricExecutionPlan Build(IReadOnlyList<ParametricFeatureDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        if (definitions.Count > 4096) throw new ArgumentException("A feature graph is limited to 4096 nodes.");
        Dictionary<Guid, ParametricFeatureDefinition> features = [];
        foreach (var feature in definitions)
        {
            ArgumentNullException.ThrowIfNull(feature);
            if (!features.TryAdd(feature.Id, feature)) throw new ArgumentException("Duplicate feature identity.");
        }
        List<ParametricPlanIssue> issues = [];
        Dictionary<Guid, IReadOnlyList<Guid>> dependencies = [];
        Dictionary<ParametricParameterReference, ParametricQuantity> quantities = [];
        HashSet<ParametricParameterReference> active = [];

        ParametricQuantity Resolve(ParametricParameterReference key)
        {
            if (quantities.TryGetValue(key, out var cached)) return cached;
            if (!features.TryGetValue(key.FeatureId, out var owner) || !owner.Parameters.TryGetValue(key.Name, out var parameter))
                throw new InvalidOperationException($"Missing parameter {key.FeatureId}/{key.Name}.");
            if (active.Count >= 128) throw new InvalidOperationException("Parameter dependency depth exceeds 128.");
            if (!active.Add(key)) throw new InvalidOperationException($"Expression cycle at {owner.Name}/{key.Name}.");
            try
            {
                var result = parameter.Expression is { } expression ? expression.EvaluateCore(Resolve) : parameter.Value!.Quantity();
                quantities.Add(key, result);
                return result;
            }
            finally { active.Remove(key); }
        }

        foreach (var feature in features.Values.OrderBy(x => x.Id))
        {
            HashSet<Guid> previous = [];
            foreach (var input in feature.Inputs)
            {
                previous.Add(input.FeatureId);
                if (!features.TryGetValue(input.FeatureId, out var source))
                    issues.Add(new(feature.Id, "MissingInput", $"Input {input.Name} refers to a missing feature."));
                else if (source.OutputKind != input.Kind)
                    issues.Add(new(feature.Id, "InputType", $"Input {input.Name} expects {input.Kind}, but receives {source.OutputKind}."));
            }
            foreach (var pair in feature.Parameters.OrderBy(x => x.Key, StringComparer.Ordinal))
            {
                if (pair.Value.Expression is { } expression)
                    foreach (var reference in expression.References())
                        if (reference.FeatureId != feature.Id) previous.Add(reference.FeatureId);
                if (pair.Value.Expression is not null || pair.Value.Value?.Kind is ParametricValueKind.Real or ParametricValueKind.Integral)
                {
                    try { _ = Resolve(new(feature.Id, pair.Key)); }
                    catch (Exception error) when (error is InvalidOperationException or ArgumentException or ArithmeticException)
                    { issues.Add(new(feature.Id, "Parameter", $"{pair.Key}: {error.Message}")); }
                }
            }
            dependencies.Add(feature.Id, Array.AsReadOnly(previous.Order().ToArray()));
        }

        Dictionary<Guid, int> indegrees = features.Keys.ToDictionary(id => id, id => dependencies[id].Count);
        Dictionary<Guid, List<Guid>> next = features.Keys.ToDictionary(id => id, _ => new List<Guid>());
        foreach (var pair in dependencies)
            foreach (Guid dependency in pair.Value)
                if (next.TryGetValue(dependency, out var nodes)) nodes.Add(pair.Key);
        SortedSet<Guid> ready = new(indegrees.Where(x => x.Value == 0).Select(x => x.Key));
        List<Guid> order = [];
        while (ready.Count > 0)
        {
            Guid id = ready.Min;
            ready.Remove(id);
            order.Add(id);
            foreach (Guid child in next[id]) if (--indegrees[child] == 0) ready.Add(child);
        }
        foreach (Guid id in features.Keys.Except(order).Order())
            issues.Add(new(id, "BlockedGraph", "Feature is in, or downstream of, a dependency cycle or missing input."));
        foreach (var feature in features.Values) ValidateBuiltInParameters(feature, quantities, issues);
        return new(order.ToArray(), dependencies, quantities, issues.ToArray());
    }

    private static void ValidateBuiltInParameters(ParametricFeatureDefinition feature,
        Dictionary<ParametricParameterReference, ParametricQuantity> quantities, List<ParametricPlanIssue> issues)
    {
        void Require(string name, ParametricDimension? dimension, bool optional = false, bool positive = false)
        {
            if (optional && !feature.Parameters.ContainsKey(name)) return;
            if (!quantities.TryGetValue(new(feature.Id, name), out var value))
                issues.Add(new(feature.Id, "Parameter", $"A resolved scalar parameter '{name}' is required."));
            else if ((dimension.HasValue && dimension != value.Dimension) || (positive && value.Value <= 0))
                issues.Add(new(feature.Id, "Parameter", $"Parameter '{name}' has incompatible units or range."));
        }
        switch (feature.Kind)
        {
            case ParametricFeatureKind.Box:
                Require("x", ParametricDimension.Distance, positive: true);
                Require("y", ParametricDimension.Distance, positive: true);
                Require("z", ParametricDimension.Distance, positive: true);
                break;
            case ParametricFeatureKind.Cylinder:
                Require("radius", ParametricDimension.Distance, positive: true);
                Require("height", ParametricDimension.Distance, positive: true);
                break;
            case ParametricFeatureKind.Placement:
            case ParametricFeatureKind.Extrusion:
            case ParametricFeatureKind.Revolution:
                Require("x", ParametricDimension.Distance, optional: true); Require("y", ParametricDimension.Distance, optional: true);
                Require("z", ParametricDimension.Distance, optional: feature.Kind != ParametricFeatureKind.Extrusion);
                if (feature.Kind == ParametricFeatureKind.Extrusion) break;
                Require("angle", ParametricDimension.Rotation, optional: feature.Kind != ParametricFeatureKind.Revolution);
                Require("axisX", ParametricDimension.Scalar, optional: true); Require("axisY", ParametricDimension.Scalar, optional: true);
                Require("axisZ", ParametricDimension.Scalar, optional: true);
                break;
            case ParametricFeatureKind.Scalar:
                Require("value", null);
                break;
        }
    }
}
