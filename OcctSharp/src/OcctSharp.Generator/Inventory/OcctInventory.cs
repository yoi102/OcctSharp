using System.Text.RegularExpressions;
using OcctSharp.Generator.Discovery;
using OcctSharp.Generator.Model;

namespace OcctSharp.Generator.Inventory;

public static partial class OcctInventory
{
    private const string ParserName = "ClangSharp/libClangSharp 21.1.8";

    public static OcctInventoryReport CreateCatalog(string occtRoot)
    {
        InventoryContext context = CreateContext(occtRoot);
        return CreateReport(
            context,
            semanticScan: false,
            isComplete: false,
            batchSize: 0,
            successfulHeaders: [],
            declarations: [],
            batches: [],
            failures: []);
    }

    public static OcctInventoryReport Discover(
        string occtRoot,
        int batchSize,
        IReadOnlyDictionary<string, string>? toolkitByPackage = null,
        IReadOnlyList<string>? preambleHeaders = null,
        IReadOnlySet<string>? emittedStableIds = null,
        IReadOnlySet<string>? manualStableIds = null,
        Action<string>? progress = null)
    {
        if (batchSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(batchSize), "The inventory batch size must be positive.");
        }

        InventoryContext context = CreateContext(occtRoot);
        Dictionary<string, BindingDeclaration> declarations = new(StringComparer.Ordinal);
        HashSet<string> successfulHeaders = new(StringComparer.Ordinal);
        List<OcctInventoryBatch> batches = [];
        List<OcctInventoryFailure> failures = [];
        int plannedBatchCount = (context.Headers.Length + batchSize - 1) / batchSize;

        for (int offset = 0, plannedBatch = 1; offset < context.Headers.Length; offset += batchSize, plannedBatch++)
        {
            string[] headers = context.Headers.Skip(offset).Take(batchSize).ToArray();
            progress?.Invoke(
                $"Inventory batch {plannedBatch}/{plannedBatchCount}: headers {offset + 1}-{offset + headers.Length} of {context.Headers.Length}.");
            DiscoverWithIsolation(
                context.OcctRoot,
                headers,
                toolkitByPackage,
                preambleHeaders,
                successfulHeaders,
                declarations,
                batches,
                failures,
                progress);
        }

