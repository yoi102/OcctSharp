using System.Text;
using System.Text.Json;
using OcctSharp.Generator.Discovery;
using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
using OcctSharp.Generator.TypeMapping;

namespace OcctSharp.Generator.Reporting;

public sealed record GenerationReportSet(
    GenerationCoverageReport Coverage,
    GenerationDiagnosticsReport Diagnostics);

public sealed record GenerationCoverageReport(
    string SchemaVersion,
    string OcctVersion,
    string BindingModelSchemaVersion,
    GenerationStateCounts Totals,
    IReadOnlyList<GenerationPackageCoverage> Packages);

public sealed record GenerationDiagnosticsReport(
    string SchemaVersion,
    string OcctVersion,
    string BindingModelSchemaVersion,
    IReadOnlyList<GenerationDeclarationDiagnostic> Declarations);

public sealed record GenerationStateCounts(
    int Total,
    int Pending,
    int Skipped,
    int Supported,
    int Manual,
    int Emitted);

public sealed record GenerationPackageCoverage(
    string SourcePackage,
    string? SourceToolkit,
    GenerationStateCounts States,
    IReadOnlyList<GenerationReasonCount> SkipReasons);

public sealed record GenerationReasonCount(string Code, int Count);

public sealed record GenerationDeclarationDiagnostic(
    string StableId,
    string NativeName,
    string NativeSignature,
    string Kind,
    string Header,
    int Line,
    int Column,
    string SourcePackage,
    string? SourceToolkit,
    string SupportState,
    bool IsEmitted,
    string Code,
    string Category,
    string Detail);

