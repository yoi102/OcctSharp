using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
using OcctSharp.Generator.TypeMapping;

namespace OcctSharp.Generator.Inventory;

public static class LongTailClassification
{
    public static OcctFinalClassification Create(
        IReadOnlyList<BindingDeclaration> declarations,
        IReadOnlyList<string> allHeaders,
        IReadOnlySet<string> successfulHeaders,
        IReadOnlyList<OcctInventoryFailure> failures,
        IReadOnlySet<string>? emittedStableIds = null,
        IReadOnlySet<string>? manualStableIds = null)
    {
        ArgumentNullException.ThrowIfNull(declarations);
        ArgumentNullException.ThrowIfNull(allHeaders);
        ArgumentNullException.ThrowIfNull(successfulHeaders);
        ArgumentNullException.ThrowIfNull(failures);

        ValidateKnownStableIds(declarations, emittedStableIds, manualStableIds);
        InitialTypeMap typeMap = InitialTypeMap.FromModel(new BindingModel(declarations));
        OcctDeclarationDisposition[] declarationDispositions = declarations
            .OrderBy(static declaration => declaration.StableId, StringComparer.Ordinal)
            .Select(declaration => ClassifyDeclaration(declaration, typeMap, emittedStableIds, manualStableIds))
            .ToArray();

        Dictionary<string, OcctInventoryFailure> failureByHeader = failures
            .ToDictionary(static failure => failure.Header, StringComparer.Ordinal);
        OcctHeaderDisposition[] headerDispositions = allHeaders
            .Order(StringComparer.Ordinal)
            .Select(header => successfulHeaders.Contains(header)
                ? new OcctHeaderDisposition(header, "Parsed", "HD000", "Parsed")
                : failureByHeader.TryGetValue(header, out OcctInventoryFailure? failure)
                    ? ClassifyHeaderFailure(failure)
                    : new OcctHeaderDisposition(header, "Pending", "HD999", "UnaccountedHeader"))
            .ToArray();

        int declarationPending = declarationDispositions.Count(static item => item.State == "Pending");
        int headerPending = headerDispositions.Count(static item => item.State == "Pending");
        return new OcctFinalClassification(
            declarationPending == 0 && headerPending == 0,
            declarationDispositions.Length,
            declarationDispositions.Length - declarationPending,
            declarationPending,
            CountStates(declarationDispositions.Select(static item => item.State)),
            CountReasons(declarationDispositions.Select(static item => (item.Code, item.Category))),
            headerDispositions.Length,
            headerDispositions.Length - headerPending,
            headerPending,
            CountStates(headerDispositions.Select(static item => item.State)),
            CountReasons(headerDispositions.Select(static item => (item.Code, item.Category))),
            declarationDispositions,
            headerDispositions);
    }

    private static OcctDeclarationDisposition ClassifyDeclaration(
        BindingDeclaration declaration,
        InitialTypeMap typeMap,
        IReadOnlySet<string>? emittedStableIds,
        IReadOnlySet<string>? manualStableIds)
    {
        if (emittedStableIds?.Contains(declaration.StableId) == true
            && manualStableIds?.Contains(declaration.StableId) == true)
        {
            throw new InvalidDataException(
                $"Declaration '{declaration.StableId}' cannot be both emitted and manual.");
        }

        (string State, string Code, string Category) disposition = emittedStableIds?.Contains(declaration.StableId) == true
            ? ("Emitted", "EM001", "GeneratedBinding")
            : manualStableIds?.Contains(declaration.StableId) == true
                ? ("Manual", "MN001", "ManualBinding")
            : declaration.SupportState switch
        {
            BindingSupportState.Supported => ("SupportedUnselected", "LT000", "EligibleUnselected"),
            BindingSupportState.Manual => ("Manual", "MN001", "ManualBinding"),
            BindingSupportState.Skipped when declaration.SkipReason is not null =>
                ("Skipped", declaration.SkipReason.Code, declaration.SkipReason.Category),
            BindingSupportState.Pending => ClassifyPending(declaration, typeMap),
            _ => ("Pending", "LT999", "UnrecognizedSupportState"),
        };

        return new OcctDeclarationDisposition(
            declaration.StableId,
            declaration.NativeName,
            declaration.Kind.ToString(),
            declaration.Header,
            declaration.SourcePackage,
            declaration.SourceToolkit,
            disposition.State,
            disposition.Code,
            disposition.Category);
    }

