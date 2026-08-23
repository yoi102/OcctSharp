namespace OcctSharp.Generator.Model;

public sealed class BindingModel
{
    public BindingModel(IEnumerable<BindingDeclaration> declarations)
    {
        ArgumentNullException.ThrowIfNull(declarations);

        Declarations = declarations
            .GroupBy(static declaration => declaration.StableId, StringComparer.Ordinal)
            .Select(static group => SelectCanonical(group))
            .OrderBy(static declaration => declaration.StableId, StringComparer.Ordinal)
            .ToArray();
    }

    public IReadOnlyList<BindingDeclaration> Declarations { get; }

    private static BindingDeclaration SelectCanonical(IGrouping<string, BindingDeclaration> group)
    {
        BindingDeclaration first = group.First();
        if (group.Any(candidate => candidate.NativeName != first.NativeName || candidate.Kind != first.Kind))
        {
            throw new InvalidDataException(
                $"Stable declaration ID '{group.Key}' maps to conflicting declarations.");
        }

        return group
            .OrderBy(static declaration => declaration.Header, StringComparer.Ordinal)
            .ThenBy(static declaration => declaration.Line)
            .ThenBy(static declaration => declaration.Column)
            .First();
    }
}