public static class GenerationReportWriter
{
    public const string CoverageRelativePath = "artifacts/generator-reports/coverage.json";
    public const string DiagnosticsRelativePath = "artifacts/generator-reports/diagnostics.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    public static GenerationReportSet Create(
        DiscoveryReport discovery,
        GeneratedBindingSet bindingSet)
    {
        ArgumentNullException.ThrowIfNull(discovery);
        ArgumentNullException.ThrowIfNull(bindingSet);

        HashSet<string> emittedIds = bindingSet.SourceStableIds.ToHashSet(StringComparer.Ordinal);
        string[] missingIds = emittedIds
            .Except(discovery.Model.Declarations.Select(static declaration => declaration.StableId), StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (missingIds.Length != 0)
        {
            throw new InvalidDataException(
                "The generated binding set references declarations outside the discovery model: "
                + string.Join(", ", missingIds));
        }

        return new GenerationReportSet(
            new GenerationCoverageReport(
                "1.0",
                discovery.OcctVersion,
                discovery.SchemaVersion,
                CountStates(discovery.Model.Declarations, emittedIds),
                BuildPackageCoverage(discovery.Model.Declarations, emittedIds)),
            new GenerationDiagnosticsReport(
                "1.0",
                discovery.OcctVersion,
                discovery.SchemaVersion,
                BuildDiagnostics(discovery.Model, emittedIds)));
    }

    public static void Write(string outputRoot, GenerationReportSet reports)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputRoot);
        ArgumentNullException.ThrowIfNull(reports);

        string fullOutputRoot = Path.GetFullPath(outputRoot);
        string stagingRoot = Path.Combine(
            fullOutputRoot,
            "artifacts",
            "generator-report-staging",
            Path.GetRandomFileName());
        Directory.CreateDirectory(stagingRoot);

        try
        {
            WriteJson(Path.Combine(stagingRoot, "coverage.json"), reports.Coverage);
            WriteJson(Path.Combine(stagingRoot, "diagnostics.json"), reports.Diagnostics);

            ReplaceFromStaging(
                Path.Combine(stagingRoot, "coverage.json"),
                Path.Combine(fullOutputRoot, ToPlatformPath(CoverageRelativePath)));
            ReplaceFromStaging(
                Path.Combine(stagingRoot, "diagnostics.json"),
                Path.Combine(fullOutputRoot, ToPlatformPath(DiagnosticsRelativePath)));
        }
        finally
        {
            if (Directory.Exists(stagingRoot))
            {
                Directory.Delete(stagingRoot, recursive: true);
            }
        }
    }

    private static GenerationPackageCoverage[] BuildPackageCoverage(
        IReadOnlyList<BindingDeclaration> declarations,
        HashSet<string> emittedIds)
    {
        return declarations
            .GroupBy(
                static declaration => (declaration.SourcePackage, declaration.SourceToolkit))
            .OrderBy(static group => group.Key.SourcePackage, StringComparer.Ordinal)
            .ThenBy(static group => group.Key.SourceToolkit, StringComparer.Ordinal)
            .Select(group => new GenerationPackageCoverage(
                group.Key.SourcePackage,
                group.Key.SourceToolkit,
                CountStates(group, emittedIds),
                group.Where(static declaration => declaration.SkipReason is not null)
                    .GroupBy(static declaration => declaration.SkipReason!.Code, StringComparer.Ordinal)
                    .OrderBy(static reason => reason.Key, StringComparer.Ordinal)
                    .Select(static reason => new GenerationReasonCount(reason.Key, reason.Count()))
                    .ToArray()))
            .ToArray();
    }

    private static GenerationDeclarationDiagnostic[] BuildDiagnostics(
        BindingModel model,
        HashSet<string> emittedIds)
    {
        InitialTypeMap typeMap = InitialTypeMap.FromModel(model);
        return model.Declarations
            .OrderBy(static declaration => declaration.StableId, StringComparer.Ordinal)
            .Select(declaration => CreateDiagnostic(declaration, emittedIds.Contains(declaration.StableId), typeMap))
            .ToArray();
    }

    private static GenerationDeclarationDiagnostic CreateDiagnostic(
        BindingDeclaration declaration,
        bool isEmitted,
        InitialTypeMap typeMap)
    {
        (string Code, string Category, string Detail) = isEmitted
            ? ("EM001", "Emitted", "The declaration is present in the generated binding set.")
            : declaration.SupportState switch
            {
                BindingSupportState.Skipped when declaration.SkipReason is not null => (
                    declaration.SkipReason.Code,
                    declaration.SkipReason.Category,
                    declaration.SkipReason.Detail),
                BindingSupportState.Supported => (
                    "EL000",
                    "Eligible",
                    "The declaration is eligible for the current value-copy emitter rules but is not selected for emission."),
                BindingSupportState.Manual => (
                    "MN001",
                    "Manual",
                    "The declaration is intentionally represented by a documented manual binding."),
                BindingSupportState.Pending => ToTuple(SimpleBindingEligibilityPass.Assess(declaration, typeMap)),
                _ => ("DG001", "Unknown", "The declaration has an unrecognized support state."),
            };

        return new GenerationDeclarationDiagnostic(
            declaration.StableId,
            declaration.NativeName,
            declaration.NativeSignature,
            declaration.Kind.ToString(),
            declaration.Header,
            declaration.Line,
            declaration.Column,
            declaration.SourcePackage,
            declaration.SourceToolkit,
            declaration.SupportState.ToString(),
            isEmitted,
            Code,
            Category,
            Detail);
    }

    private static (string Code, string Category, string Detail) ToTuple(
        SimpleBindingEligibilityAssessment assessment) =>
        (assessment.Code, assessment.Category, assessment.Detail);

    private static GenerationStateCounts CountStates(
        IEnumerable<BindingDeclaration> declarations,
        HashSet<string> emittedIds)
    {
        BindingDeclaration[] items = declarations.ToArray();
        return new GenerationStateCounts(
            items.Length,
            items.Count(static declaration => declaration.SupportState == BindingSupportState.Pending),
            items.Count(static declaration => declaration.SupportState == BindingSupportState.Skipped),
            items.Count(static declaration => declaration.SupportState == BindingSupportState.Supported),
            items.Count(static declaration => declaration.SupportState == BindingSupportState.Manual),
            items.Count(declaration => emittedIds.Contains(declaration.StableId)));
    }

    private static void WriteJson<T>(string path, T value)
    {
        string content = JsonSerializer.Serialize(value, JsonOptions) + "\n";
        File.WriteAllText(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static void ReplaceFromStaging(string source, string destination)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.Copy(source, destination, overwrite: true);
    }

    private static string ToPlatformPath(string relativePath) =>
        relativePath.Replace('/', Path.DirectorySeparatorChar);
}
