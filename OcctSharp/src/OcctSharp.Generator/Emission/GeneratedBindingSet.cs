namespace OcctSharp.Generator.Emission;

public sealed record GeneratedBindingSet(
    string OcctVersion,
    IReadOnlyList<string> SourceStableIds,
    IReadOnlyList<GeneratedFile> Files);
