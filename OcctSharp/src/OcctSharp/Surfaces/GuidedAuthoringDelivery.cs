using System.Text.Json;

namespace OcctSharp;

#pragma warning disable CS1591
/// <summary>Copied portable description and source fingerprints, not an executable script or guaranteed exchange-format payload.</summary>
public sealed record GuidedAuthoringRecipe(Guid PlanId, string Kind, string DefinitionJson, IReadOnlyList<string> SourceFingerprints);
public sealed record GuidedAuthoringProduct(XdeLabel Result, XdeLabel Recipe, GuidedAuthoringRecipe Snapshot);
public static class GuidedAuthoringDelivery
{
    public static GuidedAuthoringRecipe Capture(GuidedSweepPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return new(plan.Id, "guided-sweep", JsonSerializer.Serialize(new { schema = 1, plan.Options, plan.Sections,
            plan.GuideOrSupportInputIndex, ScaleDomain = plan.ScaleLaw?.Domain, ScaleSpans = plan.ScaleLaw?.Spans }),
            Fingerprints(plan.InputCount, plan.CopyInput));
    }
    public static GuidedAuthoringRecipe Capture(ConstrainedFillPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return new(plan.Id, "constrained-fill", JsonSerializer.Serialize(new { schema = 1, plan.Options, plan.Constraints, plan.InitialSurfaceInputIndex }),
            Fingerprints(plan.InputCount, plan.CopyInput));
    }
    private static System.Collections.ObjectModel.ReadOnlyCollection<string> Fingerprints(int count, Func<int, Shape> copy)
    {
        string[] values = new string[count];
        for (int i = 0; i < count; i++) { using Shape shape = copy(i); values[i] = RepairSnapshot.ComputeFingerprint(shape); }
        return Array.AsReadOnly(values);
    }
    public static GuidedAuthoringProduct Publish(XdeDocument document, AuthoringResult result, GuidedAuthoringRecipe recipe,
        string name, XdeColor? color = null, IReadOnlyList<XdeLabel>? sourceReferences = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        return PublishCore(document, result.RequireShape(), result.PlanId, recipe, name, color, sourceReferences);
    }
    public static GuidedAuthoringProduct Publish(XdeDocument document, ConstrainedFillResult result, GuidedAuthoringRecipe recipe,
        string name, XdeColor? color = null, IReadOnlyList<XdeLabel>? sourceReferences = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        return PublishCore(document, result.RequireFace(), result.Result.PlanId, recipe, name, color, sourceReferences);
    }
    private static GuidedAuthoringProduct PublishCore(XdeDocument document, Shape shape, Guid planId, GuidedAuthoringRecipe recipe,
        string name, XdeColor? color, IReadOnlyList<XdeLabel>? references)
    {
        ArgumentNullException.ThrowIfNull(document); ArgumentNullException.ThrowIfNull(recipe); ArgumentException.ThrowIfNullOrWhiteSpace(name);
        document.ThrowIfDisposed(); if (document.HasOpenTransaction) throw new InvalidOperationException("Guided publication owns its transaction.");
        if (recipe.PlanId != planId) throw new ArgumentException("Recipe belongs to a different authoring plan.");
        ArgumentException.ThrowIfNullOrWhiteSpace(recipe.DefinitionJson);
        if (recipe.Kind is not ("guided-sweep" or "constrained-fill") || recipe.DefinitionJson.Length > 1048576)
            throw new ArgumentException("Recipe kind or definition exceeds the supported storage contract.");
        string[] fingerprints = ScalarLawDefinition.Copy(recipe.SourceFingerprints, 512);
        if (fingerprints.Any(value => value is null || value.Length != 64 || !value.All(Uri.IsHexDigit)))
            throw new ArgumentException("Source fingerprints must be copied SHA256 strings.");
        using (JsonDocument parsed = JsonDocument.Parse(recipe.DefinitionJson))
            if (parsed.RootElement.ValueKind != JsonValueKind.Object) throw new ArgumentException("Recipe definition must be a JSON object.");
        recipe = recipe with { SourceFingerprints = Array.AsReadOnly(fingerprints) };
        string payload = JsonSerializer.Serialize(recipe);
        XdeLabel[] sources = references is null ? [] : ScalarLawDefinition.Copy(references, 512);
        foreach (var source in sources)
            if (source is null || !ReferenceEquals(source.Document, document)) throw new ArgumentException("Source references must belong to this document.");
        using Shape independent = AuthoringBridge.CopyInputs([shape])[0];
        using XdeTransaction transaction = document.BeginTransaction("Publish guided authoring result");
        XdeLabel result = document.AddShape(independent, name); if (color is not null) result.Color = color;
        XdeLabel recipeLabel = result.AddChild(); recipeLabel.Name = "OcctSharp guided recipe"; recipeLabel.AsciiString = payload;
        recipeLabel.SetReferences(sources); recipeLabel.Reference = result;
        transaction.Commit(); return new(result, recipeLabel, recipe);
    }
}
#pragma warning restore CS1591
