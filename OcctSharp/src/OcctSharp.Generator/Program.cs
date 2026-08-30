using System.Text.Json;
using System.Text.Json.Serialization;
using OcctSharp.Generator.Discovery;
using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Inventory;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Reporting;
using OcctSharp.Generator.Transformation;

return args switch
{
    ["model-smoke", string outputPath] => await RunModelSmokeAsync(outputPath),
    ["discover", "--occt-root", string occtRoot, "--header", string header, "--output", string outputPath]
        => await RunDiscoveryAsync(occtRoot, [header], expectedOcctVersion: null, outputPath),
    ["discover", "--occt-root", string occtRoot, "--config", string configPath, "--output", string outputPath]
        => await RunConfiguredDiscoveryAsync(occtRoot, configPath, outputPath),
    ["generate", "--occt-root", string occtRoot, "--config", string configPath, "--output-root", string outputRoot]
        => await RunGenerationAsync(occtRoot, configPath, outputRoot),
    ["inventory-catalog", "--occt-root", string occtRoot, "--output", string outputPath]
        => await RunInventoryCatalogAsync(occtRoot, outputPath),
    ["inventory", "--occt-root", string occtRoot, "--config", string configPath, "--output", string outputPath, "--batch-size", string batchSize]
        => await RunInventoryAsync(occtRoot, configPath, outputPath, batchSize, manifestPath: null),
    ["inventory", "--occt-root", string occtRoot, "--config", string configPath, "--output", string outputPath, "--batch-size", string batchSize, "--manifest", string manifestPath]
        => await RunInventoryAsync(occtRoot, configPath, outputPath, batchSize, manifestPath),
    _ => PrintUsage(),
};

static async Task<int> RunInventoryCatalogAsync(string occtRoot, string outputPath)
{
    try
    {
        OcctInventoryReport report = OcctInventory.CreateCatalog(occtRoot);
        await WriteJsonAsync(outputPath, report);
        Console.WriteLine(
            $"Catalogued {report.Headers.Total} public entry headers across {report.Headers.Packages.Count} source packages for OCCT {report.OcctVersion}; report: '{Path.GetFullPath(outputPath)}'.");
        return 0;
    }
    catch (Exception error)
    {
        Console.Error.WriteLine(error.Message);
        return 1;
    }
}

