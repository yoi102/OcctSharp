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

    public bool AutoGenerationScopes { get; init; }

    public IReadOnlyList<SharedHandleScopeConfiguration> SharedHandleScopes { get; init; } = [];

    public IReadOnlyList<SharedHandlePackageScopeConfiguration> SharedHandlePackageScopes { get; init; } = [];

    public bool AutoSharedHandlePackageScopes { get; init; }

    public IReadOnlyList<TopologyScopeConfiguration> TopologyScopes { get; init; } = [];

    public IReadOnlyList<string> InventoryPreambleHeaders { get; init; } = [];

    /// <summary>Headers emitted before generated shared-type headers to complete known forward-declared template element types.</summary>
    public IReadOnlyList<string> GeneratedPreambleHeaders { get; init; } = [];

    public IReadOnlyList<string> HeaderPatterns { get; init; } = [];

    public IReadOnlyList<string> ExcludedHeaders { get; init; } = [];

    public IReadOnlyList<ManualBindingConfiguration> ManualBindings { get; init; } = [];

    public IReadOnlyList<ExcludedBindingConfiguration> ExcludedBindings { get; init; } = [];

    /// <summary>Whole source packages excluded from automatic core-package generation with an explicit disposition.</summary>
    public IReadOnlyList<ExcludedAutoPackageConfiguration> ExcludedAutoPackages { get; init; } = [];
}

public sealed record ManualBindingConfiguration
{
    public string StableId { get; init; } = string.Empty;

    public string SpecialCaseId { get; init; } = string.Empty;
}

public sealed record ExcludedBindingConfiguration
{
    public string StableId { get; init; } = string.Empty;

    public string Code { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public string Detail { get; init; } = string.Empty;
}

public sealed record ExcludedAutoPackageConfiguration
{
    public IReadOnlyList<string> SourcePackages { get; init; } = [];

    public string Code { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public string Detail { get; init; } = string.Empty;
}

public sealed record SharedHandlePackageScopeConfiguration
{
    public string SourcePackage { get; init; } = string.Empty;

    public string NativeTypePrefix { get; init; } = string.Empty;

    public IReadOnlyList<string> ExcludedNativeTypes { get; init; } = [];

    /// <summary>Suppresses all constructors for packages whose declared creation ABI is not link-safe.</summary>
    public bool SuppressConstructors { get; init; }

    public IReadOnlyList<string> ExcludedStableIds { get; init; } = [];

    /// <summary>Native types whose instances must be placement-allocated by an NCollection_IncAllocator constructor argument.</summary>
    public IReadOnlyList<string> PlacementAllocatorNativeTypes { get; init; } = [];
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

    /// <summary>True when the wrapper is return/parameter-only and must not expose creation exports.</summary>
    public bool SuppressConstructors { get; init; }

    public IReadOnlyList<string> ExcludedStableIds { get; init; } = [];

    /// <summary>True when construction uses NCollection_IncAllocator placement new and the wrapper must retain that allocator.</summary>
    public bool UsesPlacementAllocator { get; init; }
}

public sealed record GenerationScopeConfiguration
{
    public string SourcePackage { get; init; } = string.Empty;

    public string NativeNamePrefix { get; init; } = string.Empty;

    public string Header { get; init; } = string.Empty;

    public string ExportNamePrefix { get; init; } = string.Empty;

    public string ManagedNamePrefix { get; init; } = string.Empty;

    /// <summary>Matches one free-function name and its declaring header instead of a C++ static-member prefix.</summary>
    public bool ExactNativeName { get; init; }

    public static GenerationScopeConfiguration Precision { get; } = new()
    {
        SourcePackage = "Precision",
        NativeNamePrefix = "Precision::",
        Header = "Precision.hxx",
        ExportNamePrefix = "precision",
        ManagedNamePrefix = "Precision",
    };
}
