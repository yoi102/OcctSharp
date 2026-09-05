namespace OcctSharp;

#pragma warning disable CS1591
/// <summary>Exact topology index gated by a complete source fingerprint. Changed sources must be rebound explicitly.</summary>
public sealed record ParametricLocalSelection(string Input, string Fingerprint, int Index, ShapeKind Kind)
{
    public static ParametricLocalSelection Bind(string input, RepairSnapshot source, RepairSelection selection)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input); ArgumentNullException.ThrowIfNull(source);
        source.Validate(selection); return new(input, source.Fingerprint, selection.Index, source.Topology[selection.Index].Kind);
    }
}
public sealed record ParametricFilletVertex(ParametricLocalSelection Vertex, double Radius);
public sealed record ParametricFilletContour(ParametricLocalSelection Edge, double Radius = 1, string? RadiusParameter = null,
    ParametricLawRecipe? Law = null, IReadOnlyList<FilletRadiusSample>? Samples = null, IReadOnlyList<ParametricFilletVertex>? Vertices = null);
public sealed record ParametricFilletRecipe(string Source, IReadOnlyList<ParametricFilletContour> Contours,
    ContourFilletOptions? Options = null, RepairBudget? Budget = null, IReadOnlyList<ParametricLocalSelection>? Protected = null) : ParametricRecipe;
public sealed record ParametricChamferContour(ParametricLocalSelection Edge, ParametricLocalSelection Support,
    ChamferDimensions Dimensions, double First, double Second = 0);
public sealed record ParametricChamferRecipe(string Source, IReadOnlyList<ParametricChamferContour> Contours,
    ContourChamferMode Mode = ContourChamferMode.Classic) : ParametricRecipe;
public sealed record ParametricDraftFace(ParametricLocalSelection Face, double Angle, GpXyz Direction,
    GpXyz NeutralOrigin, GpXyz NeutralNormal, DraftPropagation Propagation = DraftPropagation.NativeTangentChain);
public sealed record ParametricDraftRecipe(string Source, IReadOnlyList<ParametricDraftFace> Faces) : ParametricRecipe;
public sealed record ParametricLocalSliding(ParametricLocalSelection Edge, ParametricLocalSelection Face);
public sealed record ParametricLimitedFeatureRecipe(string Basis, string Profile, ParametricLocalSelection Support,
    LimitedFeatureOptions Options, string? From = null, string? Until = null, string? Spine = null,
    IReadOnlyList<ParametricLocalSliding>? Sliding = null) : ParametricRecipe;