static async Task<int> RunInventoryAsync(
    string occtRoot,
    string configPath,
    string outputPath,
    string batchSizeText,
    string? manifestPath)
{
    try
    {
        if (!int.TryParse(batchSizeText, out int batchSize) || batchSize < 1)
        {
            throw new ArgumentException($"Inventory batch size '{batchSizeText}' is not a positive integer.");
        }

        DiscoveryConfiguration configuration = await ReadConfigurationAsync(configPath);
        HashSet<string>? emittedStableIds = null;
        if (manifestPath is not null)
        {
            GeneratedManifest? manifest = JsonSerializer.Deserialize<GeneratedManifest>(
                await File.ReadAllTextAsync(manifestPath),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (manifest is null || !string.Equals(manifest.OcctVersion, configuration.OcctVersion, StringComparison.Ordinal))
            {
                throw new InvalidDataException("The generated manifest is missing or targets a different OCCT version.");
            }
            emittedStableIds = manifest.SourceStableIds.ToHashSet(StringComparer.Ordinal);
        }
        HashSet<string> manualStableIds = configuration.ManualBindings
            .Select(static binding => binding.StableId)
            .ToHashSet(StringComparer.Ordinal);
        IReadOnlyDictionary<string, BindingSkipReason> excludedBindings = GetExcludedBindings(configuration);
        IReadOnlyDictionary<string, BindingSkipReason> excludedPackages = GetExcludedAutoPackages(configuration);
        OcctInventoryReport report = OcctInventory.Discover(
            occtRoot,
            batchSize,
            configuration.ToolkitByPackage,
            configuration.InventoryPreambleHeaders,
            emittedStableIds,
            manualStableIds,
            excludedBindings,
            excludedPackages,
            Console.WriteLine);
        if (!string.Equals(configuration.OcctVersion, report.OcctVersion, StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"Configuration expects OCCT {configuration.OcctVersion}, but the selected installation reports {report.OcctVersion}.");
        }

        await WriteJsonAsync(outputPath, report);
        Console.WriteLine(
            $"Inventoried {report.Headers.Scanned}/{report.Headers.Total} headers and {report.Declarations?.Total ?? 0} unique declarations; semantic complete: {report.IsComplete}; classification complete: {report.FinalClassification?.IsComplete}; report: '{Path.GetFullPath(outputPath)}'.");
        return report.FinalClassification?.IsComplete == true ? 0 : 3;
    }
    catch (Exception error)
    {
        Console.Error.WriteLine(error.Message);
        return 1;
    }
}

static async Task<int> RunModelSmokeAsync(string outputPath)
{
    BindingModel model = new(
    [
        new BindingDeclaration(
            "record:gp_Pnt",
            "gp_Pnt",
            BindingDeclarationKind.Record,
            "gp_Pnt.hxx",
            1,
            1),
    ]);

    await WriteJsonAsync(outputPath, model);
    Console.WriteLine($"Wrote deterministic model smoke report to '{Path.GetFullPath(outputPath)}'.");
    return 0;
}

static async Task<int> RunConfiguredDiscoveryAsync(string occtRoot, string configPath, string outputPath)
{
    try
    {
        DiscoveryConfiguration? configuration = JsonSerializer.Deserialize<DiscoveryConfiguration>(
            await File.ReadAllTextAsync(configPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (configuration is null || !IsSupportedConfigurationSchema(configuration.SchemaVersion))
        {
            throw new InvalidDataException("The discovery configuration is missing or has an unsupported schema version.");
        }

        return await RunDiscoveryAsync(
            occtRoot,
            ExpandHeaders(occtRoot, configuration),
            configuration.OcctVersion,
            outputPath,
            configuration.ToolkitByPackage,
            configuration.ManualBindings,
            GetExcludedBindings(configuration),
            GetExcludedAutoPackages(configuration));
    }
    catch (Exception error)
    {
        Console.Error.WriteLine(error.Message);
        return 1;
    }
}

static async Task<int> RunGenerationAsync(string occtRoot, string configPath, string outputRoot)
{
    try
    {
        DiscoveryConfiguration configuration = await ReadConfigurationAsync(configPath);
        DiscoveryReport report = ClangAstDiscovery.Discover(
            new DiscoveryOptions(occtRoot, ExpandHeaders(occtRoot, configuration), configuration.ToolkitByPackage));
        report = ApplyManualBindings(report, configuration.ManualBindings);
        report = ApplyConfiguredExclusions(
            report,
            GetExcludedBindings(configuration),
            GetExcludedAutoPackages(configuration));
        if (!string.Equals(configuration.OcctVersion, report.OcctVersion, StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"Configuration expects OCCT {configuration.OcctVersion}, but the selected installation reports {report.OcctVersion}.");
        }

        IReadOnlyList<SharedHandlePackageScopeConfiguration> sharedHandlePackageScopes =
            ExpandSharedHandlePackageScopes(report.Model, configuration);
        IReadOnlyList<SharedHandleScopeConfiguration> sharedHandleScopes =
            SharedHandlePackageScopeExpander.Expand(
                report.Model,
                configuration.SharedHandleScopes,
                sharedHandlePackageScopes);
        IReadOnlyList<GenerationScopeConfiguration> generationScopes =
            GenerationScopeExpander.Expand(
                report.Model,
                configuration.GenerationScopes,
                configuration.AutoGenerationScopes);
        GeneratedBindingSet bindingSet = InitialBindingEmitter.Emit(
            report,
            generationScopes,
            sharedHandleScopes,
            configuration.TopologyScopes,
            configuration.GeneratedPreambleHeaders);
        GeneratedManifest manifest = GeneratedOutputWriter.Replace(outputRoot, bindingSet);
        GenerationReportSet reports = GenerationReportWriter.Create(report, bindingSet);
        GenerationReportWriter.Write(outputRoot, reports);
        Console.WriteLine(
            $"Generated {manifest.Files.Count} files from {manifest.SourceStableIds.Count} binding into '{Path.GetFullPath(outputRoot)}'.");
        Console.WriteLine(
            $"Wrote generation coverage, diagnostics, and shard dependency closure for {reports.Coverage.Totals.Total} declarations; managed project split ready: {reports.DependencyClosure.ManagedProjectSplitReady}.");
        return 0;
    }
    catch (Exception error)
    {
        Console.Error.WriteLine(error.Message);
        return 1;
    }
}

static async Task<DiscoveryConfiguration> ReadConfigurationAsync(string configPath)
{
    DiscoveryConfiguration? configuration = JsonSerializer.Deserialize<DiscoveryConfiguration>(
        await File.ReadAllTextAsync(configPath),
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    if (configuration is null || !IsSupportedConfigurationSchema(configuration.SchemaVersion))
    {
        throw new InvalidDataException("The discovery configuration is missing or has an unsupported schema version.");
    }

    return configuration;
}

static IReadOnlyList<string> ExpandHeaders(string occtRoot, DiscoveryConfiguration configuration)
{
    string includeRoot = Path.Combine(Path.GetFullPath(occtRoot), "inc");
    SortedSet<string> headers = new(configuration.Headers, StringComparer.Ordinal);
    foreach (string pattern in configuration.HeaderPatterns.Order(StringComparer.Ordinal))
    {
        if (string.IsNullOrWhiteSpace(pattern)
            || Path.IsPathRooted(pattern)
            || pattern.Contains("..", StringComparison.Ordinal))
        {
            throw new InvalidDataException($"Header pattern '{pattern}' must be a non-rooted include-directory pattern.");
        }
        foreach (string path in Directory.GetFiles(includeRoot, pattern, SearchOption.TopDirectoryOnly)
            .Order(StringComparer.Ordinal))
        {
            headers.Add(Path.GetFileName(path));
        }
    }
    HashSet<string> excluded = new(StringComparer.Ordinal);
    foreach (string header in configuration.ExcludedHeaders)
    {
        if (string.IsNullOrWhiteSpace(header)
            || Path.IsPathRooted(header)
            || header.Contains("..", StringComparison.Ordinal)
            || !string.Equals(Path.GetFileName(header), header, StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"Excluded header '{header}' must be a non-rooted include-directory file name.");
        }
        excluded.Add(header);
    }
    return headers.Where(header => !excluded.Contains(header)).ToArray();
}

static async Task<int> RunDiscoveryAsync(
    string occtRoot,
    IReadOnlyList<string> headers,
    string? expectedOcctVersion,
    string outputPath,
    IReadOnlyDictionary<string, string>? toolkitByPackage = null,
    IReadOnlyList<ManualBindingConfiguration>? manualBindings = null,
    IReadOnlyDictionary<string, BindingSkipReason>? excludedBindings = null,
    IReadOnlyDictionary<string, BindingSkipReason>? excludedPackages = null)
{
    try
    {
        DiscoveryReport report = ClangAstDiscovery.Discover(
            new DiscoveryOptions(occtRoot, headers, toolkitByPackage));
        report = ApplyManualBindings(report, manualBindings ?? []);
        report = ApplyConfiguredExclusions(
            report,
            excludedBindings ?? new Dictionary<string, BindingSkipReason>(StringComparer.Ordinal),
            excludedPackages ?? new Dictionary<string, BindingSkipReason>(StringComparer.Ordinal));
        if (expectedOcctVersion is not null
            && !string.Equals(expectedOcctVersion, report.OcctVersion, StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"Configuration expects OCCT {expectedOcctVersion}, but the selected installation reports {report.OcctVersion}.");
        }
        await WriteJsonAsync(outputPath, report);
        Console.WriteLine(
            $"Discovered {report.Model.Declarations.Count} declarations from OCCT {report.OcctVersion}; report: '{Path.GetFullPath(outputPath)}'.");
        return 0;
    }
    catch (Exception error)
    {
        Console.Error.WriteLine(error.Message);
        return 1;
    }
}

static async Task WriteJsonAsync<T>(string outputPath, T value)
{
    string fullOutputPath = Path.GetFullPath(outputPath);
    string? directory = Path.GetDirectoryName(fullOutputPath);
    if (!string.IsNullOrEmpty(directory))
    {
        Directory.CreateDirectory(directory);
    }

    await File.WriteAllTextAsync(
        fullOutputPath,
        JsonSerializer.Serialize(value, CreateJsonOptions()) + Environment.NewLine);
}

static JsonSerializerOptions CreateJsonOptions()
{
    JsonSerializerOptions options = new() { WriteIndented = true };
    options.Converters.Add(new JsonStringEnumConverter());
    return options;
}

static bool IsSupportedConfigurationSchema(string schemaVersion) =>
    schemaVersion is "1.1" or "1.2" or "1.3" or "1.4" or "1.5" or "1.6" or "1.7" or "1.8" or "1.9";

static DiscoveryReport ApplyManualBindings(
    DiscoveryReport report,
    IReadOnlyList<ManualBindingConfiguration> manualBindings)
{
    BindingModel model = ManualBindingPass.Apply(report.Model, manualBindings);
    return report with
    {
        Model = model,
        Support = BindingSupportSummary.Create(model),
    };
}

static DiscoveryReport ApplyConfiguredExclusions(
    DiscoveryReport report,
    IReadOnlyDictionary<string, BindingSkipReason> exclusions,
    IReadOnlyDictionary<string, BindingSkipReason>? packageExclusions = null)
{
    BindingModel model = ConfiguredExclusionPass.Apply(report.Model, exclusions, packageExclusions);
    return report with
    {
        Model = model,
        Support = BindingSupportSummary.Create(model),
    };
}

static IReadOnlyDictionary<string, BindingSkipReason> GetExcludedAutoPackages(
    DiscoveryConfiguration configuration)
{
    Dictionary<string, BindingSkipReason> result = new(StringComparer.Ordinal);
    foreach (ExcludedAutoPackageConfiguration package in configuration.ExcludedAutoPackages)
    {
        if (package.SourcePackages.Count == 0
            || package.SourcePackages.Any(string.IsNullOrWhiteSpace)
            || string.IsNullOrWhiteSpace(package.Code)
            || string.IsNullOrWhiteSpace(package.Category)
            || string.IsNullOrWhiteSpace(package.Detail))
        {
            throw new InvalidDataException(
                "Every excluded auto package group must define sourcePackages, code, category, and detail.");
        }
        foreach (string sourcePackage in package.SourcePackages)
        {
            if (!result.TryAdd(
                    sourcePackage,
                    new BindingSkipReason(package.Code, package.Category, package.Detail)))
            {
                throw new InvalidDataException(
                    $"Excluded auto package '{sourcePackage}' is configured more than once.");
            }
        }
    }
    return result;
}

static IReadOnlyDictionary<string, BindingSkipReason> GetExcludedBindings(
    DiscoveryConfiguration configuration)
{
    Dictionary<string, BindingSkipReason> result = new(StringComparer.Ordinal);
    foreach (ExcludedBindingConfiguration binding in configuration.ExcludedBindings)
    {
        if (string.IsNullOrWhiteSpace(binding.StableId)
            || string.IsNullOrWhiteSpace(binding.Code)
            || string.IsNullOrWhiteSpace(binding.Category)
            || string.IsNullOrWhiteSpace(binding.Detail))
        {
            throw new InvalidDataException(
                "Every excluded binding must define stableId, code, category, and detail.");
        }
        if (!result.TryAdd(
                binding.StableId,
                new BindingSkipReason(binding.Code, binding.Category, binding.Detail)))
        {
            throw new InvalidDataException(
                $"Excluded binding stable ID '{binding.StableId}' is configured more than once.");
        }
    }

    HashSet<string> emitterExclusions = configuration.SharedHandleScopes
        .SelectMany(static scope => scope.ExcludedStableIds)
        .Concat(configuration.SharedHandlePackageScopes.SelectMany(static scope => scope.ExcludedStableIds))
        .ToHashSet(StringComparer.Ordinal);
    string[] missingDispositions = emitterExclusions.Except(result.Keys, StringComparer.Ordinal)
        .Order(StringComparer.Ordinal)
        .ToArray();
    string[] unusedDispositions = result.Keys.Except(emitterExclusions, StringComparer.Ordinal)
        .Order(StringComparer.Ordinal)
        .ToArray();
    if (missingDispositions.Length != 0 || unusedDispositions.Length != 0)
    {
        throw new InvalidDataException(
            $"Emitter exclusions and excluded binding dispositions differ. Missing dispositions: {string.Join(", ", missingDispositions)}; unused dispositions: {string.Join(", ", unusedDispositions)}.");
    }

    return result;
}

static IReadOnlyList<SharedHandlePackageScopeConfiguration> ExpandSharedHandlePackageScopes(
    BindingModel model,
    DiscoveryConfiguration configuration)
{
    if (!configuration.AutoSharedHandlePackageScopes)
    {
        return configuration.SharedHandlePackageScopes;
    }

    List<SharedHandlePackageScopeConfiguration> result = [.. configuration.SharedHandlePackageScopes];
    HashSet<string> identities = result
        .Select(static scope => scope.SourcePackage + "\u001f" + scope.NativeTypePrefix)
        .ToHashSet(StringComparer.Ordinal);
    foreach ((string package, string prefix) in model.Declarations
                 .Where(static declaration => declaration.Kind == BindingDeclarationKind.Record)
                 .Where(static declaration =>
                     !string.IsNullOrWhiteSpace(declaration.SourcePackage)
                     && declaration.NativeName.IndexOf('_', StringComparison.Ordinal) > 0)
                 .Select(static declaration => (
                     Package: declaration.SourcePackage,
                     Prefix: declaration.NativeName[..(declaration.NativeName.IndexOf('_', StringComparison.Ordinal) + 1)]))
                 .Distinct()
                 .OrderBy(static item => item.Package, StringComparer.Ordinal)
                 .ThenBy(static item => item.Prefix, StringComparer.Ordinal))
    {
        if (identities.Add(package + "\u001f" + prefix))
        {
            result.Add(new SharedHandlePackageScopeConfiguration
            {
                SourcePackage = package,
                NativeTypePrefix = prefix,
            });
        }
    }
    return result;
}

static int PrintUsage()
{
    Console.Error.WriteLine("Usage:");
    Console.Error.WriteLine("  OcctSharp.Generator model-smoke <output-path>");
    Console.Error.WriteLine("  OcctSharp.Generator discover --occt-root <path> --header <relative-header> --output <path>");
    Console.Error.WriteLine("  OcctSharp.Generator discover --occt-root <path> --config <generation.json> --output <path>");
    Console.Error.WriteLine("  OcctSharp.Generator generate --occt-root <path> --config <generation.json> --output-root <inner-workspace>");
    Console.Error.WriteLine("  OcctSharp.Generator inventory-catalog --occt-root <path> --output <path>");
    Console.Error.WriteLine("  OcctSharp.Generator inventory --occt-root <path> --config <generation.json> --output <path> --batch-size <count>");
    return 2;
}
