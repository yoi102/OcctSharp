using System.Text.Json;
using System.Text.Json.Serialization;

namespace OcctSharp;

#pragma warning disable CS1591
public sealed record ViewerRecipeAppearance(string PresentationKey, ViewerAppearanceProfile? Profile, string? TextureKey);
public sealed record ViewerRecipeEnvironment(string Key, bool ShowBackground, bool Illuminate);

/// <summary>Portable copied review state. Asset keys are application identifiers, never native IDs or implicit file paths.</summary>
public sealed class ViewerReviewRecipe
{
    [JsonConstructor]
    public ViewerReviewRecipe(string assetScope, ViewerReviewCamera camera, ViewerRenderProfile profile,
        IReadOnlyList<ViewerLightDefinition> lights, IReadOnlyList<ViewerRecipeAppearance> appearances,
        ViewerRecipeEnvironment? environment = null, int schemaVersion = 1)
    {
        ValidateKey(assetScope); ArgumentNullException.ThrowIfNull(camera); ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(lights); ArgumentNullException.ThrowIfNull(appearances);
        if (schemaVersion != 1 || lights.Count > 128 || appearances.Count > 4096) throw new ArgumentException("Unsupported recipe schema or budget.");
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in appearances) {
            ArgumentNullException.ThrowIfNull(item); ValidateKey(item.PresentationKey);
            if (!seen.Add(item.PresentationKey)) throw new ArgumentException("Duplicate recipe presentation key.");
            if (item.TextureKey is not null) { ValidateKey(item.TextureKey); if (item.Profile is null) throw new ArgumentException("A texture needs an appearance profile."); }
        }
        foreach (var light in lights) ArgumentNullException.ThrowIfNull(light);
        if (environment is not null) ValidateKey(environment.Key);
        SchemaVersion = schemaVersion; AssetScope = assetScope; Camera = camera; Profile = profile;
        Lights = Array.AsReadOnly(lights.ToArray()); Appearances = Array.AsReadOnly(appearances.ToArray()); Environment = environment;
    }
    public int SchemaVersion { get; }
    public string AssetScope { get; }
    public ViewerReviewCamera Camera { get; }
    public ViewerRenderProfile Profile { get; }
    public IReadOnlyList<ViewerLightDefinition> Lights { get; }
    public IReadOnlyList<ViewerRecipeAppearance> Appearances { get; }
    public ViewerRecipeEnvironment? Environment { get; }
    public string ToJson() => JsonSerializer.Serialize(this);
    public static ViewerReviewRecipe FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        if (json.Length > 4 * 1024 * 1024) throw new ArgumentException("Review recipe exceeds its 4 MiB character budget.");
        return JsonSerializer.Deserialize<ViewerReviewRecipe>(json) ?? throw new ArgumentException("Null review recipe.");
    }
    internal static void ValidateKey(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (key.Length > 160 || key.Any(c => !(char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.')))
            throw new ArgumentException("Asset keys must be bounded identifiers, not paths or URLs.");
    }
}

public sealed partial class ViewerRendering
{
    /// <summary>Copies review state for explicitly named presentations; callbacks map resources to application asset keys.</summary>
    public ViewerReviewRecipe CaptureRecipe(string assetScope, IReadOnlyDictionary<string, ViewerPresentation> presentations,
        Func<ViewerTexture, string>? textureKey = null, Func<ViewerEnvironment, string>? environmentKey = null)
    {
        EnsureThread(); ArgumentNullException.ThrowIfNull(presentations);
        var copied = presentations.OrderBy(x => x.Key, StringComparer.Ordinal).Select(item => {
            EnsurePresentation(item.Value); var appearance = GetAppearance(item.Value);
            string? key = appearance?.Texture is { } texture ?
                (textureKey ?? throw new ArgumentException("A texture key resolver is required."))(texture) : null;
            return new ViewerRecipeAppearance(item.Key, appearance?.Profile, key);
        }).ToArray();
        var background = environment is null ? null : new ViewerRecipeEnvironment(
            (environmentKey ?? throw new ArgumentException("An environment key resolver is required."))(environment), environmentBackground, environmentLighting);
        return new(assetScope, GetCamera(), GetProfile(), SnapshotLightRig(), copied, background);
    }

    /// <summary>Replays into explicit same-scope assets, resolving all references before mutation.
    /// Each native setting is atomic; the entire multi-resource replay is not a document transaction.</summary>
    public void ApplyRecipe(ViewerReviewRecipe recipe, string assetScope,
        IReadOnlyDictionary<string, ViewerPresentation> presentations,
        IReadOnlyDictionary<string, ViewerTexture>? textures = null,
        IReadOnlyDictionary<string, ViewerEnvironment>? environments = null)
    {
        ArgumentNullException.ThrowIfNull(recipe); ArgumentNullException.ThrowIfNull(presentations); EnsureThread();
        if (!string.Equals(recipe.AssetScope, assetScope, StringComparison.Ordinal)) throw new ArgumentException("Review recipe asset scope does not match.");
        var resolved = recipe.Appearances.Select(item => {
            if (!presentations.TryGetValue(item.PresentationKey, out var p)) throw new ArgumentException("Missing presentation asset: " + item.PresentationKey);
            EnsurePresentation(p); ViewerTexture? texture = null;
            if (item.TextureKey is not null && (textures is null || !textures.TryGetValue(item.TextureKey, out texture))) throw new ArgumentException("Missing texture asset: " + item.TextureKey);
            texture?.Ensure(this); return (p, item.Profile, texture);
        }).ToArray();
        ViewerEnvironment? env = null;
        if (recipe.Environment is { } reference && (environments is null || !environments.TryGetValue(reference.Key,out env))) throw new ArgumentException("Missing environment asset: " + reference.Key);
        env?.Ensure(this);
        SetProfile(recipe.Profile); SetCamera(recipe.Camera); ReplaceLightRig(recipe.Lights);
        foreach (var (p, profile, texture) in resolved) { if (profile is null) ResetAppearance(p); else SetAppearance(p, profile, texture); }
        SetEnvironment(env, recipe.Environment?.ShowBackground ?? false, recipe.Environment?.Illuminate ?? false);
    }
}
