using System.Text.Json;
using System.Text.Json.Serialization;

namespace OcctSharp;

#pragma warning disable CS1591
/// <summary>Versioned, finite built-in recipe values. Serializing a recipe captures its current values.</summary>
public abstract record ParametricRecipe
{
    public int SchemaVersion { get; init; } = 1;
    public string ToJson() => JsonSerializer.Serialize(this, GetType());
}
public sealed record ParametricBooleanRecipe(FeatureBooleanOperation Operation = FeatureBooleanOperation.Fuse,
    int ArgumentCount = 1, FeatureModelingOptions? Options = null) : ParametricRecipe;
public sealed record ParametricRepairRecipe(IReadOnlyList<RepairStage> Stages,
    RepairTolerancePolicy? Tolerance = null, RepairBudget? Budget = null) : ParametricRecipe;
public sealed record ParametricLawRecipe(ScalarLawKind Kind, LawDomain Domain, double FirstValue, double LastValue,
    double FirstDerivative = 0, double LastDerivative = 0) : ParametricRecipe
{
    internal ScalarLawDefinition Build() => Kind switch
    {
        ScalarLawKind.Constant => ScalarLawDefinition.Constant(Domain, FirstValue),
        ScalarLawKind.Linear => ScalarLawDefinition.Linear(Domain, FirstValue, LastValue),
        ScalarLawKind.Smooth => ScalarLawDefinition.Smooth(Domain, FirstValue, LastValue, FirstDerivative, LastDerivative),
        _ => throw new NotSupportedException("The persisted sweep-law profile supports constant, linear and smooth laws.")
    };
}
public sealed record ParametricSweepSection(string Profile, string? SpineVertex = null, bool WithContact = false, bool WithCorrection = false);
public sealed record ParametricSweepRecipe(string Spine, IReadOnlyList<ParametricSweepSection> Sections,
    GuidedSweepOptions? Options = null, string? GuideOrSupport = null, ParametricLawRecipe? ScaleLaw = null) : ParametricRecipe;
public sealed record ParametricFillConstraint(string Id, SurfaceConstraintKind Kind, string? Edge = null, string? Support = null,
    SurfaceConstraintContinuity Continuity = SurfaceConstraintContinuity.G0, bool Boundary = true, bool Required = true,
    double U = 0, double V = 0, GpPoint Point = default);
public sealed record ParametricFillRecipe(IReadOnlyList<ParametricFillConstraint> Constraints,
    ConstrainedFillOptions? Options = null, string? InitialSurface = null) : ParametricRecipe;
public sealed record ParametricMeshRecipe(AdvancedMeshOptions? Meshing = null, bool RemoveDuplicateTriangles = false,
    bool Compact = true, bool RecomputeNormals = false) : ParametricRecipe;

public sealed partial class ParametricDocument
{
    private static readonly JsonSerializerOptions RecipeOptions = new()
    { MaxDepth = 64, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow };
    private static T Recipe<T>(ParametricFeatureDefinition definition) where T : ParametricRecipe
    {
        T value = JsonSerializer.Deserialize<T>(definition.Recipe ?? throw new ArgumentException("This feature requires a typed recipe."), RecipeOptions)
            ?? throw new ArgumentException("The recipe is null.");
        if (value.SchemaVersion != 1) throw new NotSupportedException("Unsupported recipe schema.");
        return value;
    }
}
#pragma warning restore CS1591
