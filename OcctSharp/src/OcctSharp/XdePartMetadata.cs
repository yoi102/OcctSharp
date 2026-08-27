namespace OcctSharp;

/// <summary>Common XDE part metadata applied by <see cref="XdeDocument.AddPart"/>.</summary>
public sealed record XdePartMetadata(
    string Name,
    XdeColor? Color = null,
    IReadOnlyList<string>? Layers = null,
    XdeMaterial? Material = null);
