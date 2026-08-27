using OcctSharp.Generator.Model;

namespace OcctSharp.Generator.Transformation;

public static class ConfiguredExclusionPass
{
    public static BindingModel Apply(
        BindingModel model,
        IReadOnlyDictionary<string, BindingSkipReason> exclusions,
        IReadOnlyDictionary<string, BindingSkipReason>? packageExclusions = null)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(exclusions);
        packageExclusions ??= new Dictionary<string, BindingSkipReason>(StringComparer.Ordinal);

        HashSet<string> discovered = model.Declarations
            .Select(static declaration => declaration.StableId)
            .ToHashSet(StringComparer.Ordinal);
        string[] missing = exclusions.Keys
            .Where(stableId => !discovered.Contains(stableId))
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (missing.Length != 0)
        {
            throw new InvalidDataException(
                $"Configured excluded binding stable IDs were not discovered: {string.Join(", ", missing)}.");
        }

        return new BindingModel(model.Declarations.Select(declaration =>
            exclusions.TryGetValue(declaration.StableId, out BindingSkipReason? reason)
                || packageExclusions.TryGetValue(declaration.SourcePackage, out reason)
                ? declaration with
                {
                    SupportState = BindingSupportState.Skipped,
                    SkipReason = reason,
                }
                : declaration));
    }
}
