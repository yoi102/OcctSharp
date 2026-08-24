using System.Text.RegularExpressions;
using OcctSharp.Generator.Discovery;
using OcctSharp.Generator.Model;

namespace OcctSharp.Generator.Transformation;

public static partial class ManualBindingPass
{
    public static BindingModel Apply(
        BindingModel model,
        IReadOnlyList<ManualBindingConfiguration> manualBindings)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(manualBindings);

        Dictionary<string, ManualBindingConfiguration> configured = new(StringComparer.Ordinal);
        foreach (ManualBindingConfiguration binding in manualBindings)
        {
            if (string.IsNullOrWhiteSpace(binding.StableId))
            {
                throw new InvalidDataException("A manual binding stable ID cannot be empty.");
            }
            if (!SpecialCasePattern().IsMatch(binding.SpecialCaseId))
            {
                throw new InvalidDataException(
                    $"Manual binding '{binding.StableId}' must reference a special case such as SC-032.");
            }
            if (!configured.TryAdd(binding.StableId, binding))
            {
                throw new InvalidDataException($"Manual binding stable ID '{binding.StableId}' is duplicated.");
            }
        }

        HashSet<string> discovered = model.Declarations
            .Select(static declaration => declaration.StableId)
            .ToHashSet(StringComparer.Ordinal);
        string[] missing = configured.Keys
            .Where(stableId => !discovered.Contains(stableId))
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (missing.Length != 0)
        {
            throw new InvalidDataException(
                $"Configured manual binding stable IDs were not discovered: {string.Join(", ", missing)}.");
        }

        return new BindingModel(model.Declarations.Select(declaration =>
            configured.ContainsKey(declaration.StableId)
                ? declaration with
                {
                    SupportState = BindingSupportState.Manual,
                    SkipReason = null,
                }
                : declaration));
    }

    [GeneratedRegex("^SC-[0-9]{3}$", RegexOptions.CultureInvariant)]
    private static partial Regex SpecialCasePattern();
}