        return CreateReport(
            context,
            semanticScan: true,
            isComplete: failures.Count == 0 && successfulHeaders.Count == context.Headers.Length,
            batchSize,
            successfulHeaders,
            declarations.Values,
            batches,
            failures,
            emittedStableIds,
            manualStableIds);
    }

    private static void DiscoverWithIsolation(
        string occtRoot,
        string[] headers,
        IReadOnlyDictionary<string, string>? toolkitByPackage,
        IReadOnlyList<string>? preambleHeaders,
        HashSet<string> successfulHeaders,
        Dictionary<string, BindingDeclaration> declarations,
        List<OcctInventoryBatch> batches,
        List<OcctInventoryFailure> failures,
        Action<string>? progress)
    {
        try
        {
            DiscoveryReport report = ClangAstDiscovery.Discover(
                new DiscoveryOptions(occtRoot, headers, toolkitByPackage)
                {
                    PreambleHeaders = preambleHeaders ?? [],
                });
            foreach (string header in headers)
            {
                successfulHeaders.Add(header);
            }

            foreach (BindingDeclaration declaration in report.Model.Declarations)
            {
                if (!declarations.TryGetValue(declaration.StableId, out BindingDeclaration? current)
                    || GetFactScore(declaration) > GetFactScore(current))
                {
                    declarations[declaration.StableId] = declaration;
                }
            }

            batches.Add(new OcctInventoryBatch(
                batches.Count + 1,
                headers,
                report.Model.Declarations.Count,
                report.Diagnostics.Count));
        }
        catch (Exception) when (headers.Length > 1)
        {
            int middle = headers.Length / 2;
            progress?.Invoke($"Inventory batch failed; isolating {headers.Length} headers into smaller deterministic batches.");
            DiscoverWithIsolation(
                occtRoot,
                headers[..middle],
                toolkitByPackage,
                preambleHeaders,
                successfulHeaders,
                declarations,
                batches,
                failures,
                progress);
            DiscoverWithIsolation(
                occtRoot,
                headers[middle..],
                toolkitByPackage,
                preambleHeaders,
                successfulHeaders,
                declarations,
                batches,
                failures,
                progress);
        }
        catch (Exception error)
        {
            string message = TemporarySourceRegex()
                .Replace(error.Message, "<translation-unit>.cpp")
                .Replace(occtRoot, "<occt-root>", StringComparison.OrdinalIgnoreCase);
            failures.Add(new OcctInventoryFailure(headers[0], message));
            string summary = message.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)[0];
            progress?.Invoke($"Inventory skipped '{headers[0]}': {summary}");
        }
    }

    private static OcctInventoryReport CreateReport(
        InventoryContext context,
        bool semanticScan,
        bool isComplete,
        int batchSize,
        IEnumerable<string> successfulHeaders,
        IEnumerable<BindingDeclaration> declarations,
        IEnumerable<OcctInventoryBatch> batches,
        IEnumerable<OcctInventoryFailure> failures,
        IReadOnlySet<string>? emittedStableIds = null,
        IReadOnlySet<string>? manualStableIds = null)
    {
        string[] scanned = successfulHeaders.Order(StringComparer.Ordinal).ToArray();
        BindingDeclaration[] declarationArray = declarations
            .OrderBy(static declaration => declaration.StableId, StringComparer.Ordinal)
            .ToArray();
        OcctInventoryFailure[] failureArray = failures
            .OrderBy(static failure => failure.Header, StringComparer.Ordinal)
            .ToArray();

        return new OcctInventoryReport(
            "1.0",
            context.OcctVersion,
            ParserName,
            semanticScan,
            isComplete,
            batchSize,
            new OcctHeaderInventory(
                context.Headers.Length,
                context.Headers.Count(static header => Path.GetExtension(header).Equals(".h", StringComparison.OrdinalIgnoreCase)),
                context.Headers.Count(static header => Path.GetExtension(header).Equals(".hxx", StringComparison.OrdinalIgnoreCase)),
                scanned.Length,
                failureArray.Length,
                BuildHeaderPackages(context.Headers)),
            semanticScan ? BuildDeclarationInventory(declarationArray) : null,
            batches.OrderBy(static batch => batch.Sequence).ToArray(),
            failureArray,
            semanticScan
                ? LongTailClassification.Create(
                    declarationArray,
                    context.Headers,
                    scanned.ToHashSet(StringComparer.Ordinal),
                    failureArray,
                    emittedStableIds,
                    manualStableIds)
                : null);
    }

    private static OcctPackageHeaderInventory[] BuildHeaderPackages(IEnumerable<string> headers) =>
        headers
            .GroupBy(GetSourcePackage, StringComparer.Ordinal)
            .OrderBy(static group => group.Key, StringComparer.Ordinal)
            .Select(static group => new OcctPackageHeaderInventory(
                group.Key,
                group.Count(),
                group.Count(static header => Path.GetExtension(header).Equals(".h", StringComparison.OrdinalIgnoreCase)),
                group.Count(static header => Path.GetExtension(header).Equals(".hxx", StringComparison.OrdinalIgnoreCase))))
            .ToArray();

    private static OcctDeclarationInventory BuildDeclarationInventory(BindingDeclaration[] declarations) =>
        new(
            declarations.Length,
            declarations.Count(static declaration => declaration.SupportState == BindingSupportState.Pending),
            declarations.Count(static declaration => declaration.SupportState == BindingSupportState.Skipped),
            declarations.Count(static declaration => declaration.SupportState == BindingSupportState.Supported),
            declarations.Count(static declaration => declaration.SupportState == BindingSupportState.Manual),
            declarations
                .GroupBy(static declaration => (declaration.SourcePackage, declaration.SourceToolkit))
                .OrderBy(static group => group.Key.SourcePackage, StringComparer.Ordinal)
                .ThenBy(static group => group.Key.SourceToolkit, StringComparer.Ordinal)
                .Select(static group => new OcctPackageDeclarationInventory(
                    group.Key.SourcePackage,
                    group.Key.SourceToolkit,
                    group.Count(),
                    group.Count(static declaration => declaration.SupportState == BindingSupportState.Pending),
                    group.Count(static declaration => declaration.SupportState == BindingSupportState.Skipped),
                    group.Count(static declaration => declaration.SupportState == BindingSupportState.Supported),
                    group.Count(static declaration => declaration.SupportState == BindingSupportState.Manual)))
                .ToArray());

    private static InventoryContext CreateContext(string occtRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(occtRoot);
        string fullOcctRoot = Path.GetFullPath(occtRoot);
        string includeRoot = Path.Combine(fullOcctRoot, "inc");
        string versionHeader = Path.Combine(includeRoot, "Standard_Version.hxx");
        if (!Directory.Exists(includeRoot) || !File.Exists(versionHeader))
        {
            throw new DirectoryNotFoundException(
                $"'{fullOcctRoot}' is not an OCCT installation with inc/Standard_Version.hxx.");
        }

        string[] headers = Directory.EnumerateFiles(includeRoot, "*", SearchOption.TopDirectoryOnly)
            .Where(static path => Path.GetExtension(path) is ".h" or ".hxx")
            .Select(Path.GetFileName)
            .Where(static fileName => fileName is not null)
            .Select(static fileName => fileName!)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return new InventoryContext(fullOcctRoot, ReadOcctVersion(versionHeader), headers);
    }

    private static string ReadOcctVersion(string versionHeader)
    {
        const string prefix = "#define OCC_VERSION_COMPLETE \"";
        string? line = File.ReadLines(versionHeader)
            .FirstOrDefault(static line => line.StartsWith(prefix, StringComparison.Ordinal));
        if (line is null || !line.EndsWith('"'))
        {
            throw new InvalidDataException("OCC_VERSION_COMPLETE was not found in Standard_Version.hxx.");
        }

        return line[prefix.Length..^1];
    }

    private static string GetSourcePackage(string header)
    {
        string fileName = Path.GetFileNameWithoutExtension(header);
        int separator = fileName.IndexOf('_');
        return separator > 0 ? fileName[..separator] : fileName;
    }

    private static int GetFactScore(BindingDeclaration declaration) =>
        (declaration.ReturnType is null ? 0 : 1000)
        + (declaration.Parameters.Count * 100)
        + (declaration.BaseTypes.Count * 10)
        + (declaration.Access == BindingAccess.None ? 0 : 1);

    [GeneratedRegex(@"occtsharp-[0-9a-fA-F]{32}\.cpp", RegexOptions.CultureInvariant)]
    private static partial Regex TemporarySourceRegex();

    private sealed record InventoryContext(string OcctRoot, string OcctVersion, string[] Headers);
}