    private static void ValidateKnownStableIds(
        IReadOnlyList<BindingDeclaration> declarations,
        IReadOnlySet<string>? emittedStableIds,
        IReadOnlySet<string>? manualStableIds)
    {
        HashSet<string> discovered = declarations
            .Select(static declaration => declaration.StableId)
            .ToHashSet(StringComparer.Ordinal);
        string[] unknown = (emittedStableIds ?? new HashSet<string>(StringComparer.Ordinal))
            .Concat(manualStableIds ?? new HashSet<string>(StringComparer.Ordinal))
            .Where(stableId => !discovered.Contains(stableId))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (unknown.Length != 0)
        {
            throw new InvalidDataException(
                $"Manifest stable IDs were not found in the inventory: {string.Join(", ", unknown)}.");
        }
    }

    private static (string State, string Code, string Category) ClassifyPending(
        BindingDeclaration declaration,
        InitialTypeMap typeMap)
    {
        SimpleBindingEligibilityAssessment assessment = SimpleBindingEligibilityPass.Assess(declaration, typeMap);
        return assessment.Code switch
        {
            "EL001" => ("Blocked", "LT001", "DeclarationProjection"),
            "EL002" => ("Blocked", "LT002", "InstanceOwnership"),
            "EL003" => ("Blocked", "LT003", "ReturnProjection"),
            "EL004" => ("Blocked", "LT004", "ParameterProjection"),
            _ => ("Blocked", "LT099", "UnimplementedGeneralRule"),
        };
    }

    private static OcctHeaderDisposition ClassifyHeaderFailure(OcctInventoryFailure failure)
    {
        string header = failure.Header;
        if (header.StartsWith("IVtk", StringComparison.Ordinal))
        {
            return new OcctHeaderDisposition(header, "BlockedExternalDependency", "HD001", "MissingVtk");
        }
        if (header.Equals("OpenGl_GLESExtensions.hxx", StringComparison.Ordinal))
        {
            return new OcctHeaderDisposition(header, "BlockedExternalDependency", "HD002", "MissingEglGlesContext");
        }
        if (header.Equals("RWGltf_GltfOStreamWriter.hxx", StringComparison.Ordinal))
        {
            return new OcctHeaderDisposition(header, "BlockedExternalDependency", "HD003", "MissingRapidJson");
        }
        if (header.Equals("NCollection_Haft.h", StringComparison.Ordinal))
        {
            return new OcctHeaderDisposition(header, "ExcludedLanguage", "HD004", "CppCliOnly");
        }
        if (failure.Error.Contains("file not found", StringComparison.Ordinal))
        {
            return new OcctHeaderDisposition(header, "UnavailableInArtifact", "HD005", "MissingOcctGeneratedHeader");
        }
        return new OcctHeaderDisposition(header, "Blocked", "HD099", "IsolatedParseFailure");
    }

    private static OcctDispositionCount[] CountStates(IEnumerable<string> states) => states
        .GroupBy(static state => state, StringComparer.Ordinal)
        .OrderBy(static group => group.Key, StringComparer.Ordinal)
        .Select(static group => new OcctDispositionCount(group.Key, group.Count()))
        .ToArray();

    private static OcctReasonCount[] CountReasons(IEnumerable<(string Code, string Category)> reasons) => reasons
        .GroupBy(static reason => reason, EqualityComparer<(string Code, string Category)>.Default)
        .OrderBy(static group => group.Key.Code, StringComparer.Ordinal)
        .ThenBy(static group => group.Key.Category, StringComparer.Ordinal)
        .Select(static group => new OcctReasonCount(group.Key.Code, group.Key.Category, group.Count()))
        .ToArray();
}
