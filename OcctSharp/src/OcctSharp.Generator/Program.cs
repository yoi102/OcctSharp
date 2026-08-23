using System.Text.Json;
using System.Text.Json.Serialization;
using OcctSharp.Generator.Discovery;
using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Inventory;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Reporting;

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
        => await RunInventoryAsync(occtRoot, configPath, outputPath, batchSize),
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
    string batchSizeText)
{
    try
    {
        if (!int.TryParse(batchSizeText, out int batchSize) || batchSize < 1)
        {
            throw new ArgumentException($"Inventory batch size '{batchSizeText}' is not a positive integer.");
        }

        DiscoveryConfiguration configuration = await ReadConfigurationAsync(configPath);
        OcctInventoryReport report = OcctInventory.Discover(
            occtRoot,
            batchSize,
            configuration.ToolkitByPackage,
            configuration.InventoryPreambleHeaders,
            Console.WriteLine);
        if (!string.Equals(configuration.OcctVersion, report.OcctVersion, StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"Configuration expects OCCT {configuration.OcctVersion}, but the selected installation reports {report.OcctVersion}.");
        }

        await WriteJsonAsync(outputPath, report);
        Console.WriteLine(
            $"Inventoried {report.Headers.Scanned}/{report.Headers.Total} headers and {report.Declarations?.Total ?? 0} unique declarations; complete: {report.IsComplete}; report: '{Path.GetFullPath(outputPath)}'.");
        return report.IsComplete ? 0 : 3;
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
            configuration.Headers,
            configuration.OcctVersion,
            outputPath,
            configuration.ToolkitByPackage);
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
            new DiscoveryOptions(occtRoot, configuration.Headers, configuration.ToolkitByPackage));
        if (!string.Equals(configuration.OcctVersion, report.OcctVersion, StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"Configuration expects OCCT {configuration.OcctVersion}, but the selected installation reports {report.OcctVersion}.");
        }

        GeneratedBindingSet bindingSet = InitialBindingEmitter.Emit(
            report,
            configuration.GenerationScopes,
            configuration.SharedHandleScopes,
            configuration.TopologyScopes);
        GeneratedManifest manifest = GeneratedOutputWriter.Replace(outputRoot, bindingSet);
        GenerationReportSet reports = GenerationReportWriter.Create(report, bindingSet);
        GenerationReportWriter.Write(outputRoot, reports);
        Console.WriteLine(
            $"Generated {manifest.Files.Count} files from {manifest.SourceStableIds.Count} binding into '{Path.GetFullPath(outputRoot)}'.");
        Console.WriteLine(
            $"Wrote generation coverage and diagnostics for {reports.Coverage.Totals.Total} declarations.");
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

static async Task<int> RunDiscoveryAsync(
    string occtRoot,
    IReadOnlyList<string> headers,
    string? expectedOcctVersion,
    string outputPath,
    IReadOnlyDictionary<string, string>? toolkitByPackage = null)
{
    try
    {
        DiscoveryReport report = ClangAstDiscovery.Discover(
            new DiscoveryOptions(occtRoot, headers, toolkitByPackage));
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
    schemaVersion is "1.1" or "1.2" or "1.3" or "1.4";

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
