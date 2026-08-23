using OcctSharp.Generator.Model;

namespace OcctSharp.Generator.Discovery;

public sealed record DiscoveryReport(
    string SchemaVersion,
    string OcctVersion,
    string Parser,
    IReadOnlyList<string> Headers,
    IReadOnlyList<DiscoveryDiagnostic> Diagnostics,
    BindingModel Model,
    BindingSupportSummary Support);