public sealed partial class ParametricDocument
{
    private static Candidate EvaluateLocalFeature(ParametricFeatureDefinition definition, Func<string, Shape> input, Func<string, double> length)
    {
        Dictionary<string, RepairSnapshot> snapshots = new(StringComparer.Ordinal); List<Shape> selectedShapes = [];
        RepairSnapshot Snapshot(string name)
        {
            if (!snapshots.TryGetValue(name, out var source)) { source = RepairSnapshot.Create(input(name)); snapshots.Add(name, source); }
            return source;
        }
        RepairSelection Resolve(ParametricLocalSelection selection, string expected, ShapeKind kind)
        {
            ArgumentNullException.ThrowIfNull(selection);
            if (selection.Input != expected || selection.Kind != kind) throw new ArgumentException("Local selector has an incompatible input or topology kind.");
            var source = Snapshot(expected);
            if (source.Fingerprint != selection.Fingerprint) throw new InvalidOperationException("The local feature source changed; explicitly rebind its topology selections.");
            var resolved = source.Select(selection.Index);
            LocalFeatureBridge.Select(source, resolved, kind); return resolved;
        }
        Shape Topology(ParametricLocalSelection selection, ShapeKind kind)
        {
            ArgumentNullException.ThrowIfNull(selection);
            var resolved = Resolve(selection, selection.Input, kind);
            var shape = LocalFeatureBridge.ShareSelected(Snapshot(selection.Input), resolved); selectedShapes.Add(shape); return shape;
        }
        Candidate CandidateFrom(LocalFeatureResult result, string sourceName, Shape? accepted = null)
        {
            var candidate = new Candidate(Share(accepted ?? result.RequireShape()));
            candidate.Diagnostics.Add(result.Diagnostics.Message);
            candidate.Diagnostics.Add(System.Text.Json.JsonSerializer.Serialize(result.Contours));
            candidate.Diagnostics.Add($"Local recipe source {sourceName} is fingerprint-bound. Rebinding is required after geometric source changes; no positional naming heuristic is used.");
            return candidate;
        }
        try
        {
            switch (definition.Kind)
            {
                case ParametricFeatureKind.ContourFillet:
                {
                    var recipe = Recipe<ParametricFilletRecipe>(definition); var source = Snapshot(recipe.Source);
                    var programs = ScalarLawDefinition.Copy(recipe.Contours, 256).Select(c =>
                    {
                        ArgumentNullException.ThrowIfNull(c); var seed = Resolve(c.Edge, recipe.Source, ShapeKind.Edge);
                        if ((c.Law is not null ? 1 : 0) + (c.Samples is not null ? 1 : 0) + (c.RadiusParameter is not null ? 1 : 0) > 1)
                            throw new ArgumentException("Choose one persisted radius source.");
                        var p = c.Law is not null ? FilletContourProgram.FromLaw(seed, c.Law.Build())
                            : c.Samples is not null ? FilletContourProgram.Sampled(seed, c.Samples)
                            : FilletContourProgram.Constant(seed, c.RadiusParameter is null ? c.Radius : length(c.RadiusParameter));
                        return p.WithVertexRadii(ScalarLawDefinition.Copy(c.Vertices ?? [], 65536).Select(v =>
                            new FilletVertexRadius(Resolve(v.Vertex, recipe.Source, ShapeKind.Vertex), v.Radius)));
                    }).ToArray();
                    using var result = ContourFilletRecipe.Create(source, programs, recipe.Options).Build(source);
                    using var acceptance = LocalFeatureAcceptance.Inspect(source, result, recipe.Budget,
                        ScalarLawDefinition.Copy(recipe.Protected ?? [], 100000).Select(p => Resolve(p, recipe.Source, p.Kind)));
                    using var accepted = acceptance.Accept(); return CandidateFrom(result, recipe.Source, accepted);
                }
                case ParametricFeatureKind.ContourChamfer:
                {
                    var recipe = Recipe<ParametricChamferRecipe>(definition); var source = Snapshot(recipe.Source);
                    var programs = ScalarLawDefinition.Copy(recipe.Contours, 256).Select(c => new ChamferContourProgram(
                        Resolve(c.Edge, recipe.Source, ShapeKind.Edge), Resolve(c.Support, recipe.Source, ShapeKind.Face), c.Dimensions, c.First, c.Second));
                    using var result = ContourChamferRecipe.Create(source, programs, recipe.Mode).Build(source);
                    return CandidateFrom(result, recipe.Source);
                }
                case ParametricFeatureKind.FaceDraft:
                {
                    var recipe = Recipe<ParametricDraftRecipe>(definition); var source = Snapshot(recipe.Source);
                    var programs = ScalarLawDefinition.Copy(recipe.Faces, 256).Select(f => new FaceDraftProgram(
                        Resolve(f.Face, recipe.Source, ShapeKind.Face), f.Angle, f.Direction, f.NeutralOrigin, f.NeutralNormal, f.Propagation));
                    using var result = FaceDraftRecipe.Create(source, programs).Build(source); return CandidateFrom(result, recipe.Source);
                }
                case ParametricFeatureKind.LimitedFeature:
                {
                    var recipe = Recipe<ParametricLimitedFeatureRecipe>(definition);
                    var support = Topology(recipe.Support, ShapeKind.Face);
                    var sliding = ScalarLawDefinition.Copy(recipe.Sliding ?? [], 256).Select(s => new LocalSlidingConstraint(
                        Topology(s.Edge, ShapeKind.Edge), Topology(s.Face, ShapeKind.Face))).ToArray();
                    // Every selected support and basis/profile comes from the same
                    // copied snapshots so the complete graph retains shared topology.
                    using var plan = LimitedFeaturePlan.Create(Snapshot(recipe.Basis).Shape, Snapshot(recipe.Profile).Shape, support, recipe.Options,
                        recipe.From is null ? null : Snapshot(recipe.From).Shape, recipe.Until is null ? null : Snapshot(recipe.Until).Shape,
                        recipe.Spine is null ? null : Snapshot(recipe.Spine).Shape, sliding);
                    using var result = plan.Build(); return CandidateFrom(result, recipe.Basis);
                }
                default: throw new NotSupportedException("Not a local-feature recipe.");
            }
        }
        finally { foreach (var shape in selectedShapes) shape.Dispose(); foreach (var snapshot in snapshots.Values) snapshot.Dispose(); }
    }
}
#pragma warning restore CS1591
