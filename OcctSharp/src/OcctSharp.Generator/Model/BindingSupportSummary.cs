namespace OcctSharp.Generator.Model;

public sealed record BindingSupportSummary(
    int Total,
    int Pending,
    int Skipped,
    int Supported,
    int Manual,
    IReadOnlyDictionary<string, int> SkipReasons)
{
    public static BindingSupportSummary Create(BindingModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        IReadOnlyDictionary<string, int> skipReasons = model.Declarations
            .Where(static declaration => declaration.SkipReason is not null)
            .GroupBy(static declaration => declaration.SkipReason!.Code, StringComparer.Ordinal)
            .OrderBy(static group => group.Key, StringComparer.Ordinal)
            .ToDictionary(static group => group.Key, static group => group.Count(), StringComparer.Ordinal);

        return new BindingSupportSummary(
            model.Declarations.Count,
            model.Declarations.Count(static declaration => declaration.SupportState == BindingSupportState.Pending),
            model.Declarations.Count(static declaration => declaration.SupportState == BindingSupportState.Skipped),
            model.Declarations.Count(static declaration => declaration.SupportState == BindingSupportState.Supported),
            model.Declarations.Count(static declaration => declaration.SupportState == BindingSupportState.Manual),
            skipReasons);
    }
}
