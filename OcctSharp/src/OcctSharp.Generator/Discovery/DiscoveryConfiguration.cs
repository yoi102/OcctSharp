namespace OcctSharp.Generator.Discovery;

public sealed record DiscoveryConfiguration(
    string SchemaVersion,
    string OcctVersion,
    string Scope,
    IReadOnlyList<string> Headers,
    IReadOnlyDictionary<string, string> ToolkitByPackage)
{
    public IReadOnlyList<GenerationScopeConfiguration> GenerationScopes { get; init; } =
    [
        GenerationScopeConfiguration.Precision,
    ];

    public IReadOnlyList<SharedHandleScopeConfiguration> SharedHandleScopes { get; init; } = [];

    public IReadOnlyList<SharedHandlePackageScopeConfiguration> SharedHandlePackageScopes { get; init; } = [];

    public IReadOnlyList<TopologyScopeConfiguration> TopologyScopes { get; init; } = [];

    public IReadOnlyList<string> InventoryPreambleHeaders { get; init; } = [];

    public IReadOnlyList<string> HeaderPatterns { get; init; } = [];

    public IReadOnlyList<ManualBindingConfiguration> ManualBindings { get; init; } = [];
}

public sealed record ManualBindingConfiguration
{
    public string StableId { get; init; } = string.Empty;

    public string SpecialCaseId { get; init; } = string.Empty;
}

public sealed record SharedHandlePackageScopeConfiguration
{
    public string SourcePackage { get; init; } = string.Empty;

    public string NativeTypePrefix { get; init; } = string.Empty;

    public IReadOnlyList<string> ExcludedNativeTypes { get; init; } = [];
}

public sealed record TopologyScopeConfiguration
{
    public string SourcePackage { get; init; } = string.Empty;

    public string NativeType { get; init; } = string.Empty;

    public string Header { get; init; } = string.Empty;

    public string ExportNamePrefix { get; init; } = string.Empty;

    public string ManagedTypeName { get; init; } = string.Empty;

    public IReadOnlyList<TopologyTypeConfiguration> TypedTypes { get; init; } = [];
}

public sealed record TopologyTypeConfiguration
{
    public string NativeType { get; init; } = string.Empty;

    public string Header { get; init; } = string.Empty;

    public string ManagedTypeName { get; init; } = string.Empty;

    public string ShapeKind { get; init; } = string.Empty;
}

public sealed record SharedHandleScopeConfiguration
{
    public string SourcePackage { get; init; } = string.Empty;

    public string NativeType { get; init; } = string.Empty;

    public string Header { get; init; } = string.Empty;

    public string ExportNamePrefix { get; init; } = string.Empty;

    public string ManagedTypeName { get; init; } = string.Empty;
}

public sealed record GenerationScopeConfiguration
{
    public string SourcePackage { get; init; } = string.Empty;

    public string NativeNamePrefix { get; init; } = string.Empty;

    public string Header { get; init; } = string.Empty;

    public string ExportNamePrefix { get; init; } = string.Empty;

    public string ManagedNamePrefix { get; init; } = string.Empty;

    public static GenerationScopeConfiguration Precision { get; } = new()
    {
        SourcePackage = "Precision",
        NativeNamePrefix = "Precision::",
        Header = "Precision.hxx",
        ExportNamePrefix = "precision",
        ManagedNamePrefix = "Precision",
    };
}
