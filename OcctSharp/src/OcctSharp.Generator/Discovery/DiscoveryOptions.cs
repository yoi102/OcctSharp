namespace OcctSharp.Generator.Discovery;

public sealed record DiscoveryOptions(
    string OcctRoot,
    IReadOnlyList<string> Headers,
    IReadOnlyDictionary<string, string>? ToolkitByPackage = null)
{
    public IReadOnlyList<string> PreambleHeaders { get; init; } = [];
}
