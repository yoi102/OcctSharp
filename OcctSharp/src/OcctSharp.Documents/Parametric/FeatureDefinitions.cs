using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace OcctSharp;

#pragma warning disable CS1591
public enum ParametricFeatureKind { SourceShape, Box, Cylinder, Placement, Extrusion, Revolution, Boolean, Repair, GuidedSweep, ConstrainedFill, Mesh, Scalar }
public enum ParametricOutputKind { ExactShape, Mesh, Scalar }
public enum ParametricExecutionState { NotExecuted, Executing, Succeeded, Failed, Blocked }
public enum ParametricRecomputeMode { Incremental, Full, Targeted }
public enum ParametricDeletePolicy { RejectDependants, Cascade }
public enum ParametricExternalReferencePolicy { Reject, Retain }

public sealed class ParametricParameter
{
    [JsonConstructor]
    public ParametricParameter(ParametricValue? value, ParametricExpression? expression)
    {
        if ((value is null) == (expression is null)) throw new ArgumentException("Specify exactly one value or expression.");
        Value = value;
        Expression = expression;
    }
    public ParametricValue? Value { get; }
    public ParametricExpression? Expression { get; }
    public static ParametricParameter FromValue(ParametricValue value) => new(value, null);
    public static ParametricParameter FromExpression(ParametricExpression value) => new(null, value);
}

public sealed record ParametricInput(string Name, Guid FeatureId, ParametricOutputKind Kind);

/// <summary>Copied versioned recipe metadata; topology is persisted separately as document-owned named shapes.</summary>
public sealed class ParametricFeatureDefinition
{
    public const int CurrentSchemaVersion = 1;

    [JsonConstructor]
    public ParametricFeatureDefinition(Guid id, string name, ParametricFeatureKind kind,
        IReadOnlyDictionary<string, ParametricParameter> parameters, IReadOnlyList<ParametricInput> inputs,
        string? recipe = null, int schemaVersion = CurrentSchemaVersion)
    {
        if (schemaVersion != CurrentSchemaVersion) throw new NotSupportedException($"Unsupported feature schema {schemaVersion}.");
        if (id == Guid.Empty || !Enum.IsDefined(kind)) throw new ArgumentException("Invalid feature identity or kind.");
        ValidateName(name);
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(inputs);
        if (parameters.Count > 256 || inputs.Count > 512 || (recipe?.Length ?? 0) > 1_000_000)
            throw new ArgumentException("Feature definition exceeds bounded limits.");
        Dictionary<string, ParametricParameter> copiedParameters = new(StringComparer.Ordinal);
        foreach (var pair in parameters)
        {
            ValidateName(pair.Key);
            ArgumentNullException.ThrowIfNull(pair.Value);
            copiedParameters.Add(pair.Key, pair.Value);
        }
        ParametricInput[] copiedInputs = inputs.ToArray();
        HashSet<string> slots = new(StringComparer.Ordinal);
        foreach (var input in copiedInputs)
        {
            ArgumentNullException.ThrowIfNull(input);
            ValidateName(input.Name);
            if (input.FeatureId == Guid.Empty || !Enum.IsDefined(input.Kind) || !slots.Add(input.Name))
                throw new ArgumentException("Input slots require unique names and valid identities/types.");
        }
        Id = id;
        Name = name;
        Kind = kind;
        SchemaVersion = schemaVersion;
        Recipe = recipe;
        Parameters = new ReadOnlyDictionary<string, ParametricParameter>(copiedParameters);
        Inputs = Array.AsReadOnly(copiedInputs);
    }

    public Guid Id { get; }
    public string Name { get; }
    public ParametricFeatureKind Kind { get; }
    public int SchemaVersion { get; }
    public IReadOnlyDictionary<string, ParametricParameter> Parameters { get; }
    public IReadOnlyList<ParametricInput> Inputs { get; }
    public string? Recipe { get; }
    [JsonIgnore] public ParametricOutputKind OutputKind => Kind switch
    {
        ParametricFeatureKind.Mesh => ParametricOutputKind.Mesh,
        ParametricFeatureKind.Scalar => ParametricOutputKind.Scalar,
        _ => ParametricOutputKind.ExactShape
    };

    public ParametricFeatureDefinition WithParameter(string name, ParametricParameter value)
    {
        Dictionary<string, ParametricParameter> changed = new(Parameters, StringComparer.Ordinal) { [name] = value };
        return new(Id, Name, Kind, changed, Inputs, Recipe, SchemaVersion);
    }

    public ParametricFeatureDefinition WithInputs(IReadOnlyList<ParametricInput> inputs) =>
        new(Id, Name, Kind, Parameters, inputs, Recipe, SchemaVersion);

    internal static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 128 || name.Contains('\0'))
            throw new ArgumentException("Names must contain one to 128 non-NUL characters.", nameof(name));
    }
}

public sealed record ParametricPlanIssue(Guid FeatureId, string Code, string Message);

/// <summary>A deterministic copied executable graph and resolved scalar parameter table.</summary>
public sealed class ParametricExecutionPlan
{
    internal ParametricExecutionPlan(Guid[] order, Dictionary<Guid, IReadOnlyList<Guid>> dependencies,
        Dictionary<ParametricParameterReference, ParametricQuantity> quantities, ParametricPlanIssue[] issues)
    {
        Order = Array.AsReadOnly(order);
        Dependencies = new ReadOnlyDictionary<Guid, IReadOnlyList<Guid>>(dependencies);
        Quantities = new ReadOnlyDictionary<ParametricParameterReference, ParametricQuantity>(quantities);
        Issues = Array.AsReadOnly(issues);
    }
    public IReadOnlyList<Guid> Order { get; }
    public IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> Dependencies { get; }
    public IReadOnlyDictionary<ParametricParameterReference, ParametricQuantity> Quantities { get; }
    public IReadOnlyList<ParametricPlanIssue> Issues { get; }
    public bool CanExecute => Issues.Count == 0;
}
#pragma warning restore CS1591
